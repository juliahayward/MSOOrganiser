using MSOCore.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Calculators
{
    public class MetaEventFreezer
    {
        public void FreezeMetaEvents()
        {
            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = context.Olympiad_Infoes.First(x => x.Current);
            if (currentOlympiad.Ruleset == "Pentamind")
            {
                FreezePentamind(currentOlympiad.Id);
                FreezeModernAbstract(currentOlympiad.Id);
                FreezeEurogames(currentOlympiad.Id);
                FreezePoker(currentOlympiad.Id);
                FreezeChess(currentOlympiad.Id);
                FreezeBackgammon(currentOlympiad.Id);
                FreezeMental(currentOlympiad.Id);
                FreezeImperfect(currentOlympiad.Id);
            }
            else
            {
                FreezeGrandPrix(currentOlympiad.Id);
                FreezeDraughtsGP(currentOlympiad.Id);
                FreezeMultiplayerGP(currentOlympiad.Id);
                FreezePokerGP(currentOlympiad.Id);
                FreezeChessGP(currentOlympiad.Id);
                FreezeBackgammonGP(currentOlympiad.Id);
                FreezeAbstractGP(currentOlympiad.Id);
                FreezeImperfectInfoGP(currentOlympiad.Id);
            }
        }

        public void FreezePentamind(int olympiadId)
        {
            var pentamindStandingsGenerator = new PentamindStandingsGenerator();
            var standings = pentamindStandingsGenerator.GetStandings(null);

            Freeze(olympiadId, "PEWC", standings.Standings, null);
            Freeze(olympiadId, "PEWO", standings.Standings, x => x.IsInWomensPenta);
            Freeze(olympiadId, "PEJR", standings.Standings, x => x.IsJunior);
            Freeze(olympiadId, "PESR", standings.Standings, x => x.IsSenior);
        }

        public void FreezeGrandPrix(int olympiadId)
        {
            var pentamindStandingsGenerator = new GrandPrixStandingsGenerator();
            var standings = pentamindStandingsGenerator.GetStandings(null);

            Freeze(olympiadId, "GPC", standings.Standings, null);
            Freeze(olympiadId, "WGPC", standings.Standings, x => x.IsInWomensPenta);
            Freeze(olympiadId, "JGPC", standings.Standings, x => x.IsJunior);
            Freeze(olympiadId, "SGPC", standings.Standings, x => x.IsSenior);
        }

        public void FreezeEurogames(int olympiadId)
        {
            var pentamindStandingsGenerator = new PentamindStandingsGenerator();
            var eurostandings = pentamindStandingsGenerator.GetEurogameStandings(null);
            Freeze(olympiadId, "EGWC", eurostandings.Standings);
        }

        public void FreezeModernAbstract(int olympiadId)
        {
            var pentamindStandingsGenerator = new PentamindStandingsGenerator();
            var modernAbstractStandings = pentamindStandingsGenerator.GetModernAbstractStandings(null);
            Freeze(olympiadId, "MBWC", modernAbstractStandings.Standings);         
        }

        public void FreezePoker(int olympiadId)
        {
            var pokerStandingsGenerator = new PentamindStandingsGenerator();
            var pokerstandings = pokerStandingsGenerator.GetPokerStandings(null);
            Freeze(olympiadId, "POAC", pokerstandings.Standings);        
        }

        public void FreezeChess(int olympiadId)
        {
            var pentamindStandingsGenerator = new PentamindStandingsGenerator();
            var standings = pentamindStandingsGenerator.GetChessStandings(null);
            Freeze(olympiadId, "CHCC", standings.Standings);
        }

        public void FreezeBackgammon(int olympiadId)
        {
            var pentamindStandingsGenerator = new PentamindStandingsGenerator();
            var standings = pentamindStandingsGenerator.GetBackgammonStandings(null);
            Freeze(olympiadId, "BACC", standings.Standings);
        }

        public void FreezeMental(int olympiadId)
        {
            var pentamindStandingsGenerator = new PentamindStandingsGenerator();
            var standings = pentamindStandingsGenerator.GetMentalStandings(null);
            Freeze(olympiadId, "MCCC", standings.Standings);
        }

        public void FreezeImperfect(int olympiadId)
        {
            var pentamindStandingsGenerator = new PentamindStandingsGenerator();
            var standings = pentamindStandingsGenerator.GetImperfectStandings(null);
            Freeze(olympiadId, "IICC", standings.Standings);
        }

        public void FreezeMental(int olympiadId, string code)
        {
            var pentamindStandingsGenerator = new PentamindStandingsGenerator();
            var standings = pentamindStandingsGenerator.GetMentalStandings(null);
            Freeze(olympiadId, code, standings.Standings);
        }

        public void FreezeImperfect(int olympiadId, string code)
        {
            var pentamindStandingsGenerator = new PentamindStandingsGenerator();
            var standings = pentamindStandingsGenerator.GetImperfectStandings(null);
            Freeze(olympiadId, code, standings.Standings);
        }

        public void FreezeDraughtsGP(int olympiadId)
        {
            var standingsGenerator = new GrandPrixStandingsGenerator();
            var standings = standingsGenerator.GetGPCategoryStandings("draughts");
            Freeze(olympiadId, "DRGPC", standings.Standings);
        }

        public void FreezePokerGP(int olympiadId)
        {
            var standingsGenerator = new GrandPrixStandingsGenerator();
            var standings = standingsGenerator.GetGPCategoryStandings("poker");
            Freeze(olympiadId, "POGPC", standings.Standings); 
        }

        public void FreezeMultiplayerGP(int olympiadId)
        {
            var standingsGenerator = new GrandPrixStandingsGenerator();
            var standings = standingsGenerator.GetGPCategoryStandings("multiplayer");
            Freeze(olympiadId, "MPGPC", standings.Standings);
        }

        public void FreezeChessGP(int olympiadId)
        {
            var standingsGenerator = new GrandPrixStandingsGenerator();
            var standings = standingsGenerator.GetGPCategoryStandings("chess");
            Freeze(olympiadId, "CHGPC", standings.Standings);
        }

        public void FreezeBackgammonGP(int olympiadId)
        {
            var standingsGenerator = new GrandPrixStandingsGenerator();
            var standings = standingsGenerator.GetGPCategoryStandings("backgammon");
            Freeze(olympiadId, "BAGPC", standings.Standings);
        }

        public void FreezeAbstractGP(int olympiadId)
        {
            var standingsGenerator = new GrandPrixStandingsGenerator();
            var standings = standingsGenerator.GetGPCategoryStandings("abstract");
            Freeze(olympiadId, "ABGPC", standings.Standings);
        }

        public void FreezeImperfectInfoGP(int olympiadId)
        {
            var standingsGenerator = new GrandPrixStandingsGenerator();
            var standings = standingsGenerator.GetGPCategoryStandings("imperfectinfo");
            Freeze(olympiadId, "IIGPC", standings.Standings);
        }


        private void Freeze(int olympiadId, string code, IEnumerable<IContestantStanding> standings,
            Predicate<IContestantStanding> filter = null)
        {
            if (filter == null) filter = (x => true);

            var context = DataEntitiesProvider.Provide();
            int rank = 1;
            foreach (var standing in standings.Where(x => filter(x)))
            {
                if (!standing.IsValid) continue;

                var contestant = context.Contestants.FirstOrDefault(x => x.Mind_Sport_ID == standing.ContestantId);
                var evt = context.Events.FirstOrDefault(x => x.OlympiadId == olympiadId && x.Code == code);
                var entry = context.Entrants.FirstOrDefault(x => x.OlympiadId == olympiadId
                                        && x.Game_Code == code && x.Mind_Sport_ID == standing.ContestantId);
                if (entry == null)
                {
                    entry = Entrant.NewEntrant(evt.EIN, code, olympiadId, contestant, 0m);
                    context.Entrants.Add(entry);
                }
                entry.Score = standing.TotalScoreStr;
                entry.Rank = rank;
                entry.Medal = MedalForRank(rank);
                rank++;
                context.SaveChanges();
            }
        }


        private string MedalForRank(int rank)
        {
            switch (rank)
            {
                case 1: return "Gold";
                case 2: return "Silver";
                case 3: return "Bronze";
                default: return null;
            }
        }
    }
}
