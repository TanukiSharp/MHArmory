using System;
using System.Collections.Generic;
using System.Linq;
using MHArmory.Core.DataStructures;

namespace MHArmory.Search
{
    public class SolverDataJewelModel : IHasAbilities
    {
        public IJewel Jewel { get; }
        public int Available { get; set; }

        IAbility[] IHasAbilities.Abilities { get { return Jewel.Abilities; } }

        public SolverDataJewelModel(IJewel jewel, int available)
        {
            Jewel = jewel;
            Available = available;
        }
    }

    public class SolverDataEquipmentModel : ISolverDataEquipmentModel
    {
        public IEquipment Equipment { get; }

        public bool IsMatchingArmorSetSkill { get; set; }
        public int MatchingSkillTotalLevel { get; set; }
        public double AverageSkillCompletionRatio { get; set; }
        public bool ToBeRemoved { get; set; }

        public event EventHandler SelectionChanged;

        private bool isSelected;
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

        private readonly List<SolverDataEquipmentModel> inputHeads;
        private readonly List<SolverDataEquipmentModel> inputChests;
        private readonly List<SolverDataEquipmentModel> inputGloves;
        private readonly List<SolverDataEquipmentModel> inputWaists;
        private readonly List<SolverDataEquipmentModel> inputLegs;
        private readonly List<SolverDataEquipmentModel> inputCharms;
        private readonly List<SolverDataJewelModel> inputJewels;

        public int MaxSkillCountPerArmorPiece { get; private set; }

        public int MinJewelSize { get; private set; }
        public int MaxJewelSize { get; private set; }

        public SolverData(
            IList<int> weaponSlots,
            IEnumerable<IArmorPiece> heads,
            IEnumerable<IArmorPiece> chests,
            IEnumerable<IArmorPiece> gloves,
            IEnumerable<IArmorPiece> waists,
            IEnumerable<IArmorPiece> legs,
            IEnumerable<ICharmLevel> charms,
            IEnumerable<SolverDataJewelModel> jewels,
            IEnumerable<IAbility> desiredAbilities
        )
        {
            inputHeads = heads.Select(x => new SolverDataEquipmentModel(x)).ToList();
            inputChests = chests.Select(x => new SolverDataEquipmentModel(x)).ToList();
            inputGloves = gloves.Select(x => new SolverDataEquipmentModel(x)).ToList();
            inputWaists = waists.Select(x => new SolverDataEquipmentModel(x)).ToList();
            inputLegs = legs.Select(x => new SolverDataEquipmentModel(x)).ToList();
            inputCharms = charms.Select(x => new SolverDataEquipmentModel(x)).ToList();
            inputJewels = jewels.ToList();

            int maxSkills = inputHeads.MaxOrZero(x => x.Equipment.Abilities.Length);
            maxSkills = Math.Max(maxSkills, inputChests.MaxOrZero(x => x.Equipment.Abilities.Length));
            maxSkills = Math.Max(maxSkills, inputGloves.MaxOrZero(x => x.Equipment.Abilities.Length));
            maxSkills = Math.Max(maxSkills, inputWaists.MaxOrZero(x => x.Equipment.Abilities.Length));
            maxSkills = Math.Max(maxSkills, inputLegs.MaxOrZero(x => x.Equipment.Abilities.Length));

            MaxSkillCountPerArmorPiece = maxSkills;

            WeaponSlots = weaponSlots.ToArray();
            DesiredAbilities = desiredAbilities.ToArray();

            ProcessInputData();
        }

        private void ProcessInputData()
        {
            RemoveJewelsNotMatchingAnySkill();

            if (inputJewels.Count > 0)
            {
                MinJewelSize = inputJewels.Min(x => x.Jewel.SlotSize);
                MaxJewelSize = inputJewels.Max(x => x.Jewel.SlotSize);
            }
            else
            {
                MinJewelSize = 0;
                MaxJewelSize = 0;
            }

            ComputeMatchingSkillCount();

            SetBestSlotScore();

            CheckFullArmorSets();

            DeleteMarkedEquipments();

            UpdateSelection();

            UnselectLessPoweredCharms();
        }

        public ISolverData Done()
        {
            UpdateReferences();

            return this;
        }

        private void UpdateReferences()
        {
            AllHeads = inputHeads.ToArray<ISolverDataEquipmentModel>();
            AllChests = inputChests.ToArray<ISolverDataEquipmentModel>();
            AllGloves = inputGloves.ToArray<ISolverDataEquipmentModel>();
            AllWaists = inputWaists.ToArray<ISolverDataEquipmentModel>();
            AllLegs = inputLegs.ToArray<ISolverDataEquipmentModel>();
            AllCharms = inputCharms.ToArray<ISolverDataEquipmentModel>();
            AllJewels = inputJewels.ToArray<SolverDataJewelModel>();
        }

        private void RemoveJewelsNotMatchingAnySkill()
        {
            inputJewels.RemoveAll(j => j.Jewel.Abilities.Any(a => IsMatchingDesiredAbilities(a)) == false);
        }

        private bool IsMatchingDesiredAbilities(IAbility ability)
        {
            foreach (IAbility desiredAbility in DesiredAbilities)
            {
                if (DataUtility.AreAbilitiesOnSameSkill(ability, desiredAbility))
                    return true;
            }

            return false;
        }

        private void DeleteMarkedEquipments()
        {
            DeleteMarkedEquipments(inputHeads);
            DeleteMarkedEquipments(inputChests);
            DeleteMarkedEquipments(inputGloves);
            DeleteMarkedEquipments(inputWaists);
            DeleteMarkedEquipments(inputLegs);
            DeleteMarkedEquipments(inputCharms);
        }

        private void DeleteMarkedEquipments(List<SolverDataEquipmentModel> equipments)
        {
            equipments.RemoveAll(x => x.ToBeRemoved);
        }

        private int EquipmentMatchingArmorSetAbility(IEquipment equipment)
        {
            int count = 0;

            if (equipment is IArmorPiece armorPiece && armorPiece.ArmorSetSkills != null)
            {
                foreach (IAbility ability in armorPiece.ArmorSetSkills.SelectMany(x => x.Parts).SelectMany(x => x.GrantedSkills))
                {
                    if (IsMatchingDesiredAbilities(ability))
                        count++;
                }
            }

            return count;
        }

        private void ComputeMatchingSkillCount()
        {
            ComputeMatchingSkillCount(inputHeads);
            ComputeMatchingSkillCount(inputChests);
            ComputeMatchingSkillCount(inputGloves);
            ComputeMatchingSkillCount(inputWaists);
            ComputeMatchingSkillCount(inputLegs);
            ComputeMatchingSkillCount(inputCharms);
        }

        private void ComputeMatchingSkillCount(IEnumerable<SolverDataEquipmentModel> equipments)
        {
            foreach (SolverDataEquipmentModel e in equipments)
            {
                int skillCount = EquipmentMatchingArmorSetAbility(e.Equipment);

                if (skillCount > 0)
                    e.IsMatchingArmorSetSkill = true;

                skillCount += e.Equipment.Abilities.Where(a => IsMatchingDesiredAbilities(a)).Sum(a => a.Level);

                if (skillCount == 0)
                    e.ToBeRemoved = true;
                else
                {
                    e.AverageSkillCompletionRatio = e.Equipment.Abilities
                        .Where(a => IsMatchingDesiredAbilities(a))
                        .AverageOrZero(a => a.Level / (double)a.Skill.MaxLevel);

                    e.MatchingSkillTotalLevel = skillCount;
                }
            }
        }

        private void SetBestSlotScore()
        {
            SetBestSlotScore(inputHeads);
            SetBestSlotScore(inputChests);
            SetBestSlotScore(inputGloves);
            SetBestSlotScore(inputWaists);
            SetBestSlotScore(inputLegs);
            SetBestSlotScore(inputCharms);
        }

        private void SetBestSlotScore(IEnumerable<SolverDataEquipmentModel> equipments)
        {
            SetBestSlotScore(equipments, 3);
            SetBestSlotScore(equipments, 2);
        }

        private void SetBestSlotScore(IEnumerable<SolverDataEquipmentModel> equipments, int slotCount)
        {
            IEnumerable<SolverDataEquipmentModel> bestSlots = equipments
                .Where(x => x.Equipment.Slots.Length == slotCount)
                .OrderByDescending(x => x.Equipment.Slots.Sum())
                .ThenByDescending(x => x.Equipment is IArmorPiece armorPiece ? armorPiece.Defense.Augmented : 0)
                .Take(3);

            bool isFirst = true;

            foreach (SolverDataEquipmentModel x in bestSlots)
            {
                if (isFirst)
                {
                    x.IsSelected = true;
                    isFirst = false;
                }

                x.ToBeRemoved = false;
            }
        }

        private void UpdateSelection()
        {
            UpdateSelection(inputHeads);
            UpdateSelection(inputChests);
            UpdateSelection(inputGloves);
            UpdateSelection(inputWaists);
            UpdateSelection(inputLegs);
            UpdateSelection(inputCharms);
        }

        private void UpdateSelection(List<SolverDataEquipmentModel> equipments)
        {
            foreach (SolverDataEquipmentModel e in equipments.Where(x => x.IsSelected == false))
            {
                e.IsSelected =
                    e.IsMatchingArmorSetSkill ||
                    e.AverageSkillCompletionRatio >= Heuristics.MinimumAverageSkillCompletionRatio ||
                    e.MatchingSkillTotalLevel >= MaxSkillCountPerArmorPiece;
            }
        }

        private void UnselectLessPoweredCharms()
        {
            foreach (IGrouping<int, SolverDataEquipmentModel> group in inputCharms.Where(x => x.Equipment is ICharmLevel).GroupBy(x => ((ICharmLevel)x.Equipment).Charm.Id))
            {
                int max = group.Max(x => ((ICharmLevel)x.Equipment).Level);

                foreach (SolverDataEquipmentModel charmLevel in group)
                {
                    charmLevel.IsSelected = ((ICharmLevel)charmLevel.Equipment).Level == max;
                }
            }
        }

        private void CheckFullArmorSets()
        {
            CheckFullArmorSets(inputHeads, inputHeads, inputChests, inputGloves, inputWaists, inputLegs);
            CheckFullArmorSets(inputChests, inputHeads, inputChests, inputGloves, inputWaists, inputLegs);
            CheckFullArmorSets(inputGloves, inputHeads, inputChests, inputGloves, inputWaists, inputLegs);
            CheckFullArmorSets(inputWaists, inputHeads, inputChests, inputGloves, inputWaists, inputLegs);
            CheckFullArmorSets(inputLegs, inputHeads, inputChests, inputGloves, inputWaists, inputLegs);
        }

        private void CheckFullArmorSets(
            List<SolverDataEquipmentModel> source,
            List<SolverDataEquipmentModel> heads,
            List<SolverDataEquipmentModel> chests,
            List<SolverDataEquipmentModel> gloves,
            List<SolverDataEquipmentModel> waists,
            List<SolverDataEquipmentModel> legs
        )
        {
            for (int i = 0; i < source.Count; i++)
            {
                IFullArmorSet fullArmorSet = ((IArmorPiece)source[i].Equipment).FullArmorSet;

                if (fullArmorSet == null)
                    continue;

                IArmorPiece[] setPieces = fullArmorSet.ArmorPieces;

                IArmorPiece head = setPieces.First(x => x.Type == EquipmentType.Head);
                IArmorPiece chest = setPieces.First(x => x.Type == EquipmentType.Chest);
                IArmorPiece glove = setPieces.First(x => x.Type == EquipmentType.Gloves);
                IArmorPiece waist = setPieces.First(x => x.Type == EquipmentType.Waist);
                IArmorPiece leg = setPieces.First(x => x.Type == EquipmentType.Legs);

                if (heads.Where(x => x.ToBeRemoved == false).Any(x => x.Equipment.Id == head.Id) == false ||
                    chests.Where(x => x.ToBeRemoved == false).Any(x => x.Equipment.Id == chest.Id) == false ||
                    gloves.Where(x => x.ToBeRemoved == false).Any(x => x.Equipment.Id == glove.Id) == false ||
                    waists.Where(x => x.ToBeRemoved == false).Any(x => x.Equipment.Id == waist.Id) == false ||
                    legs.Where(x => x.ToBeRemoved == false).Any(x => x.Equipment.Id == leg.Id) == false)
                {
                    // if one piece is marked as to be removed, remove them all

                    heads.Where(x => x.Equipment.Id == head.Id).MarkAsToBeRemoved();
                    chests.Where(x => x.Equipment.Id == chest.Id).MarkAsToBeRemoved();
                    gloves.Where(x => x.Equipment.Id == glove.Id).MarkAsToBeRemoved();
                    waists.Where(x => x.Equipment.Id == waist.Id).MarkAsToBeRemoved();
                    legs.Where(x => x.Equipment.Id == leg.Id).MarkAsToBeRemoved();
                }
                else
                {
                    // otherwise, ensure they are all selected and not marked as to be removed

                    heads.Where(x => x.Equipment.Id == head.Id).MarkAsSelected();
                    chests.Where(x => x.Equipment.Id == chest.Id).MarkAsSelected();
                    gloves.Where(x => x.Equipment.Id == glove.Id).MarkAsSelected();
                    waists.Where(x => x.Equipment.Id == waist.Id).MarkAsSelected();
                    legs.Where(x => x.Equipment.Id == leg.Id).MarkAsSelected();
                }
            }
        }
    }

    public static class Operators
    {
        public static int MaxOrZero<T>(this IEnumerable<T> source, Func<T, int> func)
        {
            if (source.Any() == false)
                return 0;

            return source.Max(func);
        }

        public static double MaxOrZero<T>(this IEnumerable<T> source, Func<T, double> func)
        {
            if (source.Any() == false)
                return 0.0;

            return source.Max(func);
        }

        public static double AverageOrZero(this IEnumerable<IAbility> source, Func<IAbility, double> func)
        {
            if (source.Any() == false)
                return 0.0;

            return source.Average(func);
        }

        public static IEnumerable<SolverDataEquipmentModel> MarkAsToBeRemoved(this IEnumerable<SolverDataEquipmentModel> source)
        {
            foreach (SolverDataEquipmentModel e in source.Where(x => x.ToBeRemoved == false))
                e.ToBeRemoved = true;

            return source;
        }

        public static IEnumerable<SolverDataEquipmentModel> MarkAsSelected(this IEnumerable<SolverDataEquipmentModel> source)
        {
            foreach (SolverDataEquipmentModel e in source)
            {
                e.ToBeRemoved = false;
                e.IsSelected = true;
            }

            return source;
        }
    }
}
