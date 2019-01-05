namespace MHArmory.Search.Cutoff
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
