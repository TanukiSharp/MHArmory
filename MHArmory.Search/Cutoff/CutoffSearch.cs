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
            var heads = solverData.AllHeads.Where(x => x.IsSelected).Select(x => (IArmorPiece)x.Equipment).ToList();
            var chests = solverData.AllChests.Where(x => x.IsSelected).Select(x => (IArmorPiece)x.Equipment).ToList();
            var gloves = solverData.AllGloves.Where(x => x.IsSelected).Select(x => (IArmorPiece)x.Equipment).ToList();
            var waists = solverData.AllWaists.Where(x => x.IsSelected).Select(x => (IArmorPiece)x.Equipment).ToList();
            var legs = solverData.AllLegs.Where(x => x.IsSelected).Select(x => (IArmorPiece)x.Equipment).ToList();
            var charms = solverData.AllCharms.Where(x => x.IsSelected).Select(x => x.Equipment).ToList();

            IList<IList<IArmorPiece>> allArmorPieces = new List<IList<IArmorPiece>>
            {
                heads,
                chests,
                gloves,
                waists,
                legs
            };

            IList<IFullArmorSet> fullSets = FilterAndRemoveFullSetEquipment(allArmorPieces);

            IList<IList<IEquipment>> allEquipment = new List<IList<IEquipment>>(allArmorPieces.Select(x => x.Cast<IEquipment>().ToList()))
            {
                charms
            };

            statistics.Init(allEquipment);
            
            MappingResults maps = mapper.MapEverything(allEquipment, solverData.DesiredAbilities, solverData.AllJewels);

            SupersetInfo[] supersets = allEquipment.Select(list => supersetMaker.CreateSupersetModel(list, solverData.DesiredAbilities)).ToArray();
            MappedEquipment[] supersetMaps = mapper.MapSupersets(supersets, maps);

            IList<ArmorSetSearchResult> results = new List<ArmorSetSearchResult>();

            FullSetSearch(fullSets, solverData.WeaponSlots, solverData.DesiredAbilities, charms, solverData.AllJewels, results, cancellationToken);

            var combination = new Combination(supersetMaps, solverData.WeaponSlots, maps);
            ParallelizedDepthFirstSearch(combination, 0, maps.Equipment, supersetMaps, results, cancellationToken);
            //DepthFirstSearch(combination, 0, maps.Equipment, supersetMaps, results, new object(), ct);
            //statistics.Dump();
            var resultTask = Task.FromResult(results);
            return resultTask;
        }

        private void FullSetSearch(IList<IFullArmorSet> fullSets, int[] weaponSlots, IAbility[] desiredAbilities, IList<IEquipment> charms, IList<SolverDataJewelModel> jewels, IList<ArmorSetSearchResult> results, CancellationToken cancellationToken)
        {
            foreach (IFullArmorSet fullSet in fullSets)
            {
                var allEquipment = fullSet.ArmorPieces.Select(x => (IList<IEquipment>)new List<IEquipment>() {x}).ToList();
                allEquipment.Add(charms);
                MappingResults maps = mapper.MapEverything(allEquipment, desiredAbilities, jewels);
                SupersetInfo[] supersets = allEquipment.Select(list => supersetMaker.CreateSupersetModel(list, desiredAbilities)).ToArray();
                MappedEquipment[] supersetMaps = mapper.MapSupersets(supersets, maps);
                var combination = new Combination(supersetMaps, weaponSlots, maps);
                DepthFirstSearch(combination, 0, maps.Equipment, supersetMaps, results, new object(), cancellationToken);
            }
        }

        private IList<IFullArmorSet> FilterAndRemoveFullSetEquipment(IList<IList<IArmorPiece>> allArmorPieces)
        {
            var fullSetPieces = new List<IArmorPiece>();
            for (int i = 0; i < allArmorPieces.Count; i++)
            {
                IList<IArmorPiece> equipments = allArmorPieces[i];
                var nonFullSetPieceList = new List<IArmorPiece>();
                foreach (IArmorPiece equipment in equipments)
                {
                    if (equipment.FullArmorSet != null)
                    {
                        fullSetPieces.Add(equipment);
                    }
                    else
                    {
                        nonFullSetPieceList.Add(equipment);
                    }
                }
                allArmorPieces[i] = nonFullSetPieceList.ToArray();
            }
            var fullSetsDictionary = new Dictionary<int, IFullArmorSet>();
            foreach (IArmorPiece fullSetPiece in fullSetPieces)
            {
                fullSetsDictionary[fullSetPiece.FullArmorSet.Id] = fullSetPiece.FullArmorSet;
            }
            return fullSetsDictionary.Values.ToList();
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
