using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MHArmory.Core.DataStructures;
using MHArmory.Search.Contracts;

namespace MHArmory.Search.Cutoff
{
    public class CutoffSearch : ISolver
    {
        public string Name { get; } = "Cutoff solver";
        public string Author { get; } = "Gediminas Masaitis";
        public string Description { get; } = "Tree search with cutoffs";
        public int Version { get; } = 1;

        public event Action<double> SearchProgress;

        public static CutoffSearch Instance { get; } = new CutoffSearch(new Mapper(), new SupersetMaker(), new SearchResultVerifier());

        // I'm not sure how to consume this data. I think some GUI changes will have to be made
        public event EventHandler<CutoffStatistics> ProgressChanged;

        private readonly Mapper mapper;
        private readonly SupersetMaker supersetMaker;
        private readonly SearchResultVerifier verifier;

        private CutoffStatistics statistics;

        private CutoffSearch(Mapper mapper, SupersetMaker supersetMaker, SearchResultVerifier verifier)
        {
            this.mapper = mapper;
            this.supersetMaker = supersetMaker;
            this.verifier = verifier;
        }

        private void InvokeProgressChanged()
        {
            ProgressChanged?.Invoke(this, statistics);
            SearchProgress?.Invoke(0d);
        }

        
        public Task<IList<ArmorSetSearchResult>> SearchArmorSets(ISolverData solverData, CancellationToken cancellationToken)
        {
            statistics = new CutoffStatistics();
            var heads = solverData.AllHeads.Where(x => x.IsSelected).Select(x => x.Equipment).ToList();
            var chests = solverData.AllChests.Where(x => x.IsSelected).Select(x => x.Equipment).ToList();
            var gloves = solverData.AllGloves.Where(x => x.IsSelected).Select(x => x.Equipment).ToList();
            var waists = solverData.AllWaists.Where(x => x.IsSelected).Select(x => x.Equipment).ToList();
            var legs = solverData.AllLegs.Where(x => x.IsSelected).Select(x => x.Equipment).ToList();
            var charms = solverData.AllCharms.Where(x => x.IsSelected).Select(x => x.Equipment).ToList();

            IList<IList<IEquipment>> allArmorPieces = new List<IList<IEquipment>>
            {
                heads,
                chests,
                gloves,
                waists,
                legs,
                charms
            };

            statistics.Init(allArmorPieces);
            
            MappingResults maps = mapper.MapEverything(allArmorPieces, solverData.DesiredAbilities, solverData.AllJewels);
            SupersetInfo[] supersets = allArmorPieces.Select(list => supersetMaker.CreateSupersetModel(list, solverData.DesiredAbilities)).ToArray();
            MappedEquipment[] supersetMaps = mapper.MapSupersets(supersets, maps);

            IList<ArmorSetSearchResult> results = new List<ArmorSetSearchResult>();

            var combination = new Combination(supersetMaps, solverData.WeaponSlots, maps);
            ParallelizedDepthFirstSearch(combination, 0, maps.Equipment, supersetMaps, results, cancellationToken);
            //DepthFirstSearch(combination, 0, maps.Equipment, supersetMaps, results, new object(), ct);
            //statistics.Dump();
            var resultTask = Task.FromResult(results);
            return resultTask;
        }

        private void ParallelizedDepthFirstSearch(Combination combination, int depth, MappedEquipment[][] allEquipment, MappedEquipment[] supersets, IList<ArmorSetSearchResult> results, CancellationToken ct)
        {
            object sync = new object();
            Parallel.ForEach(allEquipment[depth], equipment =>
            {
                var combinationCopy = new Combination(combination);
                combinationCopy.Replace(depth, equipment);
                DepthFirstSearch(combinationCopy, depth + 1, allEquipment, supersets, results, sync, ct);
            });
        }

        private void DepthFirstSearch(Combination combination, int depth, MappedEquipment[][] allEquipment, MappedEquipment[] supersets, IList<ArmorSetSearchResult> results, object sync, CancellationToken ct)
        {
            if (ct.IsCancellationRequested || results.Count >= 100000)
            {
                return;
            }
            if (depth == allEquipment.Length)
            {
                bool match = verifier.TryGetSearchResult(combination, false, out ArmorSetSearchResult result);
                statistics.RealSearch(match, InvokeProgressChanged);
                if (match)
                {
                    lock (sync)
                    {
                        results.Add(result);
                    }
                }
                return;
            }

            bool supersetMatch = verifier.TryGetSearchResult(combination, true,  out _);
            statistics.SupersetSearch(depth, supersetMatch, InvokeProgressChanged);
            if (!supersetMatch)
            {
                return;
            }

            int depthLength = allEquipment[depth].Length;
            for (int i = 0; i < depthLength; i++)
            {
                combination.Replace(depth, allEquipment[depth][i]);
                DepthFirstSearch(combination, depth + 1, allEquipment, supersets, results, sync, ct);
            }
            combination.Replace(depth, supersets[depth]);
        }
    }
}
