using MHArmory.Core.DataStructures;

namespace MHArmory.Search.Cutoff.Models
{
    internal class SupersetInfo
    {
        public IEquipment Equipment { get; }
        public int MaxSkills { get; }

        public SupersetInfo(IEquipment equipment, int maxSkills)
        {
            Equipment = equipment;
            MaxSkills = maxSkills;
        }
    }
}
