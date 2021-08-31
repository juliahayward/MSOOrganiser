using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using MSOCore.Extensions;

namespace MSOCore.Calculators
{
    public class PaymentProcessor2018
    {
        public IEnumerable<Order2018> ParseJsonFile(string file)
        {
            var orders = new List<Order2018>();

            dynamic json = JsonConvert.DeserializeObject(File.ReadAllText(file));
            foreach (var o in json)
            {
                var order2018 = new Order2018();
                var order = o.Value;
                order2018.BookingId = order.booking_id.Value;
                order2018.BookingPrice = decimal.Parse(order.booking_price.Value);
                order2018.BookingSpaces = order.booking_spaces.Value;
                order2018.BookingStatus = order.booking_status.Value;
                order2018.DiscordNickname = order["Discord username"].Value;
                order2018.OnlineNicknames = order["Usernames for relevant platforms"].Value;
                order2018.Whatsapp = (order["WhatsApp with this number"] != null && order["WhatsApp with this number"].Value == "1");
                order2018.Timestamp = order.timestamp.Value;
                order2018.DoBString = order.date_of_birth.Value;
                var attendee = new Order2018.Attendee()
                {
                    Title = order.attendees.customer.title.Value,
                    FirstName = order.attendees.customer.first_name.Value,
                    LastName = order.attendees.customer.last_name.Value,
                    CountryCode = SubstituteCountryCode(order.attendees.customer.country_to_represent.Value),
                    Email = order.attendees.customer.email.Value, 
                };
                attendee.FirstName = attendee.FirstName.DecodeEncodedNonAsciiCharacters();
                attendee.LastName = attendee.LastName.DecodeEncodedNonAsciiCharacters();
                order2018.Attendees.Add(attendee);
                foreach (var o1 in order.events)
                {
                    var evt = o1.Value;
                    order2018.Events.Add(new Order2018.Event()
                    {
                        Code = Substitute(evt.event_id.Value)
                    });
                }
                orders.Add(order2018);
            }

            return orders;
        }

        private string SubstituteCountryCode(string countryCode)
        {
            if (countryCode == "cy-GB") return "cy";

            return countryCode;
        }

        private string Substitute(string eventCode)
        {
            // cope with mismatches in 2019
            if (eventCode == "CHCO") return "CLWC";

            return eventCode;
        }

        public void ProcessAll(IEnumerable<Order2018> orders)
        {
            var context = DataEntitiesProvider.Provide();
            var olympiad = context.Olympiad_Infoes.First(x => x.Current);
            var countries = context.Nationalities
                .Where(x => x.ISO2 != null)
                .ToDictionary(n => n.ISO2, n => n.Name);
            countries.Add("", "");
            var events = context.Events.Where(x => x.OlympiadId == olympiad.Id)
                .ToDictionary(e => e.Code, e => e);
            var entryFees = context.Fees.ToDictionary(x => x.Code, x => x);

            foreach (var order in orders)
            {
                foreach (var attendee in order.Attendees)
                {
                    var contestant = context.Contestants.FirstOrDefault(
                        x => x.Firstname.ToLower() == attendee.FirstName.ToLower()
                        && x.Lastname.ToLower() == attendee.LastName.ToLower());

                    if (contestant == null)
                        contestant = CreateEntrant(context, order, attendee, countries);
                    else
                        UpdateEntrant(context, contestant, order, attendee.Email);

                    bool newEventFound = false;
                    foreach (var evt in order.Events)
                    {
                        // Shouldn't happen post-fix by Vika
                        if (evt.Code == "")
                            continue;
                        // yes the first clause is necessary
                        if (contestant.Entrants.Any(x => x.Event != null && x.Event.Code == evt.Code && x.OlympiadId == olympiad.Id))
                            continue;

                        if (!events.ContainsKey(evt.Code))
                            throw new ArgumentOutOfRangeException($"Event code {evt.Code} not recognised");
                        if (events[evt.Code].Entry_Fee == null)
                            throw new ArgumentNullException($"Event code {evt.Code} has no entry fee specified");

                        // Put the contestant into the right event
                        var entrant = Entrant.NewEntrant(events[evt.Code].EIN, evt.Code, olympiad.Id, contestant,
                            entryFees[events[evt.Code].Entry_Fee].Adult.Value);
                        contestant.Entrants.Add(entrant);
                        newEventFound = true;
                    }

                    if (newEventFound)
                    {
                        contestant.Payments.Add(new Payment()
                        {
                            Banked = 1,
                            MindSportsID = contestant.Mind_Sport_ID,
                            OlympiadId = olympiad.Id,
                            Payment_Method = "MSO Website",
                            Payment1 = order.BookingPrice,
                            Year = olympiad.StartDate.Value.Year,
                            Received = DateTime.Now
                        });
                    }

                    context.SaveChanges();
                }
                // TODO figure out payments - and juniors, not in data - and early birds
            }

            var param = context.Parameters.First(x => x.Id == 1);
            // MSO happens in GMT = UTC+1 (hack)
            param.Value = DateTime.UtcNow.AddHours(1).ToString("dd MMM yyyy, HH:mm");
            context.SaveChanges();
        }

        public void ExtractEmails(IEnumerable<Order2018> orders, string filename)
        {
            using (var sw = new StreamWriter(new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write)))
            {
                foreach (var order in orders)
                {
                    foreach (var attendee in order.Attendees)
                    {
                        if (attendee.Email != null)
                        {
                            var line = string.Format("{0},{1},{2}",
                                attendee.FirstName, attendee.LastName, attendee.Email);
                            sw.WriteLine(line);
                        }
                    }
                }
            }
        }

        private Contestant CreateEntrant(DataEntities context, 
            Order2018 order,
            Order2018.Attendee entrant,
            Dictionary<string, string> countries)
        {
            if (!countries.ContainsKey(entrant.CountryCode))
                throw new ArgumentOutOfRangeException($"Unknown country code {entrant.CountryCode}");

            var contestant = new Contestant()
            {
                Firstname = entrant.FirstName ?? "",
                Lastname = entrant.LastName ?? "",
                Nationality = countries[entrant.CountryCode],
                Title = entrant.Title,
                Male = (entrant.Title == "Mr"),
                DateofBirth = SanitiseDateOfBirth(order.DateOfBirth),
                OnlineNicknames = order.OnlineNicknames,
                DiscordNickname = order.DiscordNickname,
                Whatsapp = order.Whatsapp,
                email = entrant.Email
            };

            context.Contestants.Add(contestant);

            context.SaveChanges();
            return contestant;
        }

        private void UpdateEntrant(DataEntities context, Contestant contestant, Order2018 order, string email)
        {
            if (IsSanitisedDateOfBirth(order.DateOfBirth))
                contestant.DateofBirth = order.DateOfBirth;
            if (order.OnlineNicknames != null)
            {
                // Contestants may use different nicknames for different servers, and submit separate orders
                if (contestant.OnlineNicknames == null)
                    contestant.OnlineNicknames = order.OnlineNicknames;
                else if (!contestant.OnlineNicknames.Contains(order.OnlineNicknames))
                    contestant.OnlineNicknames += "; " + order.OnlineNicknames;
            }
            if (order.DiscordNickname != null)
                contestant.DiscordNickname = order.DiscordNickname;
            contestant.Whatsapp = order.Whatsapp;
            if (!string.IsNullOrEmpty(email))
                contestant.email = email;
            if (!string.IsNullOrEmpty(email))
                contestant.email = email;

            context.SaveChanges();
        }

        public DateTime? SanitiseDateOfBirth(DateTime? order_dob)
        {
            if (!order_dob.HasValue)
                return order_dob;
            // Catch people who put in today's date
            if (DateTime.Now.AddYears(-2) < order_dob)
                return null;
            // Catch people who put in silly old values
            if (DateTime.Now.AddYears(-120) < order_dob)
                return null;

            return order_dob;
        }

        public bool IsSanitisedDateOfBirth(DateTime? order_dob)
        {
            if (!order_dob.HasValue)
                return true;
            // Catch people who put in today's date
            if (DateTime.Now.AddYears(-2) < order_dob)
                return false;
            // Catch people who put in silly old values
            if (DateTime.Now.AddYears(-120) < order_dob)
                return false;

            return true;
        }
    }

    public class Order2018
    {
        public string BookingId { get; set; }
        public decimal BookingPrice { get; set; }
        public long BookingSpaces { get; set; }
        public string BookingStatus { get; set; }
        public long Timestamp { get; set; }
        public string DoBString { get; set; }
        public string OnlineNicknames { get; set; }
        public string DiscordNickname { get; set; }

        public bool Whatsapp { get; set; }
        public DateTime? DateOfBirth { get {
                DateTime dob;
                var success = DateTime.TryParseExact(DoBString, "yyyy-MM-dd", CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dob);
                return (success) ? dob : (DateTime?)null;
            }
        }
        public List<Attendee> Attendees { get; private set; }
        public List<Event> Events { get; private set; }

        public Order2018()
        {
            Attendees = new List<Attendee>();
            Events = new List<Event>();
        }

        public class Attendee
        {
            public string Title { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string CountryCode { get; set; }
            public string Email { get; set; }
        }

        public class Event
        {
            public string Code { get; set; }
        }
    }
}
