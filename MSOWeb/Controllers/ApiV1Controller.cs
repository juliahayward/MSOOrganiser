using MSOCore.ApiLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace MSOWeb.Controllers
{
    public class ApiV1Controller : Controller
    {
        public ActionResult GetOlympiadEvents()
        {
            var l = new OlympiadEventsApiLogic();

            return Json(l.GetOlympiadEvents(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult MultiplayerEventContestants(string eventCode)
        {
            var l = new OlympiadEventsApiLogic();
            try
            {
                return new XmlResult(l.GetEventContestants(eventCode));
            }
            catch (ArgumentOutOfRangeException)
            {
                return HttpNotFound();
            }
        }

        public ActionResult SwissManagerEventContestants(string eventCode)
        {
            // See http://swiss-manager.at/unload/SwissManagerHelp_ENG.pdf
            var l = new OlympiadEventsApiLogic();
            try
            {
                Response.AddHeader("Content-Disposition", $"attachment;filename={eventCode}.xml");
                return new XmlResult(l.GetSwissManagerEventContestants(eventCode), true);
            }
            catch (ArgumentOutOfRangeException)
            {
                return HttpNotFound();
            }
        }

        public ActionResult SwissPerfectEventContestants(string eventCode)
        {
            // See help in SP98
            var l = new OlympiadEventsApiLogic();
            try
            {
                var data = l.GetEventContestants(eventCode);
                var builder = new StringBuilder();
                int index = 0;
                foreach (var c in data.Contestants.OrderByDescending(c => c.SeedingPoints))
                {
                    index++;
                    builder.Append($"{index}|{c.FirstName}|{c.LastName}|{c.ContestantId}|{c.RatingPoints}|{c.SeedingPoints}|{c.IsJuniorStr}|{c.Phone}|{c.Email}|{c.AllOnlineNicknames}|{c.DiscordNickname}|{c.Whatsapp}|{c.EntryId}|{c.Nationality}|\r\n");
                }

                Response.AddHeader("Content-Disposition", $"attachment;filename={eventCode}.txt");
                return Content(builder.ToString(), MediaTypeNames.Text.Plain);
            }
            catch (ArgumentOutOfRangeException)
            {
                return HttpNotFound();
            }
        }

        public ActionResult PlainEventContestants(string eventCode)
        {
            // See help in SP98
            var l = new OlympiadEventsApiLogic();
            try
            {
                var data = l.GetEventContestants(eventCode);
                var builder = new StringBuilder();
                int index = 0;
                builder.Append($"Index,First Name,Last Name,ContestantId,RatingPoints,SeedingPoints,IsJunior,Phone,Email,Online Nicknames,Discord Nickname,Whatsapp,EntryId,Country\r\n");
                foreach (var c in data.Contestants.OrderByDescending(c => c.SeedingPoints))
                {
                    index++;
                    builder.Append($"{index},{c.FirstName},{c.LastName},{c.ContestantId},{c.RatingPoints},{c.SeedingPoints},{c.IsJuniorStr},{c.Phone},{c.Email},\"{c.AllOnlineNicknames}\",{c.DiscordNickname},{c.Whatsapp},{c.EntryId},{c.Nationality}\r\n");
                }

                Response.AddHeader("Content-Disposition", $"attachment;filename={eventCode}.csv");
                return Content(builder.ToString(), MediaTypeNames.Text.Plain);
            }
            catch (ArgumentOutOfRangeException)
            {
                return HttpNotFound();
            }
        }


        [HttpPost]
        public ActionResult AddEventEntry(string data)
        {
            var l = new OlympiadEventsApiLogic();
            try
            {
                l.AddEventEntry(data);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

    }
}
