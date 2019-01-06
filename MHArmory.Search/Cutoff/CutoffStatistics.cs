using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using MHArmory.Core.DataStructures;

namespace MHArmory.Search.Cutoff
{
    internal class CutoffStatistics
    {
        public int CombinationsPossible { get; private set; }
        public int RealSearches { get; private set; }
        public int[] SupersetSearches { get; private set; }
        public int[] Cutoffs { get; private set; }
        public int[] Savings { get; private set; }
        public int Results { get; private set; }

        private Stopwatch Stopwatch { get; }

        private object realsync;
        private object supersync;

        public CutoffStatistics()
        {
            Stopwatch = new Stopwatch();
            Stopwatch.Start();
        }

        [Conditional("DEBUG")]
        public void Init(IList<IList<IEquipment>> allArmorPieces)
        {
            SupersetSearches = new int[allArmorPieces.Count];
            Cutoffs = new int[allArmorPieces.Count];
            Savings = new int[allArmorPieces.Count];
            CombinationsPossible = 1;
            for (int i = allArmorPieces.Count - 1; i >= 0; i--)
            {
                IList<IEquipment> equipments = allArmorPieces[i];
                CombinationsPossible *= equipments.Count;
                Savings[i] = CombinationsPossible;
            }
            realsync = new object();
            supersync = new object();
        }

        [Conditional("DEBUG")]
        public void RealSearch(bool match)
        {
            lock (realsync)
            {
                RealSearches++;
                if (match)
                {
                    Results++;
                }
            }
        }

        [Conditional("DEBUG")]
        public void SupersetSearch(int depth, bool match)
        {
            lock (supersync)
            {
                SupersetSearches[depth]++;
                if (!match)
                {
                    Cutoffs[depth]++;
                }
            }
        }

        [Conditional("DEBUG")]
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
            int totalSupersetSearches = SupersetSearches.Sum();
            sb.AppendLine($"Superset combinations searched: {totalSupersetSearches:N0}");
            int totalSearches = RealSearches + totalSupersetSearches;
            sb.AppendLine($"Total combinations searched: {totalSearches:N0}");
            sb.AppendLine($"Results: {Results:N0}");
            sb.AppendLine();

            double treeCoverage = (double)RealSearches / CombinationsPossible;
            sb.AppendLine($"Search tree coverage: {treeCoverage:P2} (less is better)");
            double searchPercentage = (double)totalSearches / CombinationsPossible;
            sb.AppendLine($"Search efficiency: {searchPercentage:P2} (less is better)");
            sb.AppendLine();

            sb.AppendLine($"Cutoffs: (count * saved per cutoff) - supersearches = saved by supersets - supersearches = saved total");
            for (int i = 0; i < Cutoffs.Length; i++)
            {
                int cutoffs = Cutoffs[i];
                int savings = Savings[i];
                int supersetSearches = SupersetSearches[i];
                int savedBySuperset = cutoffs * savings;
                int savedTotal = savedBySuperset - supersetSearches;
                sb.AppendLine($"Depth {i}: ({cutoffs:N0} * {savings:N0}) - {supersetSearches:N0} = {savedBySuperset:N0} - {supersetSearches:N0} = {savedTotal:N0}");
            }
            return sb.ToString();
        }
    }
}