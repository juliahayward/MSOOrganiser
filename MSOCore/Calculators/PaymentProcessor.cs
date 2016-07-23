using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Calculators
{
    public class PaymentProcessor
    {
        public void ProcessAll()
        {
            var context = DataEntitiesProvider.Provide();
            var olympiad = context.Olympiad_Infoes.First(x => x.Current);
            var orders = context.EntryJsons.Where(x => x.ProcessedDate == null).OrderBy(x => x.Id).ToList();
            var entryFees = context.Fees.ToDictionary(x => x.Code, x => x);

            foreach (var order in orders)
            {
                // Chop off outer quotes if necessary, and inner backslashes
                var text = order.JsonText;
                var parsedOrder = Parse(text);
                if (!parsedOrder.IsApproved) continue;

                if (parsedOrder.EventCode == null)
                    InsertMaxFeeOrder(context, olympiad, order, parsedOrder);
                else
                    InsertSingleEventOrder(context, olympiad, order, parsedOrder, entryFees);
            }
        }

        private ParsedOrder Parse(string text)
        {
            if (text.StartsWith("\"")) 
                text = text.Substring(1, text.Length - 2)
                    .Replace("\\", "")
                    .Replace("\"event\"", "\"event_detail\"");  // property name is reserved in C#

            dynamic obj = JsonConvert.DeserializeObject(text);
            // One day it would be nice to have consistent JSON sent to me!
            var parsedOrder = new ParsedOrder();
            parsedOrder.BookingId = obj.booking_id.Value;
            parsedOrder.EventCode = obj.event_id.Value;
            parsedOrder.BookingPrice = obj.booking_price.Value;
            parsedOrder.BookingSpaces = int.Parse(obj.booking_spaces.Value);
            parsedOrder.BookingStatus = int.Parse(obj.booking_status.Value.ToString());   // should be integer 1, sometimes string "1"
            foreach (var o2 in obj.attendees)
            {
                // o2.Name is the mysterious number in json
                foreach (var entrant in o2.Value)
                {
                    var parsedEntrant = new ParsedOrder.Entrant();
                    parsedEntrant.Title = entrant.title.Value;
                    parsedEntrant.FirstName = entrant.first_name.Value;
                    parsedEntrant.LastName = entrant.last_name.Value;
                    parsedEntrant.Country = entrant.country_to_represent.Value;
                    parsedEntrant.DoB = (entrant.date_of_birth != null) ? entrant.date_of_birth.Value : null;
                    parsedOrder.Entrants.Add(parsedEntrant);
                }
            }
            parsedOrder.Timestamp = obj.timestamp.Value;
            parsedOrder.EventId = obj.event_detail.id.Value;
            parsedOrder.EventName = obj.event_detail.name.Value;
            //  MessageBox.Show(obj.attendees);
            /* \"attendees\":{\"31\":[{\"title\":\"Mr\",\"first_name\":\"Howard\",\"last_name\":\"Kenward\",
             * \"country_to_represent\":\"England\",\"date_of_birth\":\"1955-07-26\"}]},
            \"timestamp\":1466096898,\"event\":{\"id\":\"31\",\"name\":\"Trench\"}}" */

            if (parsedOrder.BookingSpaces != parsedOrder.Entrants.Count())
                throw new ParsingFailureException(string.Format("Order {0} has wrong number of entrants", parsedOrder.BookingId));

            return parsedOrder;
        }

        private void InsertMaxFeeOrder(DataEntities context, Olympiad_Info olympiad, EntryJson order, ParsedOrder parsedOrder)
        {
            // Don't do this case yet
            if (parsedOrder.BookingSpaces > 1) return;
            if (parsedOrder.Entrants.Count() > 1) return;

            var feeExpected = 0m;
            foreach (var parsedEntrant in parsedOrder.Entrants)
            {
                var contestants = context.Contestants.Where(x => x.Firstname == parsedEntrant.FirstName
                    && x.Lastname == parsedEntrant.LastName);
                // Don't handle the case of people you can't verify yet
                if (contestants.Count() != 1)
                    return;

                if (DateTime.Parse(parsedEntrant.DoB) > olympiad.FirstDateOfBirthForJunior())
                    feeExpected = olympiad.MaxCon.Value;
                else
                    feeExpected = olympiad.MaxFee.Value;

                if (feeExpected != parsedOrder.BookingPrice)
                {
                    order.Notes = "Wrong fee";
                }
                else
                {
                    // All OK
                    contestants.First().Payments.Add(new Payment()
                    {
                        Banked = 1,
                        MindSportsID = contestants.First().Mind_Sport_ID,
                        OlympiadId = olympiad.Id,
                        Payment_Method = "MSO Website",
                        Payment1 = (decimal)parsedOrder.BookingPrice,
                        Year = olympiad.StartDate.Value.Year
                    });

                    order.ProcessedDate = DateTime.Now;
                    order.Notes = string.Format("Assigned to contestant {0}", contestants.First().Mind_Sport_ID);
                }

                context.SaveChanges();
            }
        }

        private void InsertSingleEventOrder(DataEntities context, Olympiad_Info olympiad, EntryJson order, 
            ParsedOrder parsedOrder, Dictionary<string, Fee> eventFees)
        {
            // Don't do this case yet
            if (parsedOrder.BookingSpaces > 1) return;
            if (parsedOrder.Entrants.Count() > 1) return;

            var feeExpected = 0m;
            foreach (var parsedEntrant in parsedOrder.Entrants)
            {
                var contestants = context.Contestants.Where(x => x.Firstname == parsedEntrant.FirstName
                    && x.Lastname == parsedEntrant.LastName);
                var evt = context.Events.FirstOrDefault(x => x.Code == parsedOrder.EventCode 
                    && x.OlympiadId == olympiad.Id);

                // Bad event code
                if (evt == null)
                    return;
                // Don't handle the case of people you can't verify yet
                if (contestants.Count() != 1)
                    return;

                if (parsedEntrant.DoB != null && DateTime.Parse(parsedEntrant.DoB) > olympiad.FirstDateOfBirthForJunior())
                    feeExpected = eventFees[evt.Entry_Fee].Concession.Value;
                else
                    feeExpected = eventFees[evt.Entry_Fee].Adult.Value;

                if (feeExpected != parsedOrder.BookingPrice)
                {
                    order.Notes = "Wrong fee";
                }
                else
                {
                    var thisContestant = contestants.First();
                    // All OK
                    thisContestant.Payments.Add(new Payment()
                    {
                        Banked = 1,
                        MindSportsID = contestants.First().Mind_Sport_ID,
                        OlympiadId = olympiad.Id,
                        Payment_Method = "MSO Website",
                        Payment1 = (decimal)parsedOrder.BookingPrice,
                        Year = olympiad.StartDate.Value.Year
                    });

                    // Put the contestant into the right event
                    var entrant = Entrant.NewEntrant(evt.EIN, evt.Code, olympiad.Id, thisContestant,
                        (decimal)parsedOrder.BookingPrice);
                    thisContestant.Entrants.Add(entrant);

                    // Update the order to avoid re-parsing
                    order.ProcessedDate = DateTime.Now;
                    order.Notes = string.Format("Assigned to contestant {0}", contestants.First().Mind_Sport_ID);
                }

                context.SaveChanges();
            }
        }

        public class ParsedOrder
        {
            public string BookingId { get; set; }
            public string EventCode { get; set; }
            public long BookingPrice { get; set; }
            public int BookingSpaces { get; set; }
            public int BookingStatus { get; set; }
            public bool IsApproved { get { return BookingStatus == 1;  } }
            public long Timestamp { get; set; }
            public string EventId { get; set; } // what is this?
            public string EventName { get; set; }
            public IList<Entrant> Entrants { get; private set;  }

            public ParsedOrder()
            {
                Entrants = new List<Entrant>();
            }

            public class Entrant
            {
                public string Title { get; set; }
                public string FirstName { get; set; }
                public string LastName { get; set; }
                public string Country { get; set; }
                public string DoB { get; set; }
            }
        }

        public class ParsingFailureException : Exception
        {
            public ParsingFailureException(string message) : base(message) { }
        }
    }
}
