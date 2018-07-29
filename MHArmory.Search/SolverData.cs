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

            List<IArmorPiece> tempAllHeads = inputHeads
                .RemoveWhere(isLessPoweredEquivalentArmorPiece)
                .Sort(inputSearchCriterias)
                .ExcludeEquipmentsNonMatchingAbilities(DesiredAbilities)
                .ToList()
                ;

            List<IArmorPiece> tempAllChests = inputChests
                .RemoveWhere(isLessPoweredEquivalentArmorPiece)
                .Sort(inputSearchCriterias)
                .ExcludeEquipmentsNonMatchingAbilities(DesiredAbilities)
                .ToList()
                ;

            List<IArmorPiece> tempAllGloves = inputGloves
                .RemoveWhere(isLessPoweredEquivalentArmorPiece)
                .Sort(inputSearchCriterias)
                .ExcludeEquipmentsNonMatchingAbilities(DesiredAbilities)
                .ToList()
                ;

            List<IArmorPiece> tempAllWaists = inputWaists
                .RemoveWhere(isLessPoweredEquivalentArmorPiece)
                .Sort(inputSearchCriterias)
                .ExcludeEquipmentsNonMatchingAbilities(DesiredAbilities)
                .ToList()
                ;

            List<IArmorPiece> tempAllLegs = inputLegs
                .RemoveWhere(isLessPoweredEquivalentArmorPiece)
                .Sort(inputSearchCriterias)
                .ExcludeEquipmentsNonMatchingAbilities(DesiredAbilities)
                .ToList()
                ;

            ExcludeNonCompleteFullArmorSets(tempAllHeads, tempAllChests, tempAllGloves, tempAllWaists, tempAllLegs);

            // --------------------------------------------------------

            allHeads = tempAllHeads
                .MapToSolverDataModel()
                .ToList()
                .UnselectOddlySkilled(DesiredAbilities)
                ;

            allChests = tempAllChests
                .MapToSolverDataModel()
                .ToList()
                .UnselectOddlySkilled(DesiredAbilities)
                ;

            allGloves = tempAllGloves
                .MapToSolverDataModel()
                .ToList()
                .UnselectOddlySkilled(DesiredAbilities)
                ;

            allWaists = tempAllWaists
                .MapToSolverDataModel()
                .ToList()
                .UnselectOddlySkilled(DesiredAbilities)
                ;

            allLegs = tempAllLegs
                .MapToSolverDataModel()
                .ToList()
                .UnselectOddlySkilled(DesiredAbilities)
                ;

            allCharms = inputCharms
                .RemoveWhere(DataUtility.IsLessPoweredEquivalentEquipment)
                .ExcludeEquipmentsNonMatchingAbilities(DesiredAbilities)
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

        private void ExcludeNonCompleteFullArmorSets(
            List<IArmorPiece> heads,
            List<IArmorPiece> chests,
            List<IArmorPiece> gloves,
            List<IArmorPiece> waists,
            List<IArmorPiece> legs
        )
        {
            ExcludeNonCompleteFullArmorSets(heads, heads, chests, gloves, waists, legs);
            ExcludeNonCompleteFullArmorSets(chests, heads, chests, gloves, waists, legs);
            ExcludeNonCompleteFullArmorSets(gloves, heads, chests, gloves, waists, legs);
            ExcludeNonCompleteFullArmorSets(waists, heads, chests, gloves, waists, legs);
            ExcludeNonCompleteFullArmorSets(legs, heads, chests, gloves, waists, legs);
        }

        private void ExcludeNonCompleteFullArmorSets(
            List<IArmorPiece> source,
            List<IArmorPiece> heads,
            List<IArmorPiece> chests,
            List<IArmorPiece> gloves,
            List<IArmorPiece> waists,
            List<IArmorPiece> legs
        )
        {
            for (int i = 0; i < heads.Count; i++)
            {
                if (heads[i].ArmorSet == null || heads[i].ArmorSet.IsFull == false)
                    continue;

                IArmorPiece[] setPieces = heads[i].ArmorSet.ArmorPieces;

                IArmorPiece head = setPieces.First(x => x.Type == EquipmentType.Head);
                IArmorPiece chest = setPieces.First(x => x.Type == EquipmentType.Chest);
                IArmorPiece glove = setPieces.First(x => x.Type == EquipmentType.Gloves);
                IArmorPiece waist = setPieces.First(x => x.Type == EquipmentType.Waist);
                IArmorPiece leg = setPieces.First(x => x.Type == EquipmentType.Legs);

                if (heads.Any(x => x.Id == head.Id) == false ||
                    chests.Any(x => x.Id == chest.Id) == false ||
                    gloves.Any(x => x.Id == glove.Id) == false ||
                    waists.Any(x => x.Id == waist.Id) == false ||
                    legs.Any(x => x.Id == leg.Id) == false)
                {
                    heads.RemoveAll(x => x.Id == head.Id);
                    chests.RemoveAll(x => x.Id == chest.Id);
                    gloves.RemoveAll(x => x.Id == glove.Id);
                    waists.RemoveAll(x => x.Id == waist.Id);
                    legs.RemoveAll(x => x.Id == leg.Id);
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
        public static IList<SolverDataEquipmentModel> UnselectOddlySkilled(this IList<SolverDataEquipmentModel> equipments, IEnumerable<IAbility> desiredAbilities)
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

        public static IEnumerable<T> ExcludeEquipmentsNonMatchingAbilities<T>(this IEnumerable<T> equipments, IEnumerable<IAbility> desiredAbilities) where T : IEquipment
        {
            bool whereFunc(T x)
            {
                bool isAbilityWorth = x.Abilities.Any(y => y.IsMatchingDesiredAbilities(desiredAbilities));
                bool isSlotsWorth = IsWorthBySlots(x.Slots);

                return isAbilityWorth || isSlotsWorth;
            }

            IEnumerable<T> filtered = equipments.Where(whereFunc);

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
