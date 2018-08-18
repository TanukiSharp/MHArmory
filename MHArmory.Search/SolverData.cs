using System;
using System.Collections.Generic;
using System.Linq;
using MHArmory.Core.DataStructures;

namespace MHArmory.Search
{
    public class SolverDataEquipmentModel : ISolverDataEquipmentModel
    {
        public IEquipment Equipment { get; }
        public bool ToBeRemoved { get; set; }

        public event EventHandler SelectionChanged;

        private bool isSelected = true;
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public SolverDataEquipmentModel(IEquipment equipment)
        {
            Equipment = equipment;
        }

        public override string ToString()
        {
            return $"[{(IsSelected ? "O" : "X")}] {Equipment.Name}";
        }
    }

    public class SolverDataJewelModel : IHasAbilities
    {
        public IJewel Jewel { get; }
        public int Available { get; set; }

        IAbility[] IHasAbilities.Abilities => Jewel.Abilities;

        public SolverDataJewelModel(IJewel jewel, int available)
        {
            Jewel = jewel;
            Available = available;
        }
    }

    public class SolverData : ISolverData
    {
        public int[] WeaponSlots { get; private set; }
        public ISolverDataEquipmentModel[] AllHeads { get; private set; }
        public ISolverDataEquipmentModel[] AllChests { get; private set; }
        public ISolverDataEquipmentModel[] AllGloves { get; private set; }
        public ISolverDataEquipmentModel[] AllWaists { get; private set; }
        public ISolverDataEquipmentModel[] AllLegs { get; private set; }
        public ISolverDataEquipmentModel[] AllCharms { get; private set; }
        public SolverDataJewelModel[] AllJewels { get; private set; }
        public IAbility[] DesiredAbilities { get; }

        private readonly IList<MaximizedSearchCriteria> inputSearchCriterias;
        private readonly List<SolverDataEquipmentModel> inputHeads;
        private readonly List<SolverDataEquipmentModel> inputChests;
        private readonly List<SolverDataEquipmentModel> inputGloves;
        private readonly List<SolverDataEquipmentModel> inputWaists;
        private readonly List<SolverDataEquipmentModel> inputLegs;
        private readonly List<SolverDataEquipmentModel> inputCharms;
        private readonly IList<SolverDataJewelModel> inputJewels;

        private List<SolverDataEquipmentModel> allHeads;
        private List<SolverDataEquipmentModel> allChests;
        private List<SolverDataEquipmentModel> allGloves;
        private List<SolverDataEquipmentModel> allWaists;
        private List<SolverDataEquipmentModel> allLegs;
        private List<SolverDataEquipmentModel> allCharms;
        private IList<SolverDataJewelModel> allJewels;

        public int MinJewelSize { get; private set; }
        public int MaxJewelSize { get; private set; }

        public SolverData(
            IList<int> weaponSlots,
            IList<MaximizedSearchCriteria> searchCriterias,
            IEnumerable<IArmorPiece> heads,
            IEnumerable<IArmorPiece> chests,
            IEnumerable<IArmorPiece> gloves,
            IEnumerable<IArmorPiece> waists,
            IEnumerable<IArmorPiece> legs,
            IEnumerable<ICharmLevel> charms,
            IEnumerable<SolverDataJewelModel> jewels,
            IList<IAbility> desiredAbilities
        )
        {
            inputHeads = heads.Select(x => new SolverDataEquipmentModel(x)).ToList();
            inputChests = chests.Select(x => new SolverDataEquipmentModel(x)).ToList();
            inputGloves = gloves.Select(x => new SolverDataEquipmentModel(x)).ToList();
            inputWaists = waists.Select(x => new SolverDataEquipmentModel(x)).ToList();
            inputLegs = legs.Select(x => new SolverDataEquipmentModel(x)).ToList();
            inputCharms = charms.Select(x => new SolverDataEquipmentModel(x)).ToList();
            inputJewels = jewels.ToList();

            if (searchCriterias == null || searchCriterias.Count == 0)
                searchCriterias = new[] { MaximizedSearchCriteria.SlotSizeCube };

            inputSearchCriterias = searchCriterias;

            WeaponSlots = weaponSlots.ToArray();
            DesiredAbilities = desiredAbilities.ToArray();

            ProcessInputData();
        }

        private void ProcessInputData()
        {
            var isLessPoweredEquivalentArmorPiece = new Func<IArmorPiece, IArmorPiece, bool>(DataUtility.IsLessPoweredEquivalentArmorPiece);

            allJewels = inputJewels
                .ExcludeNonMatchingAbilities(DesiredAbilities)
                .ToList();

            MinJewelSize = allJewels.Min(x => x.Jewel.SlotSize);
            MaxJewelSize = allJewels.Max(x => x.Jewel.SlotSize);

            List<SolverDataEquipmentModel> tempAllHeads = inputHeads
                .RemoveWhere(isLessPoweredEquivalentArmorPiece)
                .Sort(inputSearchCriterias)
                .ExcludeEquipmentsNonMatchingAbilities(DesiredAbilities)
                .ToList()
                ;

            List<SolverDataEquipmentModel> tempAllChests = inputChests
                .RemoveWhere(isLessPoweredEquivalentArmorPiece)
                .Sort(inputSearchCriterias)
                .ExcludeEquipmentsNonMatchingAbilities(DesiredAbilities)
                .ToList()
                ;

            List<SolverDataEquipmentModel> tempAllGloves = inputGloves
                .RemoveWhere(isLessPoweredEquivalentArmorPiece)
                .Sort(inputSearchCriterias)
                .ExcludeEquipmentsNonMatchingAbilities(DesiredAbilities)
                .ToList()
                ;

            List<SolverDataEquipmentModel> tempAllWaists = inputWaists
                .RemoveWhere(isLessPoweredEquivalentArmorPiece)
                .Sort(inputSearchCriterias)
                .ExcludeEquipmentsNonMatchingAbilities(DesiredAbilities)
                .ToList()
                ;

            List<SolverDataEquipmentModel> tempAllLegs = inputLegs
                .RemoveWhere(isLessPoweredEquivalentArmorPiece)
                .Sort(inputSearchCriterias)
                .ExcludeEquipmentsNonMatchingAbilities(DesiredAbilities)
                .ToList()
                ;

            ExcludeNonCompleteFullArmorSets(tempAllHeads, tempAllChests, tempAllGloves, tempAllWaists, tempAllLegs);

            allHeads = tempAllHeads
                .UnselectOddlySkilled(DesiredAbilities)
                ;

            allChests = tempAllChests
                .UnselectOddlySkilled(DesiredAbilities)
                ;

            allGloves = tempAllGloves
                .UnselectOddlySkilled(DesiredAbilities)
                ;

            allWaists = tempAllWaists
                .UnselectOddlySkilled(DesiredAbilities)
                ;

            allLegs = tempAllLegs
                .UnselectOddlySkilled(DesiredAbilities)
                ;

            CheckFullArmorSetSelection();

            allCharms = inputCharms
                .RemoveWhere<IEquipment>(DataUtility.IsLessPoweredEquivalentEquipment)
                .ExcludeEquipmentsNonMatchingAbilities(DesiredAbilities)
                .ToList();

            allHeads.RemoveAll(x => x.ToBeRemoved);
            allCharms.RemoveAll(x => x.ToBeRemoved);
            allGloves.RemoveAll(x => x.ToBeRemoved);
            allWaists.RemoveAll(x => x.ToBeRemoved);
            allLegs.RemoveAll(x => x.ToBeRemoved);
            allCharms.RemoveAll(x => x.ToBeRemoved);
        }

        public ISolverData Done()
        {
            UpdateReferences();

            if (AllHeads.Where(x => x != null).Any(x => x.Equipment.Name == "Strategist Spectacles α"))
            {
            }

            if (AllChests.Where(x => x != null).Any(x => x.Equipment.Name == "Kulve Taroth's Ire α"))
            {
            }

            if (AllGloves.Where(x => x != null).Any(x => x.Equipment.Name == "Vaal Hazak Braces β"))
            {
            }

            if (AllWaists.Where(x => x != null).Any(x => x.Equipment.Name == "Odogaron Coil β"))
            {
            }

            if (AllLegs.Where(x => x != null).Any(x => x.Equipment.Name == "Dante's Leather Boots α"))
            {
            }

            if (AllCharms.Where(x => x != null).Any(x => x.Equipment.Name == "Master's Charm III"))
            {
            }

            return this;
        }

        private void UpdateReferences()
        {
            AllHeads = allHeads.ToArray<ISolverDataEquipmentModel>();
            AllChests = allChests.ToArray<ISolverDataEquipmentModel>();
            AllGloves = allGloves.ToArray<ISolverDataEquipmentModel>();
            AllWaists = allWaists.ToArray<ISolverDataEquipmentModel>();
            AllLegs = allLegs.ToArray<ISolverDataEquipmentModel>();
            AllCharms = allCharms.ToArray<ISolverDataEquipmentModel>();
            AllJewels = allJewels.ToArray();
        }

        /// <summary>
        /// This method unselect other armor pieces if at least one in the full set is unselected.
        /// </summary>
        private void CheckFullArmorSetSelection()
        {
            IEnumerable<SolverDataEquipmentModel> all = allHeads
                .Concat(allChests)
                .Concat(allGloves)
                .Concat(allWaists)
                .Concat(allLegs);

            foreach (SolverDataEquipmentModel head in allHeads)
            {
                if (head.IsSelected)
                    continue;

                if (head.Equipment is IArmorPiece armorPiece && armorPiece.ArmorSet != null && armorPiece.ArmorSet.IsFull)
                {
                    foreach (IArmorPiece setArmorPiece in armorPiece.ArmorSet.ArmorPieces)
                    {
                        SolverDataEquipmentModel result = all.FirstOrDefault(x => x.Equipment.Id == setArmorPiece.Id);
                        if (result != null)
                            result.IsSelected = false;
                    }
                }
            }
        }

        private void CheckNonFullArmorSetSelection()
        {
        }

        private void ExcludeNonCompleteFullArmorSets(
            List<SolverDataEquipmentModel> heads,
            List<SolverDataEquipmentModel> chests,
            List<SolverDataEquipmentModel> gloves,
            List<SolverDataEquipmentModel> waists,
            List<SolverDataEquipmentModel> legs
        )
        {
            ExcludeNonCompleteFullArmorSets(heads, heads, chests, gloves, waists, legs);
            ExcludeNonCompleteFullArmorSets(chests, heads, chests, gloves, waists, legs);
            ExcludeNonCompleteFullArmorSets(gloves, heads, chests, gloves, waists, legs);
            ExcludeNonCompleteFullArmorSets(waists, heads, chests, gloves, waists, legs);
            ExcludeNonCompleteFullArmorSets(legs, heads, chests, gloves, waists, legs);
        }

        private void ExcludeNonCompleteFullArmorSets(
            List<SolverDataEquipmentModel> source,
            List<SolverDataEquipmentModel> heads,
            List<SolverDataEquipmentModel> chests,
            List<SolverDataEquipmentModel> gloves,
            List<SolverDataEquipmentModel> waists,
            List<SolverDataEquipmentModel> legs
        )
        {
            for (int i = 0; i < heads.Count; i++)
            {
                IArmorSet armorSet = ((IArmorPiece)heads[i].Equipment).ArmorSet;

                if (armorSet == null || armorSet.IsFull == false)
                    continue;

                IArmorPiece[] setPieces = armorSet.ArmorPieces;

                IArmorPiece head = setPieces.First(x => x.Type == EquipmentType.Head);
                IArmorPiece chest = setPieces.First(x => x.Type == EquipmentType.Chest);
                IArmorPiece glove = setPieces.First(x => x.Type == EquipmentType.Gloves);
                IArmorPiece waist = setPieces.First(x => x.Type == EquipmentType.Waist);
                IArmorPiece leg = setPieces.First(x => x.Type == EquipmentType.Legs);

                if (heads.Any(x => x.Equipment.Id == head.Id) == false ||
                    chests.Any(x => x.Equipment.Id == chest.Id) == false ||
                    gloves.Any(x => x.Equipment.Id == glove.Id) == false ||
                    waists.Any(x => x.Equipment.Id == waist.Id) == false ||
                    legs.Any(x => x.Equipment.Id == leg.Id) == false)
                {
                    heads.RemoveAll(x => x.Equipment.Id == head.Id);
                    chests.RemoveAll(x => x.Equipment.Id == chest.Id);
                    gloves.RemoveAll(x => x.Equipment.Id == glove.Id);
                    waists.RemoveAll(x => x.Equipment.Id == waist.Id);
                    legs.RemoveAll(x => x.Equipment.Id == leg.Id);
                    i--;
                }
            }
        }

        //private IList<T> GetMaxWeightedArmorPieces<T>(IList<T> armorPieces, int[] weights, IEnumerable<IAbility> desiredAbilities) where T : IEquipment
        //{
        //    int max = 0;

        //    for (int i = 0; i < armorPieces.Count; i++)
        //    {
        //        weights[i] = 0;

        //        foreach (IAbility ability in desiredAbilities)
        //        {
        //            weights[i] += armorPieces[i].Abilities.Where(a => a.Skill.Id == ability.Skill.Id).Sum(a => a.Level);
        //            if (weights[i] > max)
        //                max = weights[i];
        //        }
        //    }

        //    if (max == 0)
        //        return new T[] { default(T) };

        //    //int maxMin = Math.Max(1, max - 1);

        //    //return armorPieces
        //    //    .Where((x, i) => max >= weights[i] && weights[i] >= maxMin)
        //    //    .ToList();

        //    return armorPieces
        //        .Where((x, i) => weights[i] > 0)
        //        .ToList();
        //}
    }

    public static class SolverDataOperators
    {
        public static List<SolverDataEquipmentModel> UnselectOddlySkilled(this List<SolverDataEquipmentModel> equipments, IEnumerable<IAbility> desiredAbilities)
        {
            foreach (SolverDataEquipmentModel equipment in equipments)
            {
                if (IsWorthBySlots(equipment.Equipment.Slots))
                    continue;

                foreach (IAbility ability in equipment.Equipment.Abilities)
                {
                    if (ability.IsMatchingDesiredAbilities(desiredAbilities) == false)
                    {
                        equipment.IsSelected = false;
                        break;
                    }
                }
            }

            return equipments;
        }

        public static IEnumerable<SolverDataEquipmentModel> Sort(this IEnumerable<SolverDataEquipmentModel> unsorted, IEnumerable<MaximizedSearchCriteria> criterias)
        {
            return DataUtility.CreateArmorPieceSorter(unsorted, criterias);
        }

        public static bool IsWorthBySlots(int[] slots)
        {
            //int equipmentScore = DataUtility.SlotSizeScoreCube(slots);
            //int minMaxScore = minRequiredSlotSize * minRequiredSlotSize * minRequiredSlotSize + maxRequiredSlotSize * maxRequiredSlotSize * maxRequiredSlotSize;
            //return equipmentScore >= minMaxScore;

            //bool a = slots.Count(s => s >= minRequiredSlotSize) >= 1;
            //bool b = slots.Count(s => s >= maxRequiredSlotSize) >= 1;
            //bool c = slots.Count(s => s >= 1) >= 2;
            //return a && b && c;

            bool a = slots.Count(s => s >= 1) >= Heuristics.WorthBySlotsMinCount;
            bool b = slots.Sum() >= Heuristics.WorthBySlotsMinSum;
            return a && b;
        }

        public static IEnumerable<SolverDataEquipmentModel> ExcludeEquipmentsNonMatchingAbilities(this IEnumerable<SolverDataEquipmentModel> equipments, IEnumerable<IAbility> desiredAbilities)
        {
            bool whereFunc(SolverDataEquipmentModel x)
            {
                bool isArmorSetSkillMatching = x.Equipment.IsEquipmentMatchingAbility(desiredAbilities);
                bool isAbilityWorth = x.Equipment.Abilities.Any(y => y.IsMatchingDesiredAbilities(desiredAbilities));
                bool isSlotsWorth = IsWorthBySlots(x.Equipment.Slots);

                return isArmorSetSkillMatching || isAbilityWorth || isSlotsWorth;
            }

            IEnumerable<SolverDataEquipmentModel> filtered = equipments.Where(whereFunc);

            if (filtered.Any())
                return filtered;

            SolverDataEquipmentModel equipment = equipments.FirstOrDefault();
            equipment.ToBeRemoved = false;

            return new[] { equipment };
        }

        private static bool IsEquipmentMatchingAbility(this IEquipment equipment, IEnumerable<IAbility> desiredAbilities)
        {
            if (equipment is IArmorPiece armorPiece && armorPiece.ArmorSet != null && armorPiece.ArmorSet.IsFull == false)
            {
                foreach (IArmorSetSkill skill in armorPiece.ArmorSet.Skills)
                {
                    if (skill.GrantedSkills.Any(x => x.IsMatchingDesiredAbilities(desiredAbilities)))
                        return true;
                }
            }

            return false;
        }

        public static IEnumerable<T> ExcludeNonMatchingAbilities<T>(this IEnumerable<T> equipments, IEnumerable<IAbility> desiredAbilities) where T : IHasAbilities
        {
            IEnumerable<T> filtered = equipments.Where(x => x.Abilities.Any(y => y.IsMatchingDesiredAbilities(desiredAbilities)));

            if (filtered.Any())
                return filtered;

            return new[] { equipments.FirstOrDefault() };
        }

        public static bool IsMatchingDesiredAbilities(this IAbility ability, IEnumerable<IAbility> desiredAbilities)
        {
            foreach (IAbility desiredAbility in desiredAbilities)
            {
                if (DataUtility.AreAbilitiesOnSameSkill(ability, desiredAbility))
                    return true;
            }

            return false;
        }

        public static List<SolverDataEquipmentModel> RemoveWhere<T>(this List<SolverDataEquipmentModel> equipments, Func<T, T, bool> matchFunc) where T : IEquipment
        {
            for (int i = 0; i < equipments.Count; i++)
            {
                for (int j = 0; j < equipments.Count; j++)
                {
                    if (i == j)
                        continue;

                    if (matchFunc((T)equipments[i].Equipment, (T)equipments[j].Equipment))
                        equipments[j].ToBeRemoved = true;
                }
            }

            return equipments;
        }
    }
}
