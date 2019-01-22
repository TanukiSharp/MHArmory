using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using MHArmory.Core.DataStructures;

namespace MHArmory.Search.Cutoff
{
    public class CutoffStatistics
    {
        private long combinationsPossible;
        private long realSearches;
        private long[] SupersetSearches;
        private long[] Cutoffs;
        private long[] Savings;
        private long Results;

        private Stopwatch Stopwatch { get; }

        private object realSync;
        private object superSync;

        public CutoffStatistics()
        {
            Stopwatch = new Stopwatch();
            Stopwatch.Start();
        }

        public void Init(int[] equipmentCounts)
        {
            SupersetSearches = new long[equipmentCounts.Length];
            Cutoffs = new long[equipmentCounts.Length];
            Savings = new long[equipmentCounts.Length];
            combinationsPossible = 1;
            for (int i = equipmentCounts.Length - 1; i >= 0; i--)
            {
                combinationsPossible *= equipmentCounts[i];
                Savings[i] = combinationsPossible;
            }
            realSync = new object();
            superSync = new object();
        }

        public void RealSearch(bool match)
        {
            lock (realSync)
            {
                realSearches++;
                if (match)
                {
                    Results++;
                }
            }
        }

        public void SupersetSearch(int depth, bool match)
        {
            lock (superSync)
            {
                SupersetSearches[depth]++;
                if (!match)
                {
                    Cutoffs[depth]++;
                }
            }
        }

        public double GetCurrentProgress()
        {
            long supposedlySearched = realSearches;
            for (int i = 0; i < Cutoffs.Length; i++)
            {
                supposedlySearched += Cutoffs[i] * Savings[i];
            }
            double progress = supposedlySearched / (double) combinationsPossible;
            return progress;
        }

        public void Dump()
        {
            Stopwatch.Stop();
            File.WriteAllText("stats.txt", ToString());
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Total search time: {Stopwatch.Elapsed:c}");
            sb.AppendLine();
            sb.AppendLine($"Combinations possible: {combinationsPossible:N0}");
            sb.AppendLine($"Real combinations searched: {realSearches:N0}");
            long totalSupersetSearches = SupersetSearches.Sum();
            sb.AppendLine($"Superset combinations searched: {totalSupersetSearches:N0}");
            long totalSearches = realSearches + totalSupersetSearches;
            sb.AppendLine($"Total combinations searched: {totalSearches:N0}");
            sb.AppendLine($"Results: {Results:N0}");
            sb.AppendLine();

            double treeCoverage = (double)realSearches / combinationsPossible;
            sb.AppendLine($"Search tree coverage: {treeCoverage:P5} (less is better)");
            double searchPercentage = (double)totalSearches / combinationsPossible;
            sb.AppendLine($"Search efficiency: {searchPercentage:P5} (less is better)");
            sb.AppendLine();

            sb.AppendLine($"Cutoffs: (count * saved per cutoff) - supersearches = saved by supersets - supersearches = saved total");
            for (int i = 0; i < Cutoffs.Length; i++)
            {
                long cutoffs = Cutoffs[i];
                long savings = Savings[i];
                long supersetSearches = SupersetSearches[i];
                long savedBySuperset = cutoffs * savings;
                long savedTotal = savedBySuperset - supersetSearches;
                sb.AppendLine($"Depth {i}: ({cutoffs:N0} * {savings:N0}) - {supersetSearches:N0} = {savedBySuperset:N0} - {supersetSearches:N0} = {savedTotal:N0}");
            }
            return sb.ToString();
        }
    }
}
