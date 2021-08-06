using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using Microsoft.VisualBasic.FileIO;
using MSOCore.Extensions;

namespace MSOCore.Calculators
{
    public class PaymentProcessor2021
    {
        public IEnumerable<Order2021> ParseCsvFile(string file)
        {
            TextFieldParser parser = new TextFieldParser(file);
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");
            while (!parser.EndOfData)
            {
                string[] fields = parser.ReadFields();
                if (fields[0] == "ID") // header
                    continue;

                var order = new Order2021()
                {
                    WordpressId = fields[0],
                    // Oh designers, please use "normal" characters as much as possible!!
                    EventName = fields[1].Replace("’", "'").Replace("×", "x"), 
                    FirstName = fields[9],
                    Email = fields[10],
                    Title = fields[14],
                    LastName = fields[15],
                    Phone = fields[16],
                    LocalisedNationality = fields[17],
                    OnlineNickname = fields[18],
                    DiscordNickname = fields[19],
                    DoBString = fields[20]
                };
                yield return order;
            }
            parser.Close();
        }

        public void ProcessAll(IEnumerable<Order2021> orders)
        {
            var context = DataEntitiesProvider.Provide();
            var olympiad = context.Olympiad_Infoes.First(x => x.Current);
            var events = context.Events.Where(x => x.OlympiadId == olympiad.Id)
                .ToDictionary(e => e.Code, e => e);
            var eventsByName = context.Events.Where(x => x.OlympiadId == olympiad.Id)
                .ToDictionary(e => e.Mind_Sport, e => e);
            var entryFees = context.Fees.ToDictionary(x => x.Code, x => x);

            foreach (var order in orders)
            {
                var contestant = context.Contestants.FirstOrDefault(
                        x => (x.Firstname.ToLower() == order.FirstName.ToLower()
                             && x.Lastname.ToLower() == order.LastName.ToLower()
                             && (x.email == null || x.email.ToLower() == order.Email.ToLower())));

                if (contestant == null)
                    contestant = CreateEntrant(context, order);
                else
                    UpdateEntrant(context, contestant, order);

                // because some entered both qualifier and finals
                if (order.EventName.StartsWith("Mental Calculations"))
                    order.EventName = "Mental Calculations WC";

                // Already entered?
                if (contestant.Entrants.Any(x =>
                    x.Event != null && x.Event.Mind_Sport == order.EventName && x.OlympiadId == olympiad.Id))
                    continue;

                if (!eventsByName.ContainsKey(order.EventName))
                    throw new ArgumentOutOfRangeException($"Event name {order.EventName} not recognised");
                if (eventsByName[order.EventName].Entry_Fee == null)
                    throw new ArgumentNullException($"Event name {order.EventName} has no entry fee specified");

                // Put the contestant into the right event
                Event evt = eventsByName[order.EventName];
                var entrant = Entrant.NewEntrant(events[evt.Code].EIN, evt.Code, olympiad.Id, contestant, entryFees[evt.Entry_Fee].Adult.Value);
                contestant.Entrants.Add(entrant);

                // 2021 has no payments - see 2018 when we go back to real world
                context.SaveChanges();
            }

            var param = context.Parameters.First(x => x.Id == 1);
            // MSO happens in GMT = UTC+1 (hack)
            param.Value = DateTime.UtcNow.AddHours(1).ToString("dd MMM yyyy, HH:mm");
            context.SaveChanges();
        }

        private Contestant CreateEntrant(DataEntities context, Order2021 order)
        {
            var contestant = new Contestant()
            {
                Firstname = order.FirstName,
                Lastname = order.LastName,
                Nationality = order.LocalisedNationality,
                Title = order.Title,
                Male = (order.Title == "Mr"),
                DateofBirth = SanitiseDateOfBirth(order.DateOfBirth),
                OnlineNicknames = order.OnlineNickname,
                DiscordNickname = order.DiscordNickname,
                email = order.Email
            };

            context.Contestants.Add(contestant);

            context.SaveChanges();
            return contestant;
        }

        private void UpdateEntrant(DataEntities context, Contestant contestant, Order2021 order)
        {
            if (IsSanitisedDateOfBirth(order.DateOfBirth))
                contestant.DateofBirth = order.DateOfBirth;
            if (order.OnlineNickname != null)
            {
                // Contestants may use different nicknames for different servers, and submit separate orders
                if (contestant.OnlineNicknames == null)
                    contestant.OnlineNicknames = order.OnlineNickname;
                else if (!contestant.OnlineNicknames.Contains(order.OnlineNickname))
                    contestant.OnlineNicknames += "; " + order.OnlineNickname;
            }

            if (order.DiscordNickname != null)
                contestant.DiscordNickname = order.DiscordNickname;
            if (!string.IsNullOrEmpty(order.Email))
                contestant.email = order.Email;

            context.SaveChanges();
        }

        public DateTime? SanitiseDateOfBirth(DateTime? orderDob)
        {
            if (!orderDob.HasValue)
                return orderDob;
            // Catch people who put in today's date
            if (DateTime.Now.AddYears(-2) < orderDob)
                return null;
            // Catch people who put in silly old values
            if (DateTime.Now.AddYears(-120) < orderDob)
                return null;

            return orderDob;
        }

        public bool IsSanitisedDateOfBirth(DateTime? orderDob)
        {
            if (!orderDob.HasValue)
                return true;
            // Catch people who put in today's date
            if (DateTime.Now.AddYears(-2) < orderDob)
                return false;
            // Catch people who put in silly old values
            if (DateTime.Now.AddYears(-120) < orderDob)
                return false;

            return true;
        }
    }

    public class Order2021
    {
        public string WordpressId { get; set; }
        public string EventName { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public string LocalisedNationality { get; set; }
        public string DoBString { get; set; }
        public string OnlineNickname { get; set; }
        public string DiscordNickname { get; set; }

        public DateTime? DateOfBirth
        {
            get
            {
                DateTime dob;
                var success = DateTime.TryParseExact(DoBString, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out dob);
                return (success) ? dob : (DateTime?) null;
            }
        }
    }

}
