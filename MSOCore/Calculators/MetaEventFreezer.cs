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
            FreezePentamind();
            FreezeModernAbstract();
            FreezeEurogames();
            FreezePoker();
            FreezeChess();
            FreezeBackgammon();
        }

        public void FreezePentamind()
        {
            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = context.Olympiad_Infoes.First(x => x.Current);

            // First of all, for each pentamind qualifier make a PEWC entry
            var pentamindStandingsGenerator = new PentamindStandingsGenerator();
            var standings = pentamindStandingsGenerator.GetStandings(null);
            int rank = 1;
            foreach (var standing in standings.Standings)
            {
                if (!standing.IsValid) continue;

                var contestant = context.Contestants.FirstOrDefault(x => x.Mind_Sport_ID == standing.ContestantId);
                var evt = context.Events.FirstOrDefault(x => x.OlympiadId == currentOlympiad.Id && x.Code == "PEWC");
                var entry = context.Entrants.FirstOrDefault(x => x.OlympiadId == currentOlympiad.Id
                                        && x.Game_Code == "PEWC" && x.Mind_Sport_ID == standing.ContestantId);
                if (entry == null)
                {
                    entry = Entrant.NewEntrant(evt.EIN, "PEWC", currentOlympiad.Id, contestant, 0m);
                    context.Entrants.Add(entry);
                }
                entry.Score = standing.TotalScoreStr;
                entry.Rank = rank;
                entry.Medal = MedalForRank(rank);
                rank++;
                context.SaveChanges();
            }

            rank = 1;
            foreach (var standing in standings.Standings.Where(x => x.IsInWomensPenta))
            {
                if (!standing.IsValid) continue;

                var contestant = context.Contestants.FirstOrDefault(x => x.Mind_Sport_ID == standing.ContestantId);
                var evt = context.Events.FirstOrDefault(x => x.OlympiadId == currentOlympiad.Id && x.Code == "PEWO");
                var entry = context.Entrants.FirstOrDefault(x => x.OlympiadId == currentOlympiad.Id
                                        && x.Game_Code == "PEWO" && x.Mind_Sport_ID == standing.ContestantId);
                if (entry == null)
                {
                    entry = Entrant.NewEntrant(evt.EIN, "PEWO", currentOlympiad.Id, contestant, 0m);
                    context.Entrants.Add(entry);
                }
                entry.Score = standing.TotalScoreStr;
                entry.Rank = rank;
                entry.Medal = MedalForRank(rank);
                rank++;
                context.SaveChanges();
            }

            rank = 1;
            foreach (var standing in standings.Standings.Where(x => x.IsJunior))
            {
                if (!standing.IsValid) continue;

                var contestant = context.Contestants.FirstOrDefault(x => x.Mind_Sport_ID == standing.ContestantId);
                var evt = context.Events.FirstOrDefault(x => x.OlympiadId == currentOlympiad.Id && x.Code == "PEJR");
                var entry = context.Entrants.FirstOrDefault(x => x.OlympiadId == currentOlympiad.Id
                                        && x.Game_Code == "PEJR" && x.Mind_Sport_ID == standing.ContestantId);
                if (entry == null)
                {
                    entry = Entrant.NewEntrant(evt.EIN, "PEJR", currentOlympiad.Id, contestant, 0m);
                    context.Entrants.Add(entry);
                }
                entry.Score = standing.TotalScoreStr;
                entry.Rank = rank;
                entry.Medal = MedalForRank(rank);
                rank++;
                context.SaveChanges();
            }

            rank = 1;
            foreach (var standing in standings.Standings.Where(x => x.IsSenior))
            {
                if (!standing.IsValid) continue;

                var contestant = context.Contestants.FirstOrDefault(x => x.Mind_Sport_ID == standing.ContestantId);
                var evt = context.Events.FirstOrDefault(x => x.OlympiadId == currentOlympiad.Id && x.Code == "PESR");
                var entry = context.Entrants.FirstOrDefault(x => x.OlympiadId == currentOlympiad.Id
                                        && x.Game_Code == "PESR" && x.Mind_Sport_ID == standing.ContestantId);
                if (entry == null)
                {
                    entry = Entrant.NewEntrant(evt.EIN, "PESR", currentOlympiad.Id, contestant, 0m);
                    context.Entrants.Add(entry);
                }
                entry.Score = standing.TotalScoreStr;
                entry.Rank = rank;
                entry.Medal = MedalForRank(rank);
                rank++;
                context.SaveChanges();
            }
        }

        public void FreezeEurogames()
        {
            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = context.Olympiad_Infoes.First(x => x.Current);

            // Next a Eurogames one
            var pentamindStandingsGenerator = new PentamindStandingsGenerator();
            var eurostandings = pentamindStandingsGenerator.GetEuroStandings(null);
            int rank = 1;
            foreach (var standing in eurostandings.Standings)
            {
                if (!standing.IsValid) continue;

                var contestant = context.Contestants.FirstOrDefault(x => x.Mind_Sport_ID == standing.ContestantId);
                var evt = context.Events.FirstOrDefault(x => x.OlympiadId == currentOlympiad.Id && x.Code == "EGWC");
                var entry = context.Entrants.FirstOrDefault(x => x.OlympiadId == currentOlympiad.Id
                                        && x.Game_Code == "EGWC" && x.Mind_Sport_ID == standing.ContestantId);
                if (entry == null)
                {
                    entry = Entrant.NewEntrant(evt.EIN, "EGWC", currentOlympiad.Id, contestant, 0m);
                    context.Entrants.Add(entry);
                }
                entry.Score = standing.TotalScoreStr;
                entry.Rank = rank;
                entry.Medal = MedalForRank(rank);
                rank++;
                context.SaveChanges();
            }
        }

        public void FreezeModernAbstract()
        {
            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = context.Olympiad_Infoes.First(x => x.Current);
            var pentamindStandingsGenerator = new PentamindStandingsGenerator();

            // Next a Modern Abstract one
            var modernAbstractStandings = pentamindStandingsGenerator.GetModernAbstractStandings(null);
            int rank = 1;
            foreach (var standing in modernAbstractStandings.Standings)
            {
                if (!standing.IsValid) continue;

                var contestant = context.Contestants.FirstOrDefault(x => x.Mind_Sport_ID == standing.ContestantId);
                var evt = context.Events.FirstOrDefault(x => x.OlympiadId == currentOlympiad.Id && x.Code == "MBWC");
                var entry = context.Entrants.FirstOrDefault(x => x.OlympiadId == currentOlympiad.Id
                                        && x.Game_Code == "MBWC" && x.Mind_Sport_ID == standing.ContestantId);
                if (entry == null)
                {
                    entry = Entrant.NewEntrant(evt.EIN, "MBWC", currentOlympiad.Id, contestant, 0m);
                    context.Entrants.Add(entry);
                }
                entry.Score = standing.TotalScoreStr;
                entry.Rank = rank;
                entry.Medal = MedalForRank(rank);
                rank++;
                context.SaveChanges();
            }
        }

        public void FreezePoker()
        {
            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = context.Olympiad_Infoes.First(x => x.Current);

            // Next a Poker one
            var pokerStandingsGenerator = new PentamindStandingsGenerator();
            var pokerstandings = pokerStandingsGenerator.GetPokerStandings(null);
            int rank = 1;
            foreach (var standing in pokerstandings.Standings)
            {
                if (!standing.IsValid) continue;

                var contestant = context.Contestants.FirstOrDefault(x => x.Mind_Sport_ID == standing.ContestantId);
                var evt = context.Events.FirstOrDefault(x => x.OlympiadId == currentOlympiad.Id && x.Code == "POAC");
                var entry = context.Entrants.FirstOrDefault(x => x.OlympiadId == currentOlympiad.Id
                                        && x.Game_Code == "POAC" && x.Mind_Sport_ID == standing.ContestantId);
                if (entry == null)
                {
                    entry = Entrant.NewEntrant(evt.EIN, "POAC", currentOlympiad.Id, contestant, 0m);
                    context.Entrants.Add(entry);
                }
                entry.Score = standing.TotalScoreStr;
                entry.Rank = rank;
                entry.Medal = MedalForRank(rank);
                rank++;
                context.SaveChanges();
            }
        }

        public void FreezeChess()
        {
            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = context.Olympiad_Infoes.First(x => x.Current);
            var pentamindStandingsGenerator = new PentamindStandingsGenerator();

            // Next a Modern Abstract one
            var modernAbstractStandings = pentamindStandingsGenerator.GetChessStandings(null);
            int rank = 1;
            foreach (var standing in modernAbstractStandings.Standings)
            {
                if (!standing.IsValid) continue;

                var contestant = context.Contestants.FirstOrDefault(x => x.Mind_Sport_ID == standing.ContestantId);
                var evt = context.Events.FirstOrDefault(x => x.OlympiadId == currentOlympiad.Id && x.Code == "CHCC");
                var entry = context.Entrants.FirstOrDefault(x => x.OlympiadId == currentOlympiad.Id
                                        && x.Game_Code == "CHCC" && x.Mind_Sport_ID == standing.ContestantId);
                if (entry == null)
                {
                    entry = Entrant.NewEntrant(evt.EIN, "CHCC", currentOlympiad.Id, contestant, 0m);
                    context.Entrants.Add(entry);
                }
                entry.Score = standing.TotalScoreStr;
                entry.Rank = rank;
                entry.Medal = MedalForRank(rank);
                rank++;
                context.SaveChanges();
            }
        }

        public void FreezeBackgammon()
        {
            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = context.Olympiad_Infoes.First(x => x.Current);
            var pentamindStandingsGenerator = new PentamindStandingsGenerator();

            // Next a Modern Abstract one
            var modernAbstractStandings = pentamindStandingsGenerator.GetBackgammonStandings(null);
            int rank = 1;
            foreach (var standing in modernAbstractStandings.Standings)
            {
                if (!standing.IsValid) continue;

                var contestant = context.Contestants.FirstOrDefault(x => x.Mind_Sport_ID == standing.ContestantId);
                var evt = context.Events.FirstOrDefault(x => x.OlympiadId == currentOlympiad.Id && x.Code == "BACC");
                var entry = context.Entrants.FirstOrDefault(x => x.OlympiadId == currentOlympiad.Id
                                        && x.Game_Code == "BACC" && x.Mind_Sport_ID == standing.ContestantId);
                if (entry == null)
                {
                    entry = Entrant.NewEntrant(evt.EIN, "BACC", currentOlympiad.Id, contestant, 0m);
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
