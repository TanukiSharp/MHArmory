using MHArmory.Core.DataStructures;

namespace MHArmory.Search.Cutoff
{
    internal class MappedSkillPart
    {
        public int MappedId { get; set; }

        public IArmorSetSkillPart Part { get; set; }
        public MappedSkill[] Skills { get; set; }
        public int Requirement { get; set;}

        public override string ToString()
        {
            return $"Mapped {Part}";
        }
    }
}
