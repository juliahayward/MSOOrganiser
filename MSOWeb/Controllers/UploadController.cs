using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using MSOCore;
using MSOCore.ApiLogic;
using MSOCore.Calculators;

namespace MSOWeb.Controllers
{
    public class UploadController : Controller
    {
        //
        // GET: /Upload/
        [Authorize(Roles = "Superadmin, Admin")]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Superadmin, Admin")]
        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase inputFile)
        {
            try
            {
                if (inputFile == null)
                    throw new FileNotFoundException("Please select a file");

                var filename = (Directory.Exists("C:\\inetpub\\wwwroot\\msoweb\\rawdata"))
                ? "C:\\inetpub\\wwwroot\\msoweb\\rawdata\\upload" + DateTime.Now.ToString("yyyy-MM-dd-HHmm") + ".csv"
                : "C:\\src\\juliahayward\\MSOOrganiser\\MSOWeb\\RawData\\upload" + DateTime.Now.ToString("yyyy-MM-dd-HHmm") + ".csv";

                using (var source = new StreamReader(inputFile.InputStream))
                {
                    using (var target = new StreamWriter(new FileStream(
                        filename, FileMode.CreateNew, FileAccess.Write)))
                    {
                        target.Write(source.ReadToEnd());
                    }
                }

                var processor = new PaymentProcessor2021();
                var orders = processor.ParseCsvFile(filename);
                int loaded = processor.ProcessAll(orders);

                TempData["SuccessMessage"] = $"Loaded {loaded} orders from file of {orders.Count()}";
            }
            catch (Exception e)
            {
                TempData["FailureMessage"] = e.Message;
            }
            return new RedirectResult("/Upload/");
        }

        public class PokerstarsVM
        {
            public int EventId { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }

        [Authorize(Roles = "Superadmin, Admin")]
        public ActionResult Pokerstars(int id)
        {
            var context = DataEntitiesProvider.Provide();
            var evt = context.Events.FirstOrDefault(x => x.EIN == id);
            if (evt == null) throw new ArgumentException("No event with this ID");
            if (evt.Location != "Pokerstars") throw new ArgumentException("This is not a Pokerstars event");

            var model = new PokerstarsVM() {EventId = id, Name = evt.Mind_Sport, Code = evt.Code};
            return View(model);
        }

        public class PokerstarsResultModel
        {
            public class PokerstarsResultElement
            {
                public int Rank { get; set; }
                public string UserId { get; set; }
                public string Country { get; set; }
            }

            public readonly List<PokerstarsResultElement> Elements = new List<PokerstarsResultElement>();
        }

        [Authorize(Roles = "Superadmin, Admin")]
        [HttpPost]
        public ActionResult UploadPokerstars(int eventId, HttpPostedFileBase inputFile)
        {
            try
            {
                if (inputFile == null)
                    throw new FileNotFoundException("Please select a file");

                PokerstarsResultModel model = new PokerstarsResultModel();
                string fileInput = "";
                using (var source = new StreamReader(inputFile.InputStream))
                {
                    using (var target = new StringWriter())
                    {
                        target.Write(source.ReadToEnd());
                        fileInput = target.ToString();
                    }
                }

                ContestantsLogic logic = new ContestantsLogic();

                var lines = fileInput.Split('\n');
                foreach (string line in lines)
                {
                    var parts = line.Split(':', '(', ')');
                    model.Elements.Add(new PokerstarsResultModel.PokerstarsResultElement()
                    {
                        Rank = int.Parse(parts[0].Trim()),
                        UserId = parts[1].Trim(),
                        Country = parts[2].Trim()
                    });
                }

                var context = DataEntitiesProvider.Provide();
                var evt = context.Events.FirstOrDefault(x => x.EIN == eventId);
                if (evt == null) throw new ArgumentException("No event with this ID");
                if (evt.Location != "Pokerstars") throw new ArgumentException("This is not a Pokerstars event");
                var entrants = evt.Entrants.ToList();

                string ambiguous = "";

                foreach (var element in model.Elements)
                {
                    var matchingContestants = context.Contestants.Where(x => x.OnlineNicknames.ToLower().Contains(element.UserId.ToLower())).ToList();
                    if (matchingContestants.Count() > 1)
                    {
                        ambiguous += element.UserId + ",";
                    }
                    // Can't reconcile this user id - must be a new person
                    else if (matchingContestants.Count() == 0)
                    {
                        var entrant = logic.AddNewContestantWithScoreToEvent("--" + element.UserId + "--", "", element.UserId, element.Country, -element.Rank, eventId);
                    }
                    // We know who it is
                    else
                    {
                        var entrant = entrants.FirstOrDefault(x =>
                            x.Mind_Sport_ID == matchingContestants.Single().Mind_Sport_ID);
                        if (entrant == null)
                        {
                            entrant = logic.AddContestantToEvent(matchingContestants.Single().Mind_Sport_ID, eventId);
                        }
                        entrant.Score = (-element.Rank).ToString();
                        context.SaveChanges();
                    }
                }

                TempData["SuccessMessage"] = $"Loaded {model.Elements.Count} results from file. Ambiguous: {ambiguous}";
            }
            catch (Exception e)
            {
                TempData["FailureMessage"] = e.Message;
            }
            return new RedirectResult("/Olympiad/Event/" + eventId + "?editable=true");
        }
    }
}
