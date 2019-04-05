using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MHArmory.Core;
using MHArmory.Core.DataStructures;
using MHArmory.Search.Contracts;

namespace MHArmory.Search.Default
{
    public sealed class Solver : ISolver, IDisposable
    {
        private int currentCombinations;
        private double totalCombinations;

        public event Action<double> SearchProgress;

        public string Name { get; } = "Armory - Default";
        public string Author { get; } = "TaukiSharp";
        public string Description { get; } = "Default search algorithm";
        public int Version { get; } = 1;

        public void Dispose()
        {
            jewelResultObjectPool.Dispose();
            availableSlotsObjectPool.Dispose();
            armorSetSkillPartsObjectPool.Dispose();
            searchEquipmentsObjectPool.Dispose();

            SearchProgress = null;
        }

        public async Task<IList<ArmorSetSearchResult>> SearchArmorSets(ISolverData solverData, CancellationToken cancellationToken)
        {
            var innerCancellation = new CancellationTokenSource();

            Task updateTask = UpdateSearchProgression(innerCancellation.Token, cancellationToken);

            IList<ArmorSetSearchResult> result = await Task.Factory.StartNew(() =>
            {
                return SearchArmorSetsInternal(
                    solverData,
                    cancellationToken
                );
            }, TaskCreationOptions.LongRunning).Unwrap();

            innerCancellation.Cancel();

            await updateTask;

            if (cancellationToken.IsCancellationRequested == false)
                SearchProgress?.Invoke(1.0);

            return result;
        }

        private async Task UpdateSearchProgression(CancellationToken innerCancel, CancellationToken cancellationToken)
        {
            while (innerCancel.IsCancellationRequested == false && cancellationToken.IsCancellationRequested == false)
            {
                if (totalCombinations > 0)
                    SearchProgress?.Invoke(currentCombinations / totalCombinations);

                await Task.Delay(250);
            }
        }

        private readonly ObjectPool<List<ArmorSetJewelResult>> jewelResultObjectPool = new ObjectPool<List<ArmorSetJewelResult>>(() => new List<ArmorSetJewelResult>());
        private readonly ObjectPool<int[]> availableSlotsObjectPool = new ObjectPool<int[]>(() => new int[3]);
        private readonly ObjectPool<Dictionary<IArmorSetSkillPart, int>> armorSetSkillPartsObjectPool = new ObjectPool<Dictionary<IArmorSetSkillPart, int>>(() => new Dictionary<IArmorSetSkillPart, int>(ArmorSetSkillPartEqualityComparer.Default));
        private readonly ObjectPool<IEquipment[]> searchEquipmentsObjectPool = new ObjectPool<IEquipment[]>(() => new IEquipment[6]);

        private async Task<IList<ArmorSetSearchResult>> SearchArmorSetsInternal(
            ISolverData data,
            CancellationToken cancellationToken
        )
        {
            if (cancellationToken.IsCancellationRequested)
                return null;

            var allCharms = new List<ICharmLevel>();

            if (cancellationToken.IsCancellationRequested)
                return null;

            var heads = new List<IArmorPiece>();
            var chests = new List<IArmorPiece>();
            var gloves = new List<IArmorPiece>();
            var waists = new List<IArmorPiece>();
            var legs = new List<IArmorPiece>();

            var test = new List<ArmorSetSearchResult>();

            var generator = new EquipmentCombinationGenerator(
                searchEquipmentsObjectPool,
                data.AllHeads.Where(x => x.IsSelected).Select(x => x.Equipment),
                data.AllChests.Where(x => x.IsSelected).Select(x => x.Equipment),
                data.AllGloves.Where(x => x.IsSelected).Select(x => x.Equipment),
                data.AllWaists.Where(x => x.IsSelected).Select(x => x.Equipment),
                data.AllLegs.Where(x => x.IsSelected).Select(x => x.Equipment),
                data.AllCharms.Where(x => x.IsSelected).Select(x => x.Equipment)
            );

            var sb = new StringBuilder();

            long hh = data.AllHeads.Count(x => x.IsSelected);
            long cc = data.AllChests.Count(x => x.IsSelected);
            long gg = data.AllGloves.Count(x => x.IsSelected);
            long ww = data.AllWaists.Count(x => x.IsSelected);
            long ll = data.AllLegs.Count(x => x.IsSelected);
            long ch = data.AllCharms.Count(x => x.IsSelected);

            long combinationCount =
                Math.Max(hh, 1) *
                Math.Max(cc, 1) *
                Math.Max(gg, 1) *
                Math.Max(ww, 1) *
                Math.Max(ll, 1) *
                Math.Max(ch, 1);

            await Task.Yield();

            var parallelOptions = new ParallelOptions
            {
                //MaxDegreeOfParallelism = 1, // to ease debugging
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            currentCombinations = 0;
            totalCombinations = combinationCount;

            ParallelLoopResult parallelResult;

            try
            {
                OrderablePartitioner<IEquipment[]> partitioner = Partitioner.Create(generator.All(cancellationToken), EnumerablePartitionerOptions.NoBuffering);

                parallelResult = Parallel.ForEach(partitioner, parallelOptions, equips =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        searchEquipmentsObjectPool.PutObject(equips);
                        return;
                    }

                    ArmorSetSearchResult searchResult = IsArmorSetMatching(data.WeaponSlots, equips, data.AllJewels, data.DesiredAbilities);

                    Interlocked.Increment(ref currentCombinations);

                    if (searchResult.IsMatch)
                    {
                        searchResult.ArmorPieces = new IArmorPiece[]
                        {
                            (IArmorPiece)equips[0],
                            (IArmorPiece)equips[1],
                            (IArmorPiece)equips[2],
                            (IArmorPiece)equips[3],
                            (IArmorPiece)equips[4],
                        };
                        searchResult.Charm = (ICharmLevel)equips[5];

                        lock (test)
                        {
                            test.Add(searchResult);
                        }
                    }

                    searchEquipmentsObjectPool.PutObject(equips);
                });
            }
            finally
            {
                generator.Reset();
            }

            return test;
        }

        private static bool IsAnyFullArmorSet(IEquipment[] equipments)
        {
            foreach (IEquipment equipment in equipments)
            {
                if (equipment is IArmorPiece armorPiece && armorPiece.FullArmorSet != null)
                    return true;
            }

            return false;
        }

        private ArmorSetSearchResult IsArmorSetMatching(
            int[] weaponSlots, IEquipment[] equipments,
            SolverDataJewelModel[] matchingJewels,
            IAbility[] desiredAbilities
        )
        {
            List<ArmorSetJewelResult> requiredJewels = jewelResultObjectPool.GetObject();
            int[] availableSlots = availableSlotsObjectPool.GetObject();

            void OnArmorSetMismatch()
            {
                requiredJewels.Clear();
                jewelResultObjectPool.PutObject(requiredJewels);

                for (int i = 0; i < availableSlots.Length; i++)
                    availableSlots[i] = 0;
                availableSlotsObjectPool.PutObject(availableSlots);
            }

            if (IsAnyFullArmorSet(equipments))
            {
                if (DataUtility.AreOnSameFullArmorSet(equipments) == false)
                {
                    OnArmorSetMismatch();
                    return ArmorSetSearchResult.NoMatch;
                }
            }

            if (weaponSlots != null)
            {
                foreach (int slotSize in weaponSlots)
                {
                    if (slotSize > 0)
                        availableSlots[slotSize - 1]++;
                }
            }

            foreach (IEquipment equipment in equipments)
            {
                if (equipment == null)
                    continue;

                foreach (int slotSize in equipment.Slots)
                    availableSlots[slotSize - 1]++;
            }

            foreach (IAbility ability in desiredAbilities)
            {
                int armorAbilityTotal = 0;

                if (IsAbilityMatchingArmorSet(ability, equipments))
                    continue;

                foreach (IEquipment equipment in equipments)
                {
                    if (equipment != null)
                    {
                        foreach (IAbility a in equipment.Abilities)
                        {
                            if (a.Skill.Id == ability.Skill.Id)
                                armorAbilityTotal += a.Level;
                        }
                    }
                }

                int remaingAbilityLevels = ability.Level - armorAbilityTotal;

                if (remaingAbilityLevels > 0)
                {
                    bool isAll = true;

                    foreach (int x in availableSlots)
                    {
                        if (x > 0)
                        {
                            isAll = false;
                            break;
                        }
                    }

                    if (isAll)
                    {
                        OnArmorSetMismatch();
                        return ArmorSetSearchResult.NoMatch;
                    }

                    foreach (SolverDataJewelModel j in matchingJewels)
                    {
                        // bold assumption, will be fucked if they decide to create jewels with multiple skills
                        IAbility a = j.Jewel.Abilities[0];

                        if (a.Skill.Id == ability.Skill.Id)
                        {
                            int requiredJewelCount = remaingAbilityLevels / a.Level;

                            if (j.Available < requiredJewelCount)
                            {
                                OnArmorSetMismatch();
                                return ArmorSetSearchResult.NoMatch;
                            }

                            if (ConsumeSlots(availableSlots, j.Jewel.SlotSize, requiredJewelCount) == false)
                            {
                                OnArmorSetMismatch();
                                return ArmorSetSearchResult.NoMatch;
                            }

                            remaingAbilityLevels -= requiredJewelCount * a.Level;

                            requiredJewels.Add(new ArmorSetJewelResult { Jewel = j.Jewel, Count = requiredJewelCount });

                            break;
                        }
                    }

                    if (remaingAbilityLevels > 0)
                    {
                        OnArmorSetMismatch();
                        return ArmorSetSearchResult.NoMatch;
                    }
                }
            }

            return new ArmorSetSearchResult
            {
                IsMatch = true,
                Jewels = requiredJewels,
                SpareSlots = availableSlots
            };
        }

        private bool IsAbilityMatchingArmorSet(IAbility ability, IEquipment[] armorPieces)
        {
            Dictionary<IArmorSetSkillPart, int> armorSetSkillParts = armorSetSkillPartsObjectPool.GetObject();

            void Done()
            {
                armorSetSkillParts.Clear();
                armorSetSkillPartsObjectPool.PutObject(armorSetSkillParts);
            }

            foreach (IEquipment equipment in armorPieces)
            {
                var armorPiece = equipment as IArmorPiece;

                if (armorPiece == null)
                    continue;

                if (armorPiece.ArmorSetSkills == null)
                    continue;

                foreach (IArmorSetSkill armorSetSkill in armorPiece.ArmorSetSkills)
                {
                    foreach (IArmorSetSkillPart armorSetSkillPart in armorSetSkill.Parts)
                    {
                        foreach (IAbility a in armorSetSkillPart.GrantedSkills)
                        {
                            if (a.Skill.Id == ability.Skill.Id)
                            {
                                if (armorSetSkillParts.TryGetValue(armorSetSkillPart, out int value) == false)
                                    value = 0;

                                armorSetSkillParts[armorSetSkillPart] = value + 1;
                            }
                        }
                    }
                }
            }

            if (armorSetSkillParts.Count > 0)
            {
                foreach (KeyValuePair<IArmorSetSkillPart, int> armorSetSkillPartKeyValue in armorSetSkillParts)
                {
                    if (armorSetSkillPartKeyValue.Value >= armorSetSkillPartKeyValue.Key.RequiredArmorPieces)
                    {
                        foreach (IAbility x in armorSetSkillPartKeyValue.Key.GrantedSkills)
                        {
                            if (x.Skill.Id == ability.Skill.Id)
                            {
                                Done();
                                return true;
                            }
                        }
                    }
                }
            }

            Done();
            return false;
        }

        private static bool ConsumeSlots(int[] availableSlots, int jewelSize, int jewelCount)
        {
            for (int i = jewelSize - 1; i < availableSlots.Length; i++)
            {
                if (availableSlots[i] <= 0)
                    continue;

                if (availableSlots[i] >= jewelCount)
                {
                    availableSlots[i] -= jewelCount;
                    return true;
                }
                else
                {
                    jewelCount -= availableSlots[i];
                    availableSlots[i] = 0;
                }
            }

            return jewelCount <= 0;
        }

        private class EquipmentCombinationGenerator
        {
            private readonly object syncRoot = new object();
            private readonly ObjectPool<IEquipment[]> searchEquipmentsObjectPool;
            private readonly IList<IEquipment>[] allEquipments;
            private readonly int[] indexes;
            private bool isEnd;

            public int CombinationCount { get; }

            public EquipmentCombinationGenerator(
                ObjectPool<IEquipment[]> searchEquipmentsObjectPool,
                IEnumerable<IEquipment> heads,
                IEnumerable<IEquipment> chests,
                IEnumerable<IEquipment> gloves,
                IEnumerable<IEquipment> waists,
                IEnumerable<IEquipment> legs,
                IEnumerable<IEquipment> charms
            )
            {
                this.searchEquipmentsObjectPool = searchEquipmentsObjectPool;

                allEquipments = new IList<IEquipment>[]
                {
                    heads.ToList(),
                    chests.ToList(),
                    gloves.ToList(),
                    waists.ToList(),
                    legs.ToList(),
                    charms.ToList()
                };

                indexes = new int[allEquipments.Length];

                if (allEquipments.All(x => x.Count == 0))
                    CombinationCount = 0;
                else
                {
                    int combinationCount = 1;
                    for (int i = 0; i < allEquipments.Length; i++)
                        combinationCount *= Math.Max(allEquipments[i].Count, 1);
                    CombinationCount = combinationCount;
                }
            }

            private bool Increment()
            {
                for (int i = 0; i < indexes.Length; i++)
                {
                    indexes[i]++;

                    if (indexes[i] < allEquipments[i].Count)
                        return true;

                    indexes[i] = 0;
                }

                return false;
            }

            public IEquipment[] Next(CancellationToken cancellationToken)
            {
                if (cancellationToken.IsCancellationRequested)
                    return null;

                lock (syncRoot)
                {
                    if (isEnd)
                        return null;

                    IEquipment[] equipments = searchEquipmentsObjectPool.GetObject();

                    for (int i = 0; i < equipments.Length; i++)
                    {
                        IList<IEquipment> allCategoryEquipments = allEquipments[i];

                        if (allCategoryEquipments.Count > indexes[i])
                            equipments[i] = allCategoryEquipments[indexes[i]];
                        else
                            equipments[i] = null;
                    }

                    isEnd = Increment() == false;

                    return equipments;
                }
            }

            public IEnumerable<IEquipment[]> All(CancellationToken cancellationToken)
            {
                IEquipment[] result;

                while ((result = Next(cancellationToken)) != null)
                    yield return result;
            }

            public void Reset()
            {
                for (int i = 0; i < indexes.Length; i++)
                    indexes[i] = 0;

                isEnd = false;
            }
        }

        private class ArmorSetSkillPartEqualityComparer : IEqualityComparer<IArmorSetSkillPart>
        {
            public static readonly IEqualityComparer<IArmorSetSkillPart> Default = new ArmorSetSkillPartEqualityComparer();

            public bool Equals(IArmorSetSkillPart x, IArmorSetSkillPart y)
            {
                if (x == null || y == null)
                    return false;

                return x.Id == y.Id;
            }

            public int GetHashCode(IArmorSetSkillPart obj)
            {
                if (obj == null)
                    return 0;

                return obj.Id;
            }
        }
    }
}
