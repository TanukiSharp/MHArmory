using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MHArmory.Core.DataStructures;
using MHArmory.Search.Contracts;
using MHArmory.Search.Cutoff.Models;
using MHArmory.Search.Cutoff.Models.Mapped;
using MHArmory.Search.Cutoff.Services;

namespace MHArmory.Search.Cutoff
{
    public class CutoffSearch : ISolver
    {
        public string Name { get; } = "Cutoff solver";
        public string Author { get; } = "Gediminas Masaitis";
        public string Description { get; } = "Tree search with cutoffs";
        public int Version { get; } = 1;

        public static CutoffSearch Instance { get; } = new CutoffSearch(
            new MappedCutoffSearchService(new SearchResultVerifier()),
            new Mapper(),
            new SupersetMaker()
         );

        public bool SearchNullPieces { get; set; }

        private readonly IMappedCutoffSearchService mappedCutoffSearch;
        private readonly IMapper mapper;
        private readonly ISupersetMaker supersetMaker;

        public event Action<double> SearchProgress
        {
            add => mappedCutoffSearch.SearchProgress += value;
            remove => mappedCutoffSearch.SearchProgress -= value;
        }

        private CutoffSearch(IMappedCutoffSearchService mappedCutoffSearch, IMapper mapper, ISupersetMaker supersetMaker)
        {
            SearchNullPieces = true;

            this.mappedCutoffSearch = mappedCutoffSearch;
            this.mapper = mapper;
            this.supersetMaker = supersetMaker;
        }

        public Task<IList<ArmorSetSearchResult>> SearchArmorSets(ISolverData solverData, CancellationToken cancellationToken)
        {
            return Task.Run(() => SearchArmorSetsInner(solverData, cancellationToken));
        }

        public IList<ArmorSetSearchResult> SearchArmorSetsInner(ISolverData solverData, CancellationToken cancellationToken)
        {
            var heads = solverData.AllHeads.Where(x => x.IsSelected).Select(x => (IArmorPiece) x.Equipment).ToList();
            var chests = solverData.AllChests.Where(x => x.IsSelected).Select(x => (IArmorPiece) x.Equipment).ToList();
            var gloves = solverData.AllGloves.Where(x => x.IsSelected).Select(x => (IArmorPiece) x.Equipment).ToList();
            var waists = solverData.AllWaists.Where(x => x.IsSelected).Select(x => (IArmorPiece) x.Equipment).ToList();
            var legs = solverData.AllLegs.Where(x => x.IsSelected).Select(x => (IArmorPiece) x.Equipment).ToList();
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
            IList<IList<IEquipment>> allEquipment = allArmorPieces.Select(x => (IList<IEquipment>)x.Cast<IEquipment>().ToList()).ToList();
            allEquipment.Add(charms);
            MappingResults maps = mapper.MapEverything(allEquipment, solverData.DesiredAbilities, solverData.AllJewels, SearchNullPieces);

            int[] mapLengths = maps.Equipment.Select(x => x.Length).ToArray();
            var statistics = new CutoffStatistics();
            statistics.Init(mapLengths);
            IList<ArmorSetSearchResult> results = new List<ArmorSetSearchResult>();

            FullSetSearch(statistics, fullSets, solverData.WeaponSlots, solverData.DesiredAbilities, charms, solverData.AllJewels, results, cancellationToken);

            IList<SupersetInfo> supersets = allEquipment
                .Select(list => supersetMaker.CreateSupersetModel(list, solverData.DesiredAbilities))
                .ToList();
            MappedEquipment[] supersetMaps = mapper.MapSupersets(supersets, maps);
            var combination = new Combination(supersetMaps, solverData.WeaponSlots, maps);

            var parameters = new MappedCutoffSearchParameters
            {
                Statistics = statistics,
                Combination = combination,
                AllEquipment = maps.Equipment,
                Supersets = supersetMaps,
                Results = results,
                Sync = new object()
            };
            mappedCutoffSearch.ParallelizedDepthFirstSearch(parameters, 0, cancellationToken);
            
            return results;
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

        private void FullSetSearch(CutoffStatistics statistics, IList<IFullArmorSet> fullSets, int[] weaponSlots, IAbility[] desiredAbilities,
            IList<IEquipment> charms, IList<SolverDataJewelModel> jewels, IList<ArmorSetSearchResult> results,
            CancellationToken cancellationToken)
        {
            foreach (IFullArmorSet fullSet in fullSets)
            {
                var allEquipment = fullSet.ArmorPieces
                    .Select(x => (IList<IEquipment>) new List<IEquipment>() {x})
                    .ToList();
                allEquipment.Add(charms);
                MappingResults maps = mapper.MapEverything(allEquipment, desiredAbilities, jewels, false);
                SupersetInfo[] supersets = allEquipment
                    .Select(list => supersetMaker.CreateSupersetModel(list, desiredAbilities))
                    .ToArray();
                MappedEquipment[] supersetMaps = mapper.MapSupersets(supersets, maps);
                var combination = new Combination(supersetMaps, weaponSlots, maps);

                var parameters = new MappedCutoffSearchParameters
                {
                    Statistics = statistics,
                    Combination = combination,
                    AllEquipment = maps.Equipment,
                    Supersets = supersetMaps,
                    Results = results,
                    Sync = new object()
                };
                mappedCutoffSearch.DepthFirstSearch(parameters, 0, cancellationToken);
            }
        }
    }
}
