using Microsoft.Win32;
using MSOCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOOrganiser.Reports
{
    public class ContestantListCsvExporter
    {
        public void ExportThisYearsContestants()
        {
            var dlg = new SaveFileDialog();
            dlg.FileName = "Contestants"; 
            dlg.DefaultExt = ".csv"; 
            dlg.Filter = "Excel documents |*.csv"; 

            var result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;

                var context = new DataEntities();
                var olympiad = context.Olympiad_Infoes
                    .OrderByDescending(x => x.StartDate)
                    .First();
                var contestants = context.Entrants.Where(x => x.OlympiadId == olympiad.Id && x.Name != null)
                    .Select(x => x.Name)
                    .Distinct()
                    .OrderBy(n => n.Lastname)
                    .ToList();

                using (var fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    using (var sw = new StreamWriter(fs))
                    {
                        foreach (var c in contestants)
                        {
                            sw.WriteLine(string.Join(",",
                                c.Firstname, c.Lastname, c.Address1, c.Address2, c.City, c.County, c.Country, c.PostCode));
                        }
                    }
                }
            }
        }
    }
}
