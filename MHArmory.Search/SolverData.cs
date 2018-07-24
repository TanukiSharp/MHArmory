using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MHArmory.Core.DataStructures;

namespace MHArmory.Search
{
    public class SolverDataEquipmentModel
    {
        public IEquipment Equipment { get; }
        public bool IsSelected { get; set; } = true;

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

        public SolverDataJewelModel(IJewel jewel)
            : this(jewel, 999)
        {
        }

        public SolverDataJewelModel(IJewel jewel, int available)
        {
            Jewel = jewel;
            Available = available;
        }
    }

    public class SolverData
    {
        public int[] WeaponSlots { get; private set; }
        public IList<SolverDataEquipmentModel> AllHeads { get; private set; }
        public IList<SolverDataEquipmentModel> AllChests { get; private set; }
        public IList<SolverDataEquipmentModel> AllGloves { get; private set; }
        public IList<SolverDataEquipmentModel> AllWaists { get; private set; }
        public IList<SolverDataEquipmentModel> AllLegs { get; private set; }
        public IList<SolverDataEquipmentModel> AllCharms { get; private set; }
        public IList<SolverDataJewelModel> AllJewels { get; private set; }
        public IList<IAbility> DesiredAbilities { get; }

        private readonly IList<MaximizedSearchCriteria> inputSearchCriterias;
        private readonly IList<IArmorPiece> inputHeads;
        private readonly IList<IArmorPiece> inputChests;
        private readonly IList<IArmorPiece> inputGloves;
        private readonly IList<IArmorPiece> inputWaists;
        private readonly IList<IArmorPiece> inputLegs;
        private readonly IList<ICharmLevel> inputCharms;
        private readonly IList<SolverDataJewelModel> inputJewels;

        private IList<SolverDataEquipmentModel> allHeads;
        private IList<SolverDataEquipmentModel> allChests;
        private IList<SolverDataEquipmentModel> allGloves;
        private IList<SolverDataEquipmentModel> allWaists;
        private IList<SolverDataEquipmentModel> allLegs;
        private IList<SolverDataEquipmentModel> allCharms;
        private IList<SolverDataJewelModel> allJewels;

        public int MinJewelSize { get; private set; }
        public int MaxJewelSize { get; private set; }

        public SolverData(
            IList<int> weaponSlots,
            IList<MaximizedSearchCriteria> searchCriterias,
            IList<IArmorPiece> heads,
            IList<IArmorPiece> chests,
            IList<IArmorPiece> gloves,
            IList<IArmorPiece> waists,
            IList<IArmorPiece> legs,
            IList<ICharmLevel> charms,
            IList<SolverDataJewelModel> jewels,
            IList<IAbility> desiredAbilities
        )
        {
            inputHeads = heads;
            inputChests = chests;
            inputGloves = gloves;
            inputWaists = waists;
            inputLegs = legs;
            inputCharms = charms;
            inputJewels = jewels;

            if (searchCriterias == null || searchCriterias.Count == 0)
                searchCriterias = new[] { MaximizedSearchCriteria.SlotSizeCube };

            inputSearchCriterias = searchCriterias;

            WeaponSlots = weaponSlots.ToArray();
            DesiredAbilities = desiredAbilities;

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

            allHeads = inputHeads
                .RemoveWhere(isLessPoweredEquivalentArmorPiece)
                .Sort(inputSearchCriterias)
                .ExcludeNonMatchingAbilities(DesiredAbilities)
                .MapToSolverDataModel()
                .ToList()
                .UnselectOddlySkilled(DesiredAbilities)
                ;

            allChests = inputChests
                .RemoveWhere(isLessPoweredEquivalentArmorPiece)
                .Sort(inputSearchCriterias)
                .ExcludeNonMatchingAbilities(DesiredAbilities)
                .MapToSolverDataModel()
                .ToList()
                .UnselectOddlySkilled(DesiredAbilities)
                ;

            allGloves = inputGloves
                .RemoveWhere(isLessPoweredEquivalentArmorPiece)
                .Sort(inputSearchCriterias)
                .ExcludeNonMatchingAbilities(DesiredAbilities)
                .MapToSolverDataModel()
                .ToList()
                .UnselectOddlySkilled(DesiredAbilities)
                ;

            allWaists = inputWaists
                .RemoveWhere(isLessPoweredEquivalentArmorPiece)
                .Sort(inputSearchCriterias)
                .ExcludeNonMatchingAbilities(DesiredAbilities)
                .MapToSolverDataModel()
                .ToList()
                .UnselectOddlySkilled(DesiredAbilities)
                ;

            allLegs = inputLegs
                .RemoveWhere(isLessPoweredEquivalentArmorPiece)
                .Sort(inputSearchCriterias)
                .ExcludeNonMatchingAbilities(DesiredAbilities)
                .MapToSolverDataModel()
                .ToList()
                .UnselectOddlySkilled(DesiredAbilities)
                ;

            allCharms = inputCharms
                .RemoveWhere(DataUtility.IsLessPoweredEquivalentEquipment)
                .ExcludeNonMatchingAbilities(DesiredAbilities)
                .MapToSolverDataModel()
                .ToList();
        }

        public SolverData Done()
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
            AllHeads = allHeads;
            AllChests = allChests;
            AllGloves = allGloves;
            AllWaists = allWaists;
            AllLegs = allLegs;
            AllCharms = allCharms;
            AllJewels = allJewels;
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
        public static IList<SolverDataEquipmentModel> UnselectOddlySkilled(this IList<SolverDataEquipmentModel> equipments, IEnumerable<IAbility> desiredAbilities)
        {
            foreach (SolverDataEquipmentModel equipment in equipments)
            {
                foreach (IAbility ability in equipment.Equipment.Abilities)
                {
                    bool isAbilityMatching = ability.IsMatchingDesiredAbilities(desiredAbilities);

                    if (isAbilityMatching)
                    {
                        if (IsWorthBySlots(equipment.Equipment.Slots))
                            continue;
                    }
                    else
                    {
                        equipment.IsSelected = false;
                        break;
                    }
                }
            }

            return equipments;
        }

        public static IEnumerable<IArmorPiece> Sort(this IEnumerable<IArmorPiece> unsorted, IEnumerable<MaximizedSearchCriteria> criterias)
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

        public static IEnumerable<IEquipment> ExcludeNonMatchingAbilities(this IEnumerable<IEquipment> equipments, IEnumerable<IAbility> desiredAbilities)
        {
            IEnumerable<IEquipment> filtered = equipments.Where(x =>
            {
                bool isAbilityWorth = x.Abilities.Any(y => y.IsMatchingDesiredAbilities(desiredAbilities));
                bool isSlotsWorth = IsWorthBySlots(x.Slots);

                return isAbilityWorth || isSlotsWorth;
            });

            if (filtered.Any())
                return filtered;

            return new[] { equipments.FirstOrDefault() };
        }

        public static IEnumerable<T> ExcludeNonMatchingAbilities<T>(this IEnumerable<T> equipments, IEnumerable<IAbility> desiredAbilities) where T : IHasAbilities
        {
            IEnumerable<T> filtered = equipments.Where(x => x.Abilities.Any(y => y.IsMatchingDesiredAbilities(desiredAbilities)));

            if (filtered.Any())
                return filtered;

            return new[] { equipments.FirstOrDefault() };
        }

        public static IEnumerable<SolverDataEquipmentModel> MapToSolverDataModel(this IEnumerable<IEquipment> equipments)
        {
            return equipments.Select(x => new SolverDataEquipmentModel(x));
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

        public static IList<T> RemoveWhere<T>(this IList<T> equipments, Func<T, T, bool> matchFunc)
        {
            for (int i = 0; i < equipments.Count; i++)
            {
                for (int j = 0; j < equipments.Count; j++)
                {
                    if (i == j)
                        continue;

                    if (matchFunc(equipments[i], equipments[j]))
                    {
                        equipments.RemoveAt(j);

                        if (i >= j)
                            i--;

                        j--;
                    }
                }
            }

            return equipments;
        }
    }
}
