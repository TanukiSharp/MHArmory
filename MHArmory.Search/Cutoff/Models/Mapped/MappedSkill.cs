namespace MHArmory.Search.Cutoff.Models.Mapped
{
    internal class MappedSkill
    {
        public int MappedId { get; set; }

        public int SkillId { get; set; }
        public int Level { get; set; }

        public MappedSkill CopyWithLevel(int level)
        {
            var copy = new MappedSkill
            {
                MappedId = MappedId,
                SkillId = SkillId,
                Level = level
            };
            return copy;
        }

        public MappedSkill Copy()
        {
            return CopyWithLevel(Level);
        }

        public override string ToString()
        {
            return $"Mapped skill {SkillId} lv{Level}";
        }
    }
}
