using MHArmory.Search.Cutoff.Models.Mapped;

namespace MHArmory.Search.Cutoff.Models
{
    internal class ArmorSetSkillGrant
    {
        public MappedSkillPart Part { get; }
        public int Progress { get; set; }

        public ArmorSetSkillGrant(MappedSkillPart part)
        {
            Part = part;
        }
    }
}
