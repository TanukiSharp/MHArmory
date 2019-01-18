using MHArmory.Core;
using MHArmory.Core.DataStructures;
using MHArmory.Search.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MHArmory.Search.Default
{
    public class SolverData : ISolverData
    {
        public const double MinimumAverageSkillCompletionRatio = 0.75;

        public int[] WeaponSlots { get; private set; }
        public ISolverDataEquipmentModel[] AllHeads { get; private set; }
        public ISolverDataEquipmentModel[] AllChests { get; private set; }
        public ISolverDataEquipmentModel[] AllGloves { get; private set; }
        public ISolverDataEquipmentModel[] AllWaists { get; private set; }
        public ISolverDataEquipmentModel[] AllLegs { get; private set; }
        public ISolverDataEquipmentModel[] AllCharms { get; private set; }
        public SolverDataJewelModel[] AllJewels { get; private set; }
        public IAbility[] DesiredAbilities { get; private set; }

        private List<SolverDataEquipmentModel> inputHeads;
        private List<SolverDataEquipmentModel> inputChests;
        private List<SolverDataEquipmentModel> inputGloves;
        private List<SolverDataEquipmentModel> inputWaists;
        private List<SolverDataEquipmentModel> inputLegs;
        private List<SolverDataEquipmentModel> inputCharms;
        private List<SolverDataJewelModel> inputJewels;

        public int MaxSkillCountPerArmorPiece { get; private set; }

        public string Name { get; } = "Armory - Default";
        public string Author { get; } = "TanukiSharp";
        public string Description { get; } = "Default solver data processor";
        public int Version { get; } = 1;

        public void Setup(
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

            UpdateReferences();

            FreezeEquipmentSelection();
        }

        private void ProcessInputData()
        {
            RemoveJewelsNotMatchingAnySkill();
            RemoveJewelsMatchingExcludedSkills();

            RemoveEquipmentsBySkillExclusion();

            ComputeMatchingSkillCount();

            SetBestSlotScore();

            CheckFullArmorSets();

            DeleteMarkedEquipments();

            UpdateSelection();

            UnselectLessPoweredCharms();
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

        private void FreezeEquipmentSelection()
        {
            foreach (ISolverDataEquipmentModel x in AllHeads)
                x.FreezeSelection();

            foreach (ISolverDataEquipmentModel x in AllChests)
                x.FreezeSelection();

            foreach (ISolverDataEquipmentModel x in AllGloves)
                x.FreezeSelection();

            foreach (ISolverDataEquipmentModel x in AllWaists)
                x.FreezeSelection();

            foreach (ISolverDataEquipmentModel x in AllLegs)
                x.FreezeSelection();

            foreach (ISolverDataEquipmentModel x in AllCharms)
                x.FreezeSelection();
        }

        private void RemoveJewelsNotMatchingAnySkill()
        {
            inputJewels.RemoveAll(j => j.Jewel.Abilities.Any(a => IsMatchingDesiredAbilities(a)) == false);
        }

        private void RemoveJewelsMatchingExcludedSkills()
        {
            inputJewels.RemoveAll(j => j.Jewel.Abilities.Any(a => IsMatchingExcludedSkill(a)));
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

        private bool IsMatchingExcludedSkill(IAbility ability)
        {
            foreach (IAbility desiredAbility in DesiredAbilities.Where(x => x.Level == 0))
            {
                if (DataUtility.AreAbilitiesOnSameSkill(ability, desiredAbility))
                    return true;
            }

            return false;
        }

        private void RemoveEquipmentsBySkillExclusion()
        {
            IList<IAbility> excludedAbilities = DesiredAbilities.Where(x => x.Level == 0).ToList();

            RemoveEquipmentsBySkillExclusion(excludedAbilities, inputHeads);
            RemoveEquipmentsBySkillExclusion(excludedAbilities, inputChests);
            RemoveEquipmentsBySkillExclusion(excludedAbilities, inputGloves);
            RemoveEquipmentsBySkillExclusion(excludedAbilities, inputWaists);
            RemoveEquipmentsBySkillExclusion(excludedAbilities, inputLegs);
            RemoveEquipmentsBySkillExclusion(excludedAbilities, inputCharms);
        }

        private void RemoveEquipmentsBySkillExclusion(IList<IAbility> excludedAbilities, List<SolverDataEquipmentModel> equipments)
        {
            equipments.RemoveAll(e => e.Equipment.Abilities.Any(a => excludedAbilities.Any(x => DataUtility.AreAbilitiesOnSameSkill(a, x))));
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
                .Where(x => x.ToBeRemoved)
                .Where(x => x.Equipment.Slots.Length == slotCount)
                .OrderByDescending(x => x.Equipment.Slots.Sum())
                .ThenByDescending(x => x.Equipment is IArmorPiece armorPiece ? armorPiece.Defense.Augmented : 0)
                .Take(5);

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
                    e.AverageSkillCompletionRatio >= MinimumAverageSkillCompletionRatio ||
                    e.MatchingSkillTotalLevel >= MaxSkillCountPerArmorPiece ||
                    (e.MatchingSkillTotalLevel == e.Equipment.Abilities.Length && e.Equipment.Slots.Length > 0) ||
                    (e.Equipment.Slots.Length >= 2 && e.Equipment.Slots.Count(x => x >= 2) >= 1);
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
