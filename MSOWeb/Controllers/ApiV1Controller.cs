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
            // See http://swiss-manager.at/unload/SwissManagerHelp_ENG.pdf
            var l = new OlympiadEventsApiLogic();
            try
            {
                var data = l.GetEventContestants(eventCode);
                var builder = new StringBuilder();
                int index = 0;
                foreach (var c in data.Contestants)
                {
                    index++;
                    builder.Append(string.Format("{0}|{1}|{2}||||||||||\r\n",
                        index, c.Name, c.ContestantId));
                }

                Response.AddHeader("Content-Disposition", $"attachment;filename={eventCode}.txt");
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
