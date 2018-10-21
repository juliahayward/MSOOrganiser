using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

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
                order2018.Timestamp = order.timestamp.Value;
                order2018.Attendees.Add(new Order2018.Attendee()
                {
                    Title = order.attendees.customer.title.Value,
                    FirstName = order.attendees.customer.first_name.Value,
                    LastName = order.attendees.customer.last_name.Value,
                    CountryCode = order.attendees.customer.country_to_represent.Value,
                    Email = order.attendees.customer.email.Value
                });
                foreach (var o1 in order.events)
                {
                    var evt = o1.Value;
                    order2018.Events.Add(new Order2018.Event()
                    {
                        Code = evt.event_id.Value
                    });
                }
                orders.Add(order2018);
            }

            return orders;
        }

        public void ProcessAll(IEnumerable<Order2018> orders)
        {
            var context = DataEntitiesProvider.Provide();
            var olympiad = context.Olympiad_Infoes.First(x => x.Current);
            var countries = context.Nationalities
                .Where(x => x.ISO2 != null)
                .ToDictionary(n => n.ISO2, n => n.Name);
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
                        contestant = CreateEntrant(context, order.DateOfBirth, attendee, countries);

                    bool newEventFound = false;
                    foreach (var evt in order.Events)
                    {
                        // Shouldn't happen post-fix by Vika
                        if (evt.Code == "")
                            continue;
                        if (contestant.Entrants.Any(x => x.Game_Code == evt.Code && x.OlympiadId == olympiad.Id))
                            continue;

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
            DateTime? dob,
            Order2018.Attendee entrant,
            Dictionary<string, string> countries)
        {
            var contestant = new Contestant()
            {
                Firstname = entrant.FirstName,
                Lastname = entrant.LastName,
                Nationality = countries[entrant.CountryCode],
                Title = entrant.Title,
                Male = (entrant.Title == "Mr"),
                DateofBirth = dob
            };

            context.Contestants.Add(contestant);

            context.SaveChanges();
            return contestant;
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
