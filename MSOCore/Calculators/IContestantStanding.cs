using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Calculators
{
    // Used to harmonise meta-event freezing
    internal interface IContestantStanding
    {
        int ContestantId { get; set; }
        bool IsInWomensPenta { get; set; }
        bool IsJunior { get; set; }
        bool IsSenior { get; set; }
        double TotalScore { get; set; }
        string TotalScoreStr { get; }
        bool IsValid { get; set; }
    }
}
