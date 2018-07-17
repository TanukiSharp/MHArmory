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
    }

    public class SolverData
    {
        public IList<SolverDataEquipmentModel> AllHeads { get; private set; }
        public IList<SolverDataEquipmentModel> AllChests { get; private set; }
        public IList<SolverDataEquipmentModel> AllGloves { get; private set; }
        public IList<SolverDataEquipmentModel> AllWaists { get; private set; }
        public IList<SolverDataEquipmentModel> AllLegs { get; private set; }
        public IList<SolverDataEquipmentModel> AllCharms { get; private set; }
        public IList<IJewel> AllJewels { get; private set; }
        public IList<IAbility> DesiredAbilities { get; }

        private readonly IList<IArmorPiece> inputHeads;
        private readonly IList<IArmorPiece> inputChests;
        private readonly IList<IArmorPiece> inputGloves;
        private readonly IList<IArmorPiece> inputWaists;
        private readonly IList<IArmorPiece> inputLegs;
        private readonly IList<ICharmLevel> inputCharms;
        private readonly IList<IJewel> inputJewels;

        private IList<SolverDataEquipmentModel> allHeads;
        private IList<SolverDataEquipmentModel> allChests;
        private IList<SolverDataEquipmentModel> allGloves;
        private IList<SolverDataEquipmentModel> allWaists;
        private IList<SolverDataEquipmentModel> allLegs;
        private IList<SolverDataEquipmentModel> allCharms;
        private IList<IJewel> allJewels;

        public SolverData(
            IList<IArmorPiece> heads,
            IList<IArmorPiece> chests,
            IList<IArmorPiece> gloves,
            IList<IArmorPiece> waists,
            IList<IArmorPiece> legs,
            IList<ICharmLevel> charms,
            IList<IJewel> jewels,
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

            DesiredAbilities = desiredAbilities;

            SplitEquipmentsPerType();
            allJewels = MapJewels(inputJewels);
        }

        private void SplitEquipmentsPerType()
        {
            allHeads = MapEquipment(ExcludeLessPoweredEquivalent(inputHeads, IsLessPoweredEquivalentArmorPiece));
            allChests = MapEquipment(ExcludeLessPoweredEquivalent(inputChests, IsLessPoweredEquivalentArmorPiece));
            allGloves = MapEquipment(ExcludeLessPoweredEquivalent(inputGloves, IsLessPoweredEquivalentArmorPiece));
            allWaists = MapEquipment(ExcludeLessPoweredEquivalent(inputWaists, IsLessPoweredEquivalentArmorPiece));
            allLegs = MapEquipment(ExcludeLessPoweredEquivalent(inputLegs, IsLessPoweredEquivalentArmorPiece));
            allCharms = MapEquipment(ExcludeLessPoweredEquivalent(inputCharms, IsLessPoweredEquivalentEquipment));
        }

        private IList<SolverDataEquipmentModel> MapEquipment(IEnumerable<IEquipment> equipments)
        {
            return equipments
                .Where(x => x.Abilities.Any(IsAbilityMatchingDesiredAbilities))
                .Select(x => new SolverDataEquipmentModel(x))
                .ToList();
        }

        private IList<IJewel> MapJewels(IEnumerable<IJewel> jewels)
        {
            return jewels
                .Where(x => x.Abilities.Any(IsAbilityMatchingDesiredAbilities))
                .ToList();
        }

        private bool IsAbilityMatchingDesiredAbilities(IAbility ability)
        {
            foreach (IAbility desiredAbility in DesiredAbilities)
            {
                if (AreAbilitiesOnSameSkill(ability, desiredAbility))
                    return true;
            }

            return false;
        }

        private bool AreAbilitiesOnSameSkill(IAbility ability1, IAbility ability2)
        {
            return ability1 != null && ability2 != null && ability1.Skill.Id == ability2.Skill.Id;
        }

        private void RemoveUseless()
        {
            
        }

        private void Meh()
        {
            //int maxLength = Math.Max(
            //    allHeads.Count,
            //    Math.Max(
            //        allChests.Count,
            //        Math.Max(
            //            AllGloves.Count,
            //            Math.Max(
            //                allWaists.Count,
            //                Math.Max(
            //                    allLegs.Count,
            //                    allCharms.Count
            //                )
            //            )
            //        )
            //    )
            //);

            //int[] weights = new int[maxLength];

            //allHeads = GetMaxWeightedArmorPieces(allHeads, weights, desiredAbilities);
            //allChests = GetMaxWeightedArmorPieces(allChests, weights, desiredAbilities);
            //allGloves = GetMaxWeightedArmorPieces(allGloves, weights, desiredAbilities);
            //allWaists = GetMaxWeightedArmorPieces(allWaists, weights, desiredAbilities);
            //allLegs = GetMaxWeightedArmorPieces(allLegs, weights, desiredAbilities);
            //allCharms = GetMaxWeightedArmorPieces(allCharms, weights, desiredAbilities);

            //foreach (IAbility selectedAbility in desiredAbilities)
            //{
            //    if (skillsToCharmsMap.TryGetValue(selectedAbility.Skill.Id, out IList<ICharm> matchingCharms))
            //        allCharms.AddRange(matchingCharms.SelectMany(x => x.Levels));

            //    if (skillsToJewelsMap.TryGetValue(selectedAbility.Skill.Id, out IList<IJewel> matchingJewels))
            //        jewels.AddRange(matchingJewels);
            //}

            //IList<ICharmLevel> charms = GetMaxWeightedArmorPieces(allCharms, weights, desiredAbilities);
        }

        public void SecondPass()
        {
        }

        public SolverData Done()
        {
            UpdateReferences();
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

        public static IList<T> ExcludeLessPoweredEquivalent<T>(IList<T> equipments, Func<T, T, bool> checkFunc)
        {
            for (int i = 0; i < equipments.Count; i++)
            {
                for (int j = 0; j < equipments.Count; j++)
                {
                    if (i == j)
                        continue;

                    if (checkFunc(equipments[i], equipments[j]))
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

        public static bool IsLessPoweredEquivalentEquipment(IEquipment baseEquipment, IEquipment checkedEquipment)
        {
            if (checkedEquipment.Slots.Length > baseEquipment.Slots.Length)
                return false;

            for (int i = 0; i < checkedEquipment.Slots.Length; i++)
            {
                if (checkedEquipment.Slots[i] > baseEquipment.Slots[i])
                    return false;
            }

            if (checkedEquipment.Abilities.Length > baseEquipment.Abilities.Length)
                return false;

            IAbility[] checkedAbilities = checkedEquipment.Abilities;
            IAbility[] baseAbilities = baseEquipment.Abilities;

            for (int i = 0; i < checkedAbilities.Length; i++)
            {
                bool hasMatchingAbility = false;

                for (int j = 0; j < baseAbilities.Length; j++)
                {
                    if (checkedAbilities[i].Skill.Id == baseAbilities[j].Skill.Id)
                    {
                        if (checkedAbilities[i].Level > baseAbilities[j].Level)
                            return false;

                        hasMatchingAbility = true;
                    }
                }

                if (hasMatchingAbility == false)
                    return false;
            }

            return true;
        }

        private static bool IsLessPoweredEquivalentArmorPiece(IArmorPiece baseArmorPiece, IArmorPiece checkedArmorPiece)
        {
            if (IsLessPoweredEquivalentEquipment(baseArmorPiece, checkedArmorPiece) == false)
                return false;

            IAbility[] checkedAbilities = checkedArmorPiece.Abilities;
            IAbility[] baseAbilities = baseArmorPiece.Abilities;

            // From here, checkedAbilities has less or equal amount of abilities compared to baseAbilities.

            if (checkedAbilities.Length == baseAbilities.Length)
            {
                // When there is an equal amount of abilities, they match their skills.

                for (int i = 0; i < checkedAbilities.Length; i++)
                {
                    for (int j = 0; j < baseAbilities.Length; j++)
                    {
                        if (checkedAbilities[i].Skill.Id == baseAbilities[j].Skill.Id)
                        {
                            // The double loop with skill equality check is to avoid ordering issues.
                            // Ability checkedAbilities[i] is not necessarily the same as baseAbilities[i].

                            if (checkedAbilities[i].Level < baseAbilities[j].Level)
                                return true;
                        }
                    }
                }

                // Here all skills have equal level of ability, let's consider as less powered
                // if the checkedArmorPiece has less augmented defense.

                return checkedArmorPiece.Defense.Augmented <= baseArmorPiece.Defense.Augmented;
            }

            return true;
        }

        private IList<T> GetMaxWeightedArmorPieces<T>(IList<T> armorPieces, int[] weights, IEnumerable<IAbility> desiredAbilities) where T : IEquipment
        {
            int max = 0;

            for (int i = 0; i < armorPieces.Count; i++)
            {
                weights[i] = 0;

                foreach (IAbility ability in desiredAbilities)
                {
                    weights[i] += armorPieces[i].Abilities.Where(a => a.Skill.Id == ability.Skill.Id).Sum(a => a.Level);
                    if (weights[i] > max)
                        max = weights[i];
                }
            }

            if (max == 0)
                return new T[] { default(T) };

            //int maxMin = Math.Max(1, max - 1);

            //return armorPieces
            //    .Where((x, i) => max >= weights[i] && weights[i] >= maxMin)
            //    .ToList();

            return armorPieces
                .Where((x, i) => weights[i] > 0)
                .ToList();
        }
    }
}
