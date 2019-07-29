using MHArmory.Core.DataStructures;

namespace MHArmory.Search.Cutoff.Models.Mapped
{
    internal class MappedEquipment
    {
        public IEquipment Equipment { get; set; }
        public MappedSkill[] Skills { get; set; }
        public MappedSkillPart[] SkillParts { get; set; }

        public int SkillDebt { get; set; }

        public override string ToString()
        {
            return $"Mapped {Equipment}";
        }
    }
}
