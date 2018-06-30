using System;
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

            var heads = new List<IArmorPiece>();
            var chests = new List<IArmorPiece>();
            var gloves = new List<IArmorPiece>();
            var waists = new List<IArmorPiece>();
            var legs = new List<IArmorPiece>();

            var test = new List<ArmorSetViewModel>();

            foreach (AbilityViewModel selectedAbility in SelectedAbilities.Where(x => x.IsChecked))
            {
                IArmorPiece[] matchingArmorPieces = skillsToArmorsMap[selectedAbility.SkillId];

                heads.AddRange(CreateArmorPieceSorter(matchingArmorPieces.Where(x => x.Type == ArmorPieceType.Head), sortCriterias));
                chests.AddRange(CreateArmorPieceSorter(matchingArmorPieces.Where(x => x.Type == ArmorPieceType.Chest), sortCriterias));
                gloves.AddRange(CreateArmorPieceSorter(matchingArmorPieces.Where(x => x.Type == ArmorPieceType.Gloves), sortCriterias));
                waists.AddRange(CreateArmorPieceSorter(matchingArmorPieces.Where(x => x.Type == ArmorPieceType.Waist), sortCriterias));
                legs.AddRange(CreateArmorPieceSorter(matchingArmorPieces.Where(x => x.Type == ArmorPieceType.Legs), sortCriterias));
            }

            int[] output = new int[5];
            var indicesTruthTable = new IndicesTruthTable();

            indicesTruthTable.Next(output);

            var foundArmorPieces = new List<IArmorPiece>();

            if (heads.Count > 0)
                foundArmorPieces.Add(heads[output[0] % heads.Count]);
            if (chests.Count > 0)
                foundArmorPieces.Add(chests[output[1] % chests.Count]);
            if (gloves.Count > 0)
                foundArmorPieces.Add(gloves[output[2] % gloves.Count]);
            if (waists.Count > 0)
                foundArmorPieces.Add(waists[output[3] % waists.Count]);
            if (legs.Count > 0)
                foundArmorPieces.Add(legs[output[4] % legs.Count]);

            test.Add(new ArmorSetViewModel
            {
                ArmorPieces = foundArmorPieces.ToArray()
            });

            //foreach (var h in heads)
            //{
            //    foreach (var c in chests)
            //    {
            //        foreach (var g in gloves)
            //        {
            //            foreach (var w in waists)
            //            {
            //                foreach (var l in legs)
            //                {
            //                    test.Add(new ArmorSetViewModel
            //                    {
            //                        ArmorPieces = new IArmorPiece[] { h, c, g, w, l }
            //                    });
            //                }
            //            }
            //        }
            //    }
            //}

            FoundArmorSets = test;

            sw.Stop();

            var sb = new StringBuilder();

            long hh = heads.Count;
            long cc = chests.Count;
            long gg = gloves.Count;
            long ww = waists.Count;
            long ll = legs.Count;

            sb.AppendLine($"Heads count:  {heads.Count }");
            sb.AppendLine($"Chests count: {chests.Count }");
            sb.AppendLine($"Gloves count: {gloves.Count }");
            sb.AppendLine($"Waists count: {waists.Count }");
            sb.AppendLine($"Legs count:   {legs.Count }");
            sb.AppendLine($"Combination count: {hh * cc * gg * ww * ll}");
            sb.AppendLine($"Took: {sw.ElapsedMilliseconds} ms");

            SearchResult = sb.ToString();
        }

        // values bellow are max slot size (3) to the power of the slot index (0-based)
        private static readonly int[] slotSizeSortWeight = new[] { 9, 3, 1 };

        private IEnumerable<IArmorPiece> CreateArmorPieceSorter(IEnumerable<IArmorPiece> items, IEnumerable<MaximizedSearchCriteria> sortCriterias)
        {
            IOrderedEnumerable<IArmorPiece> result = items.OrderBy(x => 1); // wasting a bit of CPU cycles for productivity purpose :(

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
