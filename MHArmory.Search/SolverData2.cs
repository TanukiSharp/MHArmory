using System;
using System.Collections.Generic;
using System.Linq;
using MHArmory.Core.DataStructures;

namespace MHArmory.Search
{
    public class SolverDataEquipmentModel2 : ISolverDataEquipmentModel
    {
        public IEquipment Equipment { get; }

        public int MatchingSkillCount { get; set; }
        public bool IsMatchingSlotScore { get; set; }

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

        public SolverDataEquipmentModel2(IEquipment equipment)
        {
            Equipment = equipment;
        }

        public override string ToString()
        {
            return $"[{(IsSelected ? "O" : "X")}] {Equipment.Name}";
        }
    }

    public class SolverData2 : ISolverData
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
        private readonly List<SolverDataEquipmentModel2> inputHeads;
        private readonly List<SolverDataEquipmentModel2> inputChests;
        private readonly List<SolverDataEquipmentModel2> inputGloves;
        private readonly List<SolverDataEquipmentModel2> inputWaists;
        private readonly List<SolverDataEquipmentModel2> inputLegs;
        private readonly List<SolverDataEquipmentModel2> inputCharms;
        private readonly List<SolverDataJewelModel> inputJewels;

        public int MaxSkillCountPerArmorPiece { get; private set; }

        public int MinJewelSize { get; private set; }
        public int MaxJewelSize { get; private set; }

        public SolverData2(
            IList<int> weaponSlots,
            IList<MaximizedSearchCriteria> searchCriterias,
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
            inputHeads = heads.Select(x => new SolverDataEquipmentModel2(x)).ToList();
            inputChests = chests.Select(x => new SolverDataEquipmentModel2(x)).ToList();
            inputGloves = gloves.Select(x => new SolverDataEquipmentModel2(x)).ToList();
            inputWaists = waists.Select(x => new SolverDataEquipmentModel2(x)).ToList();
            inputLegs = legs.Select(x => new SolverDataEquipmentModel2(x)).ToList();
            inputCharms = charms.Select(x => new SolverDataEquipmentModel2(x)).ToList();
            inputJewels = jewels.ToList();

            int maxSkills = inputHeads.Max(x => x.Equipment.Abilities.Length);
            maxSkills = Math.Max(maxSkills, inputChests.Max(x => x.Equipment.Abilities.Length));
            maxSkills = Math.Max(maxSkills, inputGloves.Max(x => x.Equipment.Abilities.Length));
            maxSkills = Math.Max(maxSkills, inputWaists.Max(x => x.Equipment.Abilities.Length));
            maxSkills = Math.Max(maxSkills, inputLegs.Max(x => x.Equipment.Abilities.Length));

            MaxSkillCountPerArmorPiece = maxSkills;

            if (searchCriterias == null || searchCriterias.Count == 0)
                searchCriterias = new[] { MaximizedSearchCriteria.SlotSizeCube };

            inputSearchCriterias = searchCriterias;

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

        private void DeleteMarkedEquipments(List<SolverDataEquipmentModel2> equipments)
        {
            equipments.RemoveAll(x => x.ToBeRemoved);
        }

        private int EquipmentMatchingArmorSetAbility(IEquipment equipment)
        {
            int count = 0;

            if (equipment is IArmorPiece armorPiece && armorPiece.ArmorSet != null && armorPiece.ArmorSet.IsFull == false)
            {
                foreach (IAbility ability in armorPiece.ArmorSet.Skills.SelectMany(x => x.GrantedSkills))
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

        private void ComputeMatchingSkillCount(IEnumerable<SolverDataEquipmentModel2> equipments)
        {
            foreach (SolverDataEquipmentModel2 e in equipments)
            {
                if (e.Equipment.Name.StartsWith("Xeno'jiiva Claws"))
                {
                }

                int skillCount = EquipmentMatchingArmorSetAbility(e.Equipment);
                skillCount += e.Equipment.Abilities.Where(a => IsMatchingDesiredAbilities(a)).Sum(a => a.Level);

                if (skillCount == 0)
                    e.ToBeRemoved = true;
                else
                    e.MatchingSkillCount = skillCount;
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

        private void SetBestSlotScore(IEnumerable<SolverDataEquipmentModel2> equipments)
        {
            SetBestSlotScore(equipments, 3);
            SetBestSlotScore(equipments, 2);
        }

        private void SetBestSlotScore(IEnumerable<SolverDataEquipmentModel2> equipments, int slotCount)
        {
            IEnumerable<SolverDataEquipmentModel2> bestSlots = equipments
                .Where(x => x.Equipment.Slots.Length == slotCount)
                .OrderByDescending(x => x.Equipment.Slots.Sum())
                .ThenByDescending(x => x.Equipment is IArmorPiece armorPiece ? armorPiece.Defense.Augmented : 0)
                .Take(3);

            bool isFirst = true;

            foreach (SolverDataEquipmentModel2 x in bestSlots)
            {
                if (isFirst)
                {
                    x.IsSelected = true;
                    isFirst = false;
                }

                x.IsMatchingSlotScore = true;
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

        private void UpdateSelection(List<SolverDataEquipmentModel2> equipments)
        {
            foreach (SolverDataEquipmentModel2 e in equipments.Where(x => x.IsSelected == false))
            {
                e.IsSelected =
                    e.IsMatchingSlotScore ||
                    e.MatchingSkillCount >= MaxSkillCountPerArmorPiece;
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
            List<SolverDataEquipmentModel2> source,
            List<SolverDataEquipmentModel2> heads,
            List<SolverDataEquipmentModel2> chests,
            List<SolverDataEquipmentModel2> gloves,
            List<SolverDataEquipmentModel2> waists,
            List<SolverDataEquipmentModel2> legs
        )
        {
            for (int i = 0; i < source.Count; i++)
            {
                IArmorSet armorSet = ((IArmorPiece)source[i].Equipment).ArmorSet;

                if (armorSet == null || armorSet.IsFull == false)
                    continue;

                IArmorPiece[] setPieces = armorSet.ArmorPieces;

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
        public static IEnumerable<SolverDataEquipmentModel2> MarkAsToBeRemoved(this IEnumerable<SolverDataEquipmentModel2> source)
        {
            foreach (SolverDataEquipmentModel2 e in source.Where(x => x.ToBeRemoved == false))
                e.ToBeRemoved = true;

            return source;
        }

        public static IEnumerable<SolverDataEquipmentModel2> MarkAsSelected(this IEnumerable<SolverDataEquipmentModel2> source)
        {
            foreach (SolverDataEquipmentModel2 e in source)
            {
                e.ToBeRemoved = false;
                e.IsSelected = true;
            }

            return source;
        }
   }
}
