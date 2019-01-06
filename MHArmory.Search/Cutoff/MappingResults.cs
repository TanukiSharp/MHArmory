namespace MHArmory.Search.Cutoff
{
    internal class MappingResults
    {
        public MappedSkill[] DesiredAbilities { get; set; }
        public MappedSkillPart[] SetParts { get; set; }
        public MappedEquipment[][] Equipment { get; set; }
        public MappedJewel[] Jewels { get; set; }
    }
}
