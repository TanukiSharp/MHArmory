using System.Linq;
using MHArmory.Search.Cutoff.Models.Mapped;

namespace MHArmory.Search.Cutoff.Models
{
    internal class Combination
    {
        public MappedEquipment[] Equipments { get; }
        public int[] Slots { get; }
        public ArmorSetSkillGrant[] Grants { get; }
        public int[] RemainingSkills { get; }
        public MappedJewel[] Jewels { get; }
        
        public int[] ExcludedAbilityIndices { get; }

        public Combination(MappedEquipment[] initialEquipment, int[] weaponSlots, MappingResults results)
        {
            Equipments = initialEquipment.ToArray();
            Slots = new int[CutoffSearchConstants.Slots + 1];
            foreach (int weaponSlot in weaponSlots)
            {
                Slots[weaponSlot]++;
            }
            Grants = results.SetParts.Select(x => new ArmorSetSkillGrant(x)).ToArray();
            RemainingSkills = results.DesiredAbilities.Select(x => x.Level).ToArray();
            ExcludedAbilityIndices = results.DesiredAbilities.Where(x => x.Level == 0).Select(x => x.MappedId).ToArray();
            Jewels = results.Jewels;
            foreach (MappedEquipment equipment in initialEquipment)
            {
                Add(equipment);
            }
        }

        public Combination(Combination combination)
        {
            Equipments = (MappedEquipment[]) combination.Equipments.Clone();
            Slots = (int[]) combination.Slots.Clone();
            Grants = combination.Grants.Select(x => new ArmorSetSkillGrant(x.Part) {Progress = x.Progress}).ToArray();
            RemainingSkills = (int[]) combination.RemainingSkills.Clone();
            Jewels = (MappedJewel[]) combination.Jewels.Clone();
            ExcludedAbilityIndices = combination.ExcludedAbilityIndices;
        }

        public void Replace(int index, MappedEquipment replacement)
        {
            Subtract(Equipments[index]);
            Add(replacement);
            Equipments[index] = replacement;
        }

        private void Add(MappedEquipment equipment)
        {
            if (equipment.Equipment == null)
            {
                return;
            }
            foreach (MappedSkill skill in equipment.Skills)
            {
                RemainingSkills[skill.MappedId] -= skill.Level;
            }

            foreach (int slot in equipment.Equipment.Slots)
            {
                Slots[slot]++;
            }

            foreach (MappedSkillPart part in equipment.SkillParts)
            {
                ArmorSetSkillGrant grant = Grants[part.MappedId];
                grant.Progress++;
                if (grant.Progress == grant.Part.Requirement)
                {
                    foreach (MappedSkill grantedAbility in grant.Part.Skills)
                    {
                        RemainingSkills[grantedAbility.MappedId] -= grantedAbility.Level;
                    }
                }
            }
        }

        private void Subtract(MappedEquipment equipment)
        {
            if (equipment.Equipment == null)
            {
                return;
            }
            foreach (MappedSkill skill in equipment.Skills)
            {
                RemainingSkills[skill.MappedId] += skill.Level;
            }

            foreach (int slot in equipment.Equipment.Slots)
            {
                Slots[slot]--;
            }

            foreach (MappedSkillPart part in equipment.SkillParts)
            {
                ArmorSetSkillGrant grant = Grants[part.MappedId];
                if (grant.Progress == grant.Part.Requirement)
                {
                    foreach (MappedSkill grantedAbility in grant.Part.Skills)
                    {
                        RemainingSkills[grantedAbility.MappedId] += grantedAbility.Level;
                    }
                }
                grant.Progress--;
            }
        }
    }
}
