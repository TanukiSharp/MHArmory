using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MHArmory.Core.DataStructures;

namespace MHArmory.Search.Contracts
{
    public class IncrementalSolverData : ISolverData
    {
        public string Name { get; } = "Incremental solver data";
        public string Author { get; } = "Gediminas Masaitis";
        public string Description { get; } = "Incremental solver data";
        public int Version { get; } = 1;

        public int[] WeaponSlots { get; private set; }
        public ISolverDataEquipmentModel[] AllHeads { get; private set; }
        public ISolverDataEquipmentModel[] AllChests { get; private set; }
        public ISolverDataEquipmentModel[] AllGloves { get; private set; }
        public ISolverDataEquipmentModel[] AllWaists { get; private set; }
        public ISolverDataEquipmentModel[] AllLegs { get; private set; }
        public ISolverDataEquipmentModel[] AllCharms { get; private set; }
        public SolverDataJewelModel[] AllJewels { get; private set; }
        public IAbility[] DesiredAbilities { get; private set; }

        public void Setup(IList<int> weaponSlots, IEnumerable<IArmorPiece> heads, IEnumerable<IArmorPiece> chests, IEnumerable<IArmorPiece> gloves, IEnumerable<IArmorPiece> waists,
            IEnumerable<IArmorPiece> legs, IEnumerable<ICharmLevel> charms, IEnumerable<SolverDataJewelModel> jewels, IEnumerable<IAbility> desiredAbilities)
        {
            AllJewels = jewels.ToArray();

            WeaponSlots = weaponSlots.ToArray();
            DesiredAbilities = desiredAbilities.ToArray();

            var desiredSkillIds = new HashSet<int>(DesiredAbilities.Where(x => x.Level > 0).Select(x => x.Skill.Id));
            var excludedSkillIds = new HashSet<int>(DesiredAbilities.Where(x => x.Level <= 0).Select(x => x.Skill.Id));


            ElectionModel[] allHeads = heads.Select(x => new ElectionModel(x, desiredSkillIds, excludedSkillIds)).ToArray();
            ElectionModel[] allChests = chests.Select(x => new ElectionModel(x, desiredSkillIds, excludedSkillIds)).ToArray();
            ElectionModel[] allGloves = gloves.Select(x => new ElectionModel(x, desiredSkillIds, excludedSkillIds)).ToArray();
            ElectionModel[] allWaists = waists.Select(x => new ElectionModel(x, desiredSkillIds, excludedSkillIds)).ToArray();
            ElectionModel[] allLegs = legs.Select(x => new ElectionModel(x, desiredSkillIds, excludedSkillIds)).ToArray();
            ElectionModel[] allCharms = charms.Select(x => new ElectionModel(x, desiredSkillIds, excludedSkillIds)).ToArray();

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
            var list = new List<ElectionModel>();
            foreach (ElectionModel model in models)
            {
                ElectEquipment(list, model);
            }
            return list.Select(x => x.Model).ToArray();
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
            if (HasBetterSlotsOrAbilities(model, other))
            {
                return true;
            }

            if(!HasBetterSlotsOrAbilities(other, model))
            {
                if (HasBetterStats(model, other))
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasBetterSlotsOrAbilities(ElectionModel model, ElectionModel other)
        {
            if (HasBetterSlots(model, other))
            {
                return true;
            }

            if (HasBetterDesiredAbilities(model, other))
            {
                return true;
            }

            return false;
        }

        private static bool HasBetterSlots(ElectionModel model, ElectionModel other)
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

                if (currentLevels > otherLevels)
                {
                    return true;
                }
            }

            if (model.DesiredSkillSum > other.DesiredSkillSum)
            {
                return true;
            }

            return false;
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
