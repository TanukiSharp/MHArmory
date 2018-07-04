using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MHArmory.Core;
using MHArmory.Core.DataStructures;
using MHArmory.Searching;

namespace MHArmory.ViewModels
{
    public enum MaximizedSearchCriteria
    {
        BaseDefense,
        MaxUnaugmentedDefense,
        MaxAugmentedDefense,
        Rarity,
        SlotCount,
        SlotSize,
        FireResistance,
        WaterResistance,
        ThunderResistance,
        IceResistance,
        DragonResistance,
    }

    public class RootViewModel : ViewModelBase
    {
        public ICommand OpenSkillSelectorCommand { get; }
        public ICommand SearchArmorSetsCommand { get; }

        private bool isDataLoading = true;
        public bool IsDataLoading
        {
            get { return isDataLoading; }
            set { SetValue(ref isDataLoading, value); }
        }

        private bool isDataLoaded;
        public bool IsDataLoaded
        {
            get { return isDataLoaded; }
            set { SetValue(ref isDataLoaded, value); }
        }

        private IEnumerable<AbilityViewModel> selectedAbilities;
        public IEnumerable<AbilityViewModel> SelectedAbilities
        {
            get { return selectedAbilities; }
            set { SetValue(ref selectedAbilities, value); }
        }

        private IEnumerable<ArmorSetViewModel> foundArmorSets;
        public IEnumerable<ArmorSetViewModel> FoundArmorSets
        {
            get { return foundArmorSets; }
            set { SetValue(ref foundArmorSets, value); }
        }

        public RootViewModel()
        {
            OpenSkillSelectorCommand = new AnonymousCommand(OpenSkillSelector);
            SearchArmorSetsCommand = new AnonymousCommand(SearchArmorSets);
        }

        private void OpenSkillSelector(object parameter)
        {
            RoutedCommands.OpenSkillsSelector.Execute(null);
        }

        private bool isSearching;
        public bool IsSearching
        {
            get { return isSearching; }
            private set { SetValue(ref isSearching, value); }
        }

        public async void SearchArmorSets()
        {
            if (IsSearching)
                return;

            IsSearching = true;

            try
            {
                await SearchArmorSetsInternal();
            }
            finally
            {
                IsSearching = false;
            }
        }

        private MaximizedSearchCriteria[] sortCriterias = new MaximizedSearchCriteria[]
        {
            MaximizedSearchCriteria.BaseDefense,
            MaximizedSearchCriteria.DragonResistance,
            MaximizedSearchCriteria.SlotSize
        };

        private async Task SearchArmorSetsInternal()
        {
            // ========================================================
            // above are inputs
            // ========================================================

            var sw = Stopwatch.StartNew();

            IDictionary<int, IArmorPiece[]> skillsToArmorsMap = await GlobalData.Instance.GetSkillsToArmorsMap();
            IDictionary<int, ICharm[]> skillsToCharmsMap = await GlobalData.Instance.GetSkillsToCharmsMap();
            IDictionary<int, IJewel[]> skillsToJewelsMap = await GlobalData.Instance.GetSkillsToJewelsMap();

            IArmorPiece[] allHeads = await GlobalData.Instance.GetHeads();
            IArmorPiece[] allChests = await GlobalData.Instance.GetChests();
            IArmorPiece[] allGloves = await GlobalData.Instance.GetGloves();
            IArmorPiece[] allWaists = await GlobalData.Instance.GetWaists();
            IArmorPiece[] allLegs = await GlobalData.Instance.GetLegs();

            int maxLength = Math.Max(allHeads.Length, Math.Max(allChests.Length, Math.Max(allGloves.Length, Math.Max(allWaists.Length, allLegs.Length))));
            int[] weights = new int[maxLength];

            var desiredAblities = SelectedAbilities.Where(x => x.IsChecked);

            allHeads = GetMaxWeightedArmorPieces(allHeads, weights, desiredAblities);
            allChests = GetMaxWeightedArmorPieces(allChests, weights, desiredAblities);
            allGloves = GetMaxWeightedArmorPieces(allGloves, weights, desiredAblities);
            allWaists = GetMaxWeightedArmorPieces(allWaists, weights, desiredAblities);
            allLegs = GetMaxWeightedArmorPieces(allLegs, weights, desiredAblities);

            var heads = new List<IArmorPiece>();
            var chests = new List<IArmorPiece>();
            var gloves = new List<IArmorPiece>();
            var waists = new List<IArmorPiece>();
            var legs = new List<IArmorPiece>();
            var charms = new List<ICharmLevel>();
            var jewels = new List<IJewel>();

            var test = new List<ArmorSetViewModel>();

            foreach (AbilityViewModel selectedAbility in desiredAblities)
            {
                if (skillsToCharmsMap.TryGetValue(selectedAbility.SkillId, out ICharm[] matchingCharms))
                    charms.AddRange(matchingCharms.SelectMany(x => x.Levels));

                if (skillsToJewelsMap.TryGetValue(selectedAbility.SkillId, out IJewel[] matchingJewels))
                    jewels.AddRange(matchingJewels);
            }

            jewels.Sort((a, b) => b.SlotSize.CompareTo(a.SlotSize));

            //var armorPieceWeights = new Dictionary<int, int>();

            //foreach (AbilityViewModel selectedAbility in SelectedAbilities.Where(x => x.IsChecked))
            //{
            //    IArmorPiece[] matchingArmorPieces = skillsToArmorsMap[selectedAbility.SkillId];

            //    foreach (IArmorPiece armorPiece in matchingArmorPieces)
            //    {
            //        if (armorPieceWeights.TryGetValue(armorPiece.Id, out int weight) == false)
            //            armorPieceWeights.Add(armorPiece.Id, 1);
            //        else
            //            armorPieceWeights[armorPiece.Id] = weight + 1;
            //    }

            //    //IArmorPiece temp = CreateArmorPieceSorter(matchingArmorPieces.Where(x => x.Type == ArmorPieceType.Head), sortCriterias).FirstOrDefault();
            //    //if (temp != null)
            //    //    heads.Add(temp);

            //    //temp = CreateArmorPieceSorter(matchingArmorPieces.Where(x => x.Type == ArmorPieceType.Chest), sortCriterias).FirstOrDefault();
            //    //if (temp != null)
            //    //    chests.Add(temp);

            //    //temp = CreateArmorPieceSorter(matchingArmorPieces.Where(x => x.Type == ArmorPieceType.Gloves), sortCriterias).FirstOrDefault();
            //    //if (temp != null)
            //    //    gloves.Add(temp);

            //    //temp = CreateArmorPieceSorter(matchingArmorPieces.Where(x => x.Type == ArmorPieceType.Waist), sortCriterias).FirstOrDefault();
            //    //if (temp != null)
            //    //    waists.Add(temp);

            //    //temp = CreateArmorPieceSorter(matchingArmorPieces.Where(x => x.Type == ArmorPieceType.Legs), sortCriterias).FirstOrDefault();
            //    //if (temp != null)
            //    //    legs.Add(temp);

            //    //if (skillsToCharmsMap.TryGetValue(selectedAbility.SkillId, out ICharm[] matchingCharms))
            //    //    charms.AddRange(matchingCharms.SelectMany(x => x.Levels));

            //    //if (skillsToJewelsMap.TryGetValue(selectedAbility.SkillId, out IJewel[] matchingJewels))
            //    //    jewels.AddRange(matchingJewels);
            //}

            int[] weaponSlots = new int[] { 3 };
            IEquipment[] equipments = new IEquipment[6];

            foreach (IEquipment h in allHeads)
            {
                equipments[0] = h;
                foreach (IEquipment c in allChests)
                {
                    equipments[1] = c;
                    foreach (IEquipment g in allGloves)
                    {
                        equipments[2] = g;
                        foreach (IEquipment w in allWaists)
                        {
                            equipments[3] = w;
                            foreach (IEquipment l in allLegs)
                            {
                                equipments[4] = l;
                                foreach (ICharmLevel ch_ in charms)
                                {
                                    equipments[5] = ch_;

                                    if (IsArmorSetMatching(weaponSlots, equipments, jewels, desiredAblities) != ArmorSetSearchResult.Mismatch)
                                    {
                                    }
                                }
                            }
                        }
                    }
                }
            }



            //var indicesTruthTable = new IndicesTruthTable(6);
            //int[] output = new int[indicesTruthTable.IndicesCount];
            //int iterationsPerIndex = (int)Math.Pow(2.0, indicesTruthTable.IndicesCount);
            //int iterations = iterationsPerIndex * 100;


            //for (int i = 0; i < iterations; i++)
            //{
            //    indicesTruthTable.Next(output);

            //    //var foundArmorPieces = new List<IArmorPiece>();

            //    //if (heads.Count > 0)
            //    //    foundArmorPieces.Add(heads[output[0] % heads.Count]);
            //    //if (chests.Count > 0)
            //    //    foundArmorPieces.Add(chests[output[1] % chests.Count]);
            //    //if (gloves.Count > 0)
            //    //    foundArmorPieces.Add(gloves[output[2] % gloves.Count]);
            //    //if (waists.Count > 0)
            //    //    foundArmorPieces.Add(waists[output[3] % waists.Count]);
            //    //if (legs.Count > 0)
            //    //    foundArmorPieces.Add(legs[output[4] % legs.Count]);

            //    if (heads.Count > 0)
            //        equipments[0] = heads[output[0] % heads.Count];
            //    else
            //        equipments[0] = null;

            //    if (chests.Count > 0)
            //        equipments[1] = chests[output[1] % chests.Count];
            //    else
            //        equipments[1] = null;

            //    if (gloves.Count > 0)
            //        equipments[2] = gloves[output[2] % gloves.Count];
            //    else
            //        equipments[2] = null;

            //    if (waists.Count > 0)
            //        equipments[3] = waists[output[3] % waists.Count];
            //    else
            //        equipments[3] = null;

            //    if (legs.Count > 0)
            //        equipments[4] = legs[output[4] % legs.Count];
            //    else
            //        equipments[4] = null;

            //    if (charms.Count > 0)
            //        equipments[5] = charms[output[5] % charms.Count];
            //    else
            //        equipments[5] = null;

            //    if (IsArmorSetMatching(weaponSlots, equipments, selectedAbilities, skillsToJewelsMap))
            //    {
            //        test.Add(new ArmorSetViewModel
            //        {
            //            ArmorPieces = equipments.OfType<IArmorPiece>().ToArray()
            //        });
            //    }
            //}

            //FoundArmorSets = test;

            sw.Stop();

            var sb = new StringBuilder();

            long hh = allHeads.LongLength;
            long cc = allChests.LongLength;
            long gg = allGloves.LongLength;
            long ww = allWaists.LongLength;
            long ll = allLegs.LongLength;
            long ch = charms.Count;


            sb.AppendLine($"Heads count:  {hh}");
            sb.AppendLine($"Chests count: {cc}");
            sb.AppendLine($"Gloves count: {gg}");
            sb.AppendLine($"Waists count: {ww}");
            sb.AppendLine($"Legs count:   {ll}");
            sb.AppendLine($"Charms count:   {ch}");
            sb.AppendLine($"Combination count: {hh * cc * gg * ww * ll * ch}");
            sb.AppendLine($"Took: {sw.ElapsedMilliseconds} ms");

            SearchResult = sb.ToString();
        }

        private IArmorPiece[] GetMaxWeightedArmorPieces(IArmorPiece[] armorPieces, int[] weights, IEnumerable<AbilityViewModel> desiredAbilities)
        {
            int max = 0;

            for (int i = 0; i < armorPieces.Length; i++)
            {
                weights[i] = 0;

                foreach (AbilityViewModel ability in desiredAbilities)
                {
                    weights[i] += armorPieces[i].Abilities.Where(a => a.Skill.Id == ability.SkillId).Sum(a => a.Level);
                    if (weights[i] > max)
                        max = weights[i];
                }
            }

            if (max == 0)
                return new IArmorPiece[] { null };

            int maxMin = Math.Max(1, max - 1);

            return armorPieces
                .Where((x, i) => max >= weights[i] && weights[i] >= maxMin)
                .ToArray();
        }

        private enum ArmorSetSearchResult
        {
            Mismatch,
            SuccessOptimal,
            SuccessNonOptimal
        }

        private ArmorSetSearchResult IsArmorSetMatching(
            int[] weaponSlots, IEquipment[] equipments,
            IList<IJewel> matchingJewels,
            IEnumerable<AbilityViewModel> desiredAbilities)
        {
            bool isOptimal = true;

            int availableSlot1 = 0;
            int availableSlot2 = 0;
            int availableSlot3 = 0;

            if (weaponSlots != null)
            {
                availableSlot1 = weaponSlots.Count(x => x == 1);
                availableSlot2 = weaponSlots.Count(x => x == 2);
                availableSlot3 = weaponSlots.Count(x => x == 3);
            }

            foreach (IEquipment equipment in equipments)
            {
                if (equipment == null)
                    continue;

                availableSlot1 += equipment.Slots.Count(x => x == 1);
                availableSlot2 += equipment.Slots.Count(x => x == 2);
                availableSlot3 += equipment.Slots.Count(x => x == 3);
            }

            foreach (AbilityViewModel ability in desiredAbilities)
            {
                int armorAbilityTotal = 0;

                foreach (IEquipment equipment in equipments)
                {
                    if (equipment != null)
                        armorAbilityTotal += equipment.Abilities.Where(x => x.Id == ability.Id).Sum(a => a.Level);
                }

                int remaingAbilityLevels = ability.Level - armorAbilityTotal;

                if (remaingAbilityLevels > 0)
                {
                    if (availableSlot1 <= 0 && availableSlot2 <= 0 && availableSlot3 <= 0)
                        return ArmorSetSearchResult.Mismatch;

                    foreach (IJewel a in matchingJewels.Where(j => j.Abilities.Any(a => a.Id == ability.Id)))
                    {
                    }
                }
                else
                {
                    if (remaingAbilityLevels < 0)
                        isOptimal = false;

                    break;
                }
            }

            return ArmorSetSearchResult.Mismatch;
        }

        private class OrderedEnumerable<T> : IOrderedEnumerable<T>
        {
            private readonly IEnumerable<T> source;

            public OrderedEnumerable(IEnumerable<T> source)
            {
                this.source = source;
            }

            public IOrderedEnumerable<T> CreateOrderedEnumerable<TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer, bool descending)
            {
                return this;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return source.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return source.GetEnumerator();
            }
        }

        // values bellow are max slot size (3) to the power of the slot index (0-based)
        private static readonly int[] slotSizeSortWeight = new[] { 9, 3, 1 };

        private IEnumerable<IArmorPiece> CreateArmorPieceSorter(IEnumerable<IArmorPiece> items, IEnumerable<MaximizedSearchCriteria> sortCriterias)
        {
            if (items.Any() == false)
                return items;

            //IOrderedEnumerable<IArmorPiece> result = items.OrderBy(x => 1); // wasting a bit of CPU cycles for productivity purpose :(
            IOrderedEnumerable<IArmorPiece> result = new OrderedEnumerable<IArmorPiece>(items);

            foreach (MaximizedSearchCriteria sortCriteria in sortCriterias)
            {
                switch (sortCriteria)
                {
                    case MaximizedSearchCriteria.BaseDefense:
                        result = result.ThenByDescending(x => x.Defense.Base);
                        break;
                    case MaximizedSearchCriteria.MaxUnaugmentedDefense:
                        result = result.ThenByDescending(x => x.Defense.Max);
                        break;
                    case MaximizedSearchCriteria.MaxAugmentedDefense:
                        result = result.ThenByDescending(x => x.Defense.Augmented);
                        break;
                    case MaximizedSearchCriteria.Rarity:
                        result = result.ThenBy(x => x.Rarity);
                        break;
                    case MaximizedSearchCriteria.SlotCount:
                        result = result.ThenByDescending(x => x.Slots.Length);
                        break;
                    case MaximizedSearchCriteria.SlotSize:
                        result = result.ThenByDescending(x => x.Slots.Select((v, i) => v * slotSizeSortWeight[i]).Sum());
                        break;
                    case MaximizedSearchCriteria.FireResistance:
                        result = result.ThenByDescending(x => x.Resistances.Fire);
                        break;
                    case MaximizedSearchCriteria.WaterResistance:
                        result = result.ThenByDescending(x => x.Resistances.Water);
                        break;
                    case MaximizedSearchCriteria.ThunderResistance:
                        result = result.ThenByDescending(x => x.Resistances.Thunder);
                        break;
                    case MaximizedSearchCriteria.IceResistance:
                        result = result.ThenByDescending(x => x.Resistances.Ice);
                        break;
                    case MaximizedSearchCriteria.DragonResistance:
                        result = result.ThenByDescending(x => x.Resistances.Dragon);
                        break;
                }
            }

            return result;
        }

        private string searchResult;
        public string SearchResult
        {
            get { return searchResult; }
            private set { SetValue(ref searchResult, value); }
        }
    }
}
