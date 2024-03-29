﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Text;
using System.Web.Mvc.Html;
using MSOCore;
using MSOCore.ApiLogic;
using MSOCore.Calculators;
using System.Net;

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
                : "C:\\Users\\Julia\\OneDrive\\Src\\MSOOrganiser\\MSOWeb\\RawData\\upload" + DateTime.Now.ToString("yyyy-MM-dd-HHmm") + ".csv";

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

        public class EventUploadVM
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

            var model = new EventUploadVM() {EventId = id, Name = evt.Mind_Sport, Code = evt.Code};
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
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    int rank = 0;
                    var parts = line.Split(':', '(', ')');
                    bool validRank = int.TryParse(parts[0].Trim(), out rank);
                    if (!validRank) continue;

                    model.Elements.Add(new PokerstarsResultModel.PokerstarsResultElement()
                    {
                        Rank = rank,
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
                        logic.AddNewContestantWithScoreToEvent("--" + element.UserId + "--", "", element.UserId, element.Country, 
                            element.Rank, (-element.Rank).ToString(), eventId);
                    }
                    // We know who it is
                    else
                    {
                        var entrant = entrants.FirstOrDefault(x =>
                            x.Mind_Sport_ID == matchingContestants.Single().Mind_Sport_ID);
                        if (entrant == null)
                        {
                            logic.AddContestantWithScoreToEvent(matchingContestants.Single().Mind_Sport_ID, element.Rank, (-element.Rank).ToString(), eventId);
                        }
                        else
                        {
                            entrant.Rank = element.Rank;
                            entrant.Score = (-element.Rank).ToString();
                            context.SaveChanges();
                        }
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

        [Authorize(Roles = "Superadmin, Admin")]
        public ActionResult WorldPuzzle(int id)
        {
            var context = DataEntitiesProvider.Provide();
            var evt = context.Events.FirstOrDefault(x => x.EIN == id);
            if (evt == null) throw new ArgumentException("No event with this ID");
            if (evt.Location != "WorldPuzzle") throw new ArgumentException("This is not a WorldPuzzle event");

            var model = new EventUploadVM() { EventId = id, Name = evt.Mind_Sport, Code = evt.Code };
            return View(model);
        }

        public class WorldPuzzleResultModel
        {
            public class WorldPuzzleResultElement
            {
                public int Rank { get; set; }
                public string Name { get; set; }
                public string Country { get; set; }
                public string Nickname { get; set; }
                public string Score { get; set; }
            }

            public readonly List<WorldPuzzleResultElement> Elements = new List<WorldPuzzleResultElement>();
        }

        [Authorize(Roles = "Superadmin, Admin")]
        [HttpPost]
        public ActionResult UploadWorldPuzzle(int eventId, HttpPostedFileBase inputFile)
        {
            try
            {
                if (inputFile == null)
                    throw new FileNotFoundException("Please select a file");

                WorldPuzzleResultModel model = new WorldPuzzleResultModel();
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
                    var parts = line.Split('\t');
                    // Intermediate headers
                    if (parts[0].StartsWith("#")) continue;
                    model.Elements.Add(new WorldPuzzleResultModel.WorldPuzzleResultElement()
                    {
                        Rank = int.Parse(parts[0].Replace(".", "").Trim()),
                        Name = parts[1].Trim(),
                        Country = parts[2].Trim(),
                        Nickname = parts[3].Trim(),
                        Score = parts[4].Trim(),
                        // Answers Sent
                        // Answers OK
                        // Time - taken into account with Rank
                    });
                }

                var context = DataEntitiesProvider.Provide();
                var evt = context.Events.FirstOrDefault(x => x.EIN == eventId);
                if (evt == null) throw new ArgumentException("No event with this ID");
                if (evt.Location != "WorldPuzzle") throw new ArgumentException("This is not a WorldPuzzle event");
                var entrants = evt.Entrants.ToList();
                
                string ambiguous = "";

                foreach (var element in model.Elements)
                {
                    var matchingContestants = context.Contestants.Where(x => x.Firstname + " " + x.Lastname == element.Name).ToList();
                    if (matchingContestants.Count() > 1)
                    {
                        ambiguous += element.Name + ",";
                    }
                    // Can't reconcile this user id - must be a new person
                    else if (matchingContestants.Count() == 0)
                    {
                        var lastSpace = element.Name.LastIndexOf(" ");
                        var firstName = element.Name.Substring(0, lastSpace);
                        var lastName = element.Name.Substring(lastSpace + 1);
                        logic.AddNewContestantWithScoreToEvent(firstName, lastName, element.Nickname, element.Country, element.Rank, element.Score, eventId);
                    }
                    // We know who it is
                    else
                    {
                        var entrant = entrants.FirstOrDefault(x =>
                            x.Mind_Sport_ID == matchingContestants.Single().Mind_Sport_ID);
                        if (entrant == null)
                        {
                            logic.AddContestantWithScoreToEvent(matchingContestants.Single().Mind_Sport_ID,
                                element.Rank, element.Score, eventId);
                        }
                        else
                        {
                            entrant.Rank = element.Rank;
                            entrant.Score = element.Score;
                            context.SaveChanges();
                        }
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

        [Authorize(Roles = "Superadmin, Admin")]
        public ActionResult RussianDraughts(int id)
        {
            var context = DataEntitiesProvider.Provide();
            var evt = context.Events.FirstOrDefault(x => x.EIN == id);
            if (evt == null) throw new ArgumentException("No event with this ID");
            if (evt.Location != "PlayElephant") throw new ArgumentException("This is not a PlayElephant event");

            var model = new EventUploadVM() { EventId = id, Name = evt.Mind_Sport, Code = evt.Code };
            return View(model);
        }

        public class PlayElephantResultModel
        {
            public class PlayElephantResultElement
            {
                public int Rank { get; set; }
                public string FirstName { get; set; }
                public string LastName { get; set; }
                public string Country { get; set; }
                public string Nickname { get; set; }
                public string Score { get; set; }
                public string TieBreak { get; set; }
                public string YearOfBirth { get; set; }
            }

            public readonly List<PlayElephantResultElement> Elements = new List<PlayElephantResultElement>();
        }

        [Authorize(Roles = "Superadmin, Admin")]
        [HttpPost]
        public ActionResult UploadRussianDraughts(int eventId, HttpPostedFileBase inputFile)
        {
            try
            {
                if (inputFile == null)
                    throw new FileNotFoundException("Please select a file");

                PlayElephantResultModel model = new PlayElephantResultModel();
                string fileInput = "";
                using (var source = new StreamReader(inputFile.InputStream, Encoding.Unicode))
                {
                    using (var target = new StringWriter())
                    {
                        target.Write(source.ReadToEnd());
                        fileInput = target.ToString();
                    }
                }

                ContestantsLogic logic = new ContestantsLogic();
                int rank;
                string firstname, lastname;

                var lines = fileInput.Split('\n');
                foreach (string line in lines)
                {
                    var parts = line.Split('\t');
                    // Intermediate headers
                    if (!int.TryParse(parts[0], out rank)) continue;
                    string nickname = parts[7].Trim();
                    string name = parts[4].Trim();
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        firstname = "-" + nickname + "-";
                        lastname = "";
                    }
                    else
                    {
                        firstname = name.Split(' ')[1].Trim();
                        lastname = name.Split(' ')[0].Replace(",", "").Trim();
                    }
                    model.Elements.Add(new PlayElephantResultModel.PlayElephantResultElement()
                    {
                        Rank = rank,
                        FirstName = firstname,
                        LastName = lastname,
                        Country = parts[8].Trim(),
                        Nickname = nickname,
                        Score = parts[10].Trim(),
                        TieBreak = parts[11].Trim(),
                        YearOfBirth = (parts[5] != "#REF!" ) ? parts[5].Trim() : ""
                    });
                }

                var context = DataEntitiesProvider.Provide();
                var evt = context.Events.FirstOrDefault(x => x.EIN == eventId);
                if (evt == null) throw new ArgumentException("No event with this ID");
                if (evt.Location != "PlayElephant") throw new ArgumentException("This is not a PlayElephant event");
                var entrants = evt.Entrants.ToList();

                string ambiguous = "";

                foreach (var element in model.Elements)
                {
                    var matchingContestants = context.Contestants.Where(x => x.OnlineNicknames.Contains(element.Nickname)).ToList();
                    if (matchingContestants.Count() > 1)
                    {
                        ambiguous += element.Nickname + ",";
                    }
                    // Can't reconcile this user id - must be a new person
                    else if (matchingContestants.Count() == 0)
                    {
                        logic.AddNewContestantWithScoreToEvent(element.FirstName, element.LastName, element.Nickname, element.Country, element.Rank, element.Score, element.TieBreak, eventId);
                    }
                    // We know who it is
                    else
                    {
                        var entrant = entrants.FirstOrDefault(x =>
                            x.Mind_Sport_ID == matchingContestants.Single().Mind_Sport_ID);
                        if (entrant == null)
                        {
                            logic.AddContestantWithScoreToEvent(matchingContestants.Single().Mind_Sport_ID,
                                element.Rank, element.Score, element.TieBreak, eventId);
                        }
                        else
                        {
                            entrant.Rank = element.Rank;
                            entrant.Score = element.Score;
                            context.SaveChanges();
                        }
                    }
                }

                //TempData["SuccessMessage"] = $"Loaded {model.Elements.Count} results from file. Ambiguous: {ambiguous}";
            }
            catch (Exception e)
            {
                TempData["FailureMessage"] = e.Message;
            }
            return new RedirectResult("/Olympiad/Event/" + eventId + "?editable=true");
        }

        public class PlayStrategyResultModel
        {
            public class PlayStrategyResultElement
            {
                public int Rank { get; set; }
                public string Nickname { get; set; }
                public string Score { get; set; }
            }

            public readonly List<PlayStrategyResultElement> Elements = new List<PlayStrategyResultElement>();
        }

        [Authorize(Roles = "Superadmin, Admin")]
        public ActionResult PlayStrategy(int id)
        {
            var context = DataEntitiesProvider.Provide();
            var evt = context.Events.FirstOrDefault(x => x.EIN == id);
            if (evt == null) throw new ArgumentException("No event with this ID");
            if (evt.Location != "PlayStrategy") throw new ArgumentException("This is not a PlayStrategy event");

            var model = new EventUploadVM() { EventId = id, Name = evt.Mind_Sport, Code = evt.Code };
            return View(model);
        }

        [Authorize(Roles = "Superadmin, Admin")]
        [HttpPost]
        public ActionResult UploadPlayStrategy(int eventId, HttpPostedFileBase inputFile)
        {
            try
            {
                if (inputFile == null)
                    throw new FileNotFoundException("Please select a file");

                PlayStrategyResultModel model = new PlayStrategyResultModel();
                string fileInput = "";
                using (var source = new StreamReader(inputFile.InputStream, Encoding.UTF8))
                {
                    using (var target = new StringWriter())
                    {
                        target.Write(source.ReadToEnd());
                        fileInput = target.ToString();
                    }
                }

                ContestantsLogic logic = new ContestantsLogic();
                int rank;

                var lines = fileInput.Split('\n');
                foreach (string line in lines)
                {
                    var parts = line.Split(',');
                    // Intermediate headers
                    if (!int.TryParse(parts[0], out rank)) continue;
                    string nickname = parts[2].Trim();
                    model.Elements.Add(new PlayStrategyResultModel.PlayStrategyResultElement()
                    {
                        Rank = rank,
                        Nickname = nickname,
                        Score = parts[4].Trim(),
                    });
                }

                var context = DataEntitiesProvider.Provide();
                var evt = context.Events.FirstOrDefault(x => x.EIN == eventId);
                if (evt == null) throw new ArgumentException("No event with this ID");
                if (evt.Location != "PlayStrategy") throw new ArgumentException("This is not a PlayStrategy event");
                var entrants = evt.Entrants.ToList();

                string ambiguous = "";

                foreach (var element in model.Elements)
                {
                    var matchingContestants = context.Contestants.Where(x => x.OnlineNicknames.Contains(element.Nickname)).ToList();
                    if (matchingContestants.Count() > 1)
                    {
                        ambiguous += element.Nickname + ",";
                    }
                    // Can't reconcile this user id - must be a new person
                    else if (matchingContestants.Count() == 0)
                    {
                        logic.AddNewContestantWithScoreToEvent(element.Nickname, "", element.Nickname, "", element.Rank, element.Score, "-"+element.Rank, eventId);
                    }
                    // We know who it is
                    else
                    {
                        var entrant = entrants.FirstOrDefault(x =>
                            x.Mind_Sport_ID == matchingContestants.Single().Mind_Sport_ID);
                        if (entrant == null)
                        {
                            logic.AddContestantWithScoreToEvent(matchingContestants.Single().Mind_Sport_ID,
                                element.Rank, element.Score, "-"+element.Rank, eventId);
                        }
                        else
                        {
                            entrant.Rank = element.Rank;
                            entrant.Score = element.Score;
                            context.SaveChanges();
                        }
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

        public class BGAResultModel
        {
            public class BGAResultElement
            {
                public int Rank { get; set; }
                public string Nickname { get; set; }
                public string Score { get; set; }
                public bool Withdrawn { get; set;  }
            }

            public readonly List<BGAResultElement> Elements = new List<BGAResultElement>();
        }

        [Authorize(Roles = "Superadmin, Admin")]
        public ActionResult BoardGameArena(int id)
        {
            var context = DataEntitiesProvider.Provide();
            var evt = context.Events.FirstOrDefault(x => x.EIN == id);
            if (evt == null) throw new ArgumentException("No event with this ID");
            if (evt.Location != "BoardGameArena") throw new ArgumentException("This is not a BoardGameArena event");

            var model = new EventUploadVM() { EventId = id, Name = evt.Mind_Sport, Code = evt.Code };
            return View(model);
        }

        [Authorize(Roles = "Superadmin, Admin")]
        [HttpPost]
        public ActionResult UploadBoardGameArena(int eventId, string url)
        {
            try
            {
                if (!url.Contains("boardgamearena")) // assume ID only
                    url = "https://en.boardgamearena.com/tournament?id=" + url;

                if (url == null)
                    throw new FileNotFoundException("Please provide a URL");

                BGAResultModel model = new BGAResultModel();

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var request = WebRequest.Create(url);

                var dom = new HtmlAgilityPack.HtmlDocument();
                dom.Load(request.GetResponse().GetResponseStream());
                var main = dom.DocumentNode;
                var tableRows = main.Descendants("tr").Where(x => x.Attributes["class"]?.Value?.Contains("v2tournament__players-stats-table-row") ?? false);
                foreach (var row in tableRows)
                {
                    var colEnumerator = row.Descendants("td").GetEnumerator();
                    colEnumerator.MoveNext();
                    var ordinal = colEnumerator.Current.InnerText;
                    colEnumerator.MoveNext();
                    var userName = colEnumerator.Current.Descendants("a").First().InnerText;
                    colEnumerator.MoveNext();
                    var points = colEnumerator.Current.InnerText;
                    colEnumerator.MoveNext();
                    var played = colEnumerator.Current.InnerText;
                    colEnumerator.MoveNext();
                    var skipped = colEnumerator.Current.InnerText;
                    // if Ordinal is empty then they didn't complete the tournament - counts as Withdrawn

                    model.Elements.Add(new BGAResultModel.BGAResultElement()
                    {
                        Withdrawn = string.IsNullOrEmpty(ordinal),
                        // lop off "st", "nd", ...
                        Rank = (string.IsNullOrEmpty(ordinal)) ? -1 : int.Parse(ordinal.Substring(0, ordinal.Length-2)),
                        Score = points,
                        Nickname = userName
                    }) ;
                }

                ContestantsLogic logic = new ContestantsLogic();
                var context = DataEntitiesProvider.Provide();
                var evt = context.Events.FirstOrDefault(x => x.EIN == eventId);
                if (evt == null) throw new ArgumentException("No event with this ID");
                if (evt.Location != "BoardGameArena") throw new ArgumentException("This is not a BoardGameArena event");
                var entrants = evt.Entrants.ToList();

                string ambiguous = "";

                foreach (var element in model.Elements)
                {
                    var matchingContestants = context.Contestants.Where(x => x.OnlineNicknames.Contains(element.Nickname)).ToList();
                    if (matchingContestants.Count() > 1)
                    {
                        ambiguous += element.Nickname + ",";
                    }
                    // Can't reconcile this user id - must be a new person
                    else if (matchingContestants.Count() == 0)
                    {
                        logic.AddNewContestantWithScoreToEvent(element.Nickname, "", element.Nickname, "", element.Rank, element.Score, "-" + element.Rank, eventId, element.Withdrawn);
                    }
                    // We know who it is
                    else
                    {
                        var entrant = entrants.FirstOrDefault(x =>
                            x.Mind_Sport_ID == matchingContestants.Single().Mind_Sport_ID);
                        if (entrant == null)
                        {
                            logic.AddContestantWithScoreToEvent(matchingContestants.Single().Mind_Sport_ID,
                                element.Rank, element.Score, "-" + element.Rank, eventId, element.Withdrawn);
                        }
                        else
                        {
                            entrant.Rank = element.Rank;
                            entrant.Score = element.Score;
                            entrant.Tie_break = (element.Rank == -1) ? "" : "-" + element.Rank;
                            entrant.Withdrawn = element.Withdrawn;
                            context.SaveChanges();
                        }
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
