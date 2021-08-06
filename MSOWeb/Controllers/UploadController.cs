using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
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
                processor.ProcessAll(orders);

                TempData["SuccessMessage"] = $"Loaded {orders.Count()} orders ";
            }
            catch (Exception e)
            {
                TempData["FailureMessage"] = e.Message;
            }
            return new RedirectResult("/Upload/");
        }
    }
}
