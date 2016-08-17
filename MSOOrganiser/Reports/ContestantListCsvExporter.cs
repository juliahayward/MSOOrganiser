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

                var context = DataEntitiesProvider.Provide();
                var olympiad = context.Olympiad_Infoes
                    .OrderByDescending(x => x.StartDate)
                    .First();
                var entrants = context.Entrants.Where(x => x.OlympiadId == olympiad.Id && x.Name != null)
                    .Distinct()
                    .OrderBy(e => e.Name.Lastname)
                    .ThenBy(e => e.Name.Firstname)
                    .ToList();

                using (var fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    using (var sw = new StreamWriter(fs))
                    {
                        foreach (var e in entrants)
                        {
                            sw.WriteLine(string.Join(",",
                                e.Game_Code, e.Fee, 
                                e.Name.Firstname, e.Name.Lastname, e.Name.Address1, e.Name.Address2,
                                e.Name.City, e.Name.County, e.Name.Country, e.Name.PostCode));
                        }
                    }
                }
            }
        }
    }
}
