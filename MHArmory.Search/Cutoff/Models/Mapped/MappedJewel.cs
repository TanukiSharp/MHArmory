using MHArmory.Search.Contracts;

namespace MHArmory.Search.Cutoff.Models.Mapped
{
    internal class MappedJewel
    {
        public SolverDataJewelModel Jewel { get; set; }
        public MappedSkill Skill { get; set; }

        public override string ToString()
        {
            return $"Mapped {Jewel.Jewel} x {Jewel.Available}";
        }
    }
}
