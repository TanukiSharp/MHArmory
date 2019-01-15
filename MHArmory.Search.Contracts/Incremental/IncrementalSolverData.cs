using System.Collections.Generic;
using System.Linq;
using MHArmory.Core.DataStructures;

namespace MHArmory.Search.Contracts.Incremental
{

    public class IncrementalSolverData : ISolverData
    {
        public string Name { get; } = "Incremental solver data";
        public string Author { get; } = "Gediminas Masaitis";
        public string Description { get; } = "Incremental solver data";
        public int Version { get; } = 1;

        public bool EnableSkillCompensation { get; set; }

        public int[] WeaponSlots { get; private set; }
        public ISolverDataEquipmentModel[] AllHeads { get; private set; }
        public ISolverDataEquipmentModel[] AllChests { get; private set; }
        public ISolverDataEquipmentModel[] AllGloves { get; private set; }
        public ISolverDataEquipmentModel[] AllWaists { get; private set; }
        public ISolverDataEquipmentModel[] AllLegs { get; private set; }
        public ISolverDataEquipmentModel[] AllCharms { get; private set; }
        public SolverDataJewelModel[] AllJewels { get; private set; }
        public IAbility[] DesiredAbilities { get; private set; }

        private IDictionary<int, SolverDataJewelModel> jewelsBySkillId;

        public void Setup(IList<int> weaponSlots, IEnumerable<IArmorPiece> heads, IEnumerable<IArmorPiece> chests, IEnumerable<IArmorPiece> gloves, IEnumerable<IArmorPiece> waists,
            IEnumerable<IArmorPiece> legs, IEnumerable<ICharmLevel> charms, IEnumerable<SolverDataJewelModel> jewels, IEnumerable<IAbility> desiredAbilities)
        {
            AllJewels = jewels.ToArray();

            WeaponSlots = weaponSlots.ToArray();
            DesiredAbilities = desiredAbilities.ToArray();

            var desiredSkills = DesiredAbilities.Where(x => x.Level > 0).ToDictionary(x => x.Skill.Id);
            var excludedSkillIds = new HashSet<int>(DesiredAbilities.Where(x => x.Level <= 0).Select(x => x.Skill.Id));

            jewelsBySkillId = AllJewels.ToDictionary(x => x.Jewel.Abilities[0].Skill.Id);
            
            ElectionModel[] allHeads = heads.Select(x => new ElectionModel(x, desiredSkills, excludedSkillIds, jewelsBySkillId)).ToArray();
            ElectionModel[] allChests = chests.Select(x => new ElectionModel(x, desiredSkills, excludedSkillIds, jewelsBySkillId)).ToArray();
            ElectionModel[] allGloves = gloves.Select(x => new ElectionModel(x, desiredSkills, excludedSkillIds, jewelsBySkillId)).ToArray();
            ElectionModel[] allWaists = waists.Select(x => new ElectionModel(x, desiredSkills, excludedSkillIds, jewelsBySkillId)).ToArray();
            ElectionModel[] allLegs = legs.Select(x => new ElectionModel(x, desiredSkills, excludedSkillIds, jewelsBySkillId)).ToArray();
            ElectionModel[] allCharms = charms.Select(x => new ElectionModel(x, desiredSkills, excludedSkillIds, jewelsBySkillId)).ToArray();

            ElectAllEquipment(allHeads, allChests, allGloves, allWaists, allLegs, allCharms);
        }

        private void ElectAllEquipment(
            ElectionModel[] heads,
            ElectionModel[] chests,
            ElectionModel[] gloves,
            ElectionModel[] waists,
            ElectionModel[] legs,
            ElectionModel[] charms)
        {
            AllHeads = ElectEquipments(heads);
            AllChests = ElectEquipments(chests);
            AllGloves = ElectEquipments(gloves);
            AllWaists = ElectEquipments(waists);
            AllLegs = ElectEquipments(legs);
            AllCharms = ElectEquipments(charms);
        }

        private ISolverDataEquipmentModel[] ElectEquipments(ElectionModel[] models)
        {
            /*SetComparisonLists(models);
            var ordered = models
                .OrderBy(x => x.AbsolutelyWorseThan.Count)
                .ThenByDescending(x => x.AbsolutelyBetterThan.Count)
                .ToList();

            var list = models.Where(x => x.AbsolutelyWorseThan.Count <= 0).ToList();*/

            var list = new List<ElectionModel>();
            foreach (ElectionModel model in models)
            {
                ElectEquipment(list, model);
            }
            return list.Select(x => x.Model).ToArray();
        }

        private void SetComparisonLists(IList<ElectionModel> models)
        {
            for (int i = 0; i < models.Count; i++)
            {
                ElectionModel current = models[i];
                for (int j = i + 1; j < models.Count; j++)
                {
                    ElectionModel other = models[j];

                    bool currentBetter = IsBetter(current, other);
                    bool otherBetter = IsBetter(other, current);

                    if (currentBetter)
                    {
                        current.SomewhatBetterThan.Add(other);
                        other.SomewhatWorseThan.Add(current);
                        if (!otherBetter)
                        {
                            current.AbsolutelyBetterThan.Add(other);
                            other.AbsolutelyWorseThan.Add(current);
                        }
                    }
                    if (otherBetter)
                    {
                        current.SomewhatWorseThan.Add(other);
                        other.SomewhatBetterThan.Add(current);
                        if (!currentBetter)
                        {
                            current.AbsolutelyWorseThan.Add(other);
                            other.AbsolutelyBetterThan.Add(current);
                        }
                    }
                }
            }
        }

        private void ElectEquipment(IList<ElectionModel> list, ElectionModel equipment)
        {
            list.Add(equipment);
            for (int i = 0; i < list.Count-1; i++)
            {
                ElectionModel other = list[i];
                if (!other.IsSelected)
                {
                    continue;
                }

                bool currentBetter = IsBetter(equipment, other);
                bool otherBetter = IsBetter(other, equipment);

                if (currentBetter)
                {
                    if (!otherBetter)
                    {
                        other.IsSelected = false;
                        //list.Remove(other);
                        //i--;
                    }
                }
                else if (otherBetter)
                {
                    equipment.IsSelected = false;
                    return;
                }
            }
        }

        private bool IsBetter(ElectionModel model, ElectionModel other)
        {
            if (HasBetterByNonStats(model, other))
            {
                return true;
            }

            /*if(!HasBetterByNonStats(other, model))
            {
                if (HasBetterStats(model, other))
                {
                    return true;
                }
            }*/

            return false;
        }

        private bool HasBetterByNonStats(ElectionModel model, ElectionModel other)
        {
            if (!model.FullSet && other.FullSet)
            {
                return true;
            }

            if (HasBetterSlotEfficiency(model, other))
            {
                return true;
            }

            /*if (HasBetterSlots(model, other))
            {
                return true;
            }*/

            if (HasBetterDesiredAbilities(model, other))
            {
                return true;
            }

            return false;
        }

        private bool HasBetterSlotEfficiency(ElectionModel model, ElectionModel other)
        {
            foreach (KeyValuePair<int, int> efficiencyPair in model.SkillEfficiencies)
            {
                int skillId = efficiencyPair.Key;
                int efficiency = efficiencyPair.Value;
                int otherEfficiency = other.SkillEfficiencies[skillId];
                if (efficiency > otherEfficiency)
                {
                    return true;
                }
            }
            return false;
        }

        private bool HasBetterSlots(ElectionModel model, ElectionModel other)
        {
            if (model.SlotCount > other.SlotCount)
            {
                return true;
            }

            if (model.MaxSlot > other.MaxSlot)
            {
                return true;
            }

            if (model.SlotSum > other.SlotSum)
            {
                return true;
            }

            return false;
        }

        private bool HasBetterDesiredAbilities(ElectionModel model, ElectionModel other)
        {
            if (model.Equipment.Name["EN"].StartsWith("Nerg") && other.Equipment.Name["EN"].StartsWith("Nerg"))
            {
                int foo = 123;
                if (foo == 0)
                {
                    return true;
                }
            }

            int compensatedBySots = 0;
            bool canCompensateBySlots = EnableSkillCompensation;
            var extraSlots = other.Equipment.Slots.ToList();
            foreach (int slot in model.Equipment.Slots)
            {
                if (extraSlots.Contains(slot))
                {
                    extraSlots.Remove(slot);
                }
                else
                {
                    canCompensateBySlots = false;
                }
            }

            foreach (IAbility desiredAbility in DesiredAbilities)
            {
                int currentLevels = 0;
                int otherLevels = 0;

                if (model.DesiredAbilities.TryGetValue(desiredAbility.Skill.Id, out IAbility containedAbility))
                {
                    currentLevels = containedAbility.Level;
                }

                if (other.DesiredAbilities.TryGetValue(desiredAbility.Skill.Id, out IAbility otherContainedAbility))
                {
                    otherLevels = otherContainedAbility.Level;
                }

                int levelsMissing = currentLevels - otherLevels;

                if (levelsMissing > 0)
                {
                    //canCompensateBySlots = false;
                    if (canCompensateBySlots)
                    {
                        int currentCompensation = CompensateBySlots(desiredAbility, levelsMissing, extraSlots);
                        compensatedBySots += currentCompensation;
                        if (currentCompensation != 0)
                        {
                            continue;
                        }
                    }

                    return true;
                }
            }

            if (model.DesiredSkillSum > other.DesiredSkillSum + compensatedBySots)
            {
                return true;
            }

            return false;
        }

        private int CompensateBySlots(IAbility desiredAbility, int levelsMissing, IList<int> extraSlots)
        {
            bool jewelExists = jewelsBySkillId.TryGetValue(desiredAbility.Skill.Id, out SolverDataJewelModel jewel);
            if (!jewelExists)
            {
                return 0;
            }
            int jewelSize = jewel.Jewel.SlotSize;

            int compensatedBySots = 0;
            for (int i = 0; i < levelsMissing; i++)
            {
                if (extraSlots.Contains(jewelSize))
                {
                    compensatedBySots += 1;
                    extraSlots.Remove(jewelSize);
                }
                else
                {
                    return 0;
                }
            }
            return compensatedBySots;
        }

        private bool HasBetterStats(ElectionModel model, ElectionModel other)
        {
            if (model.Equipment.Type == EquipmentType.Charm)
            {
                return false;
            }
            var armorPiece = (IArmorPiece)model.Equipment;
            var otherPiece = (IArmorPiece)other.Equipment;

            if (armorPiece.Defense.Augmented > otherPiece.Defense.Augmented)
            {
                return true;
            }

            if (armorPiece.Defense.Max > otherPiece.Defense.Max)
            {
                return true;
            }

            if (armorPiece.Defense.Max > otherPiece.Defense.Max)
            {
                return true;
            }

            if (armorPiece.Resistances.Fire > otherPiece.Resistances.Fire)
            {
                return true;
            }

            if (armorPiece.Resistances.Thunder > otherPiece.Resistances.Thunder)
            {
                return true;
            }

            if (armorPiece.Resistances.Dragon > otherPiece.Resistances.Dragon)
            {
                return true;
            }

            if (armorPiece.Resistances.Ice > otherPiece.Resistances.Ice)
            {
                return true;
            }

            if (armorPiece.Resistances.Water > otherPiece.Resistances.Water)
            {
                return true;
            }

            if (armorPiece.Rarity < otherPiece.Rarity)
            {
                return true;
            }

            return false;
        }

        public ISolverData Done()
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

            return this;
        }
    }
}
