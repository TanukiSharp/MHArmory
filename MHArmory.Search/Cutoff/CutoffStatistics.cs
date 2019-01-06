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
        public long CombinationsPossible { get; private set; }
        public long RealSearches { get; private set; }
        public long[] SupersetSearches { get; private set; }
        public long[] Cutoffs { get; private set; }
        public long[] Savings { get; private set; }
        public long Results { get; private set; }

        private Stopwatch Stopwatch { get; }

        private object realSync;
        private object superSync;

        public CutoffStatistics()
        {
            Stopwatch = new Stopwatch();
            Stopwatch.Start();
        }

        public void Init(IList<IList<IEquipment>> allArmorPieces)
        {
            SupersetSearches = new long[allArmorPieces.Count];
            Cutoffs = new long[allArmorPieces.Count];
            Savings = new long[allArmorPieces.Count];
            CombinationsPossible = 1;
            for (int i = allArmorPieces.Count - 1; i >= 0; i--)
            {
                IList<IEquipment> equipments = allArmorPieces[i];
                CombinationsPossible *= equipments.Count;
                Savings[i] = CombinationsPossible;
            }
            realSync = new object();
            superSync = new object();
        }

        public void RealSearch(bool match, Action afterUpdate)
        {
            lock (realSync)
            {
                RealSearches++;
                if (match)
                {
                    Results++;
                }
                afterUpdate?.Invoke();
            }
        }

        public void SupersetSearch(int depth, bool match, Action afterUpdate)
        {
            lock (superSync)
            {
                SupersetSearches[depth]++;
                if (!match)
                {
                    Cutoffs[depth]++;
                }
                afterUpdate?.Invoke();
            }
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
            sb.AppendLine($"Combinations possible: {CombinationsPossible:N0}");
            sb.AppendLine($"Real combinations searched: {RealSearches:N0}");
            long totalSupersetSearches = SupersetSearches.Sum();
            sb.AppendLine($"Superset combinations searched: {totalSupersetSearches:N0}");
            long totalSearches = RealSearches + totalSupersetSearches;
            sb.AppendLine($"Total combinations searched: {totalSearches:N0}");
            sb.AppendLine($"Results: {Results:N0}");
            sb.AppendLine();

            double treeCoverage = (double)RealSearches / CombinationsPossible;
            sb.AppendLine($"Search tree coverage: {treeCoverage:P5} (less is better)");
            double searchPercentage = (double)totalSearches / CombinationsPossible;
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
