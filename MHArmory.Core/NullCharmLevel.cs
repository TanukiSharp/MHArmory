using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.Core.DataStructures;

namespace MHArmory.Core
{
    public class NullCharm : ICharm
    {
        public static readonly ICharm Default = new NullCharm();

        public int Id { get; } = -1;
        public Dictionary<string, string> Name { get; } = new Dictionary<string, string> { [Localization.DefaultLanguage] = "<none>" };
        public ICharmLevel[] Levels { get; } = new ICharmLevel[0];
    }

    public class NullCharmLevel : ICharmLevel
    {
        public static readonly ICharmLevel Default = new NullCharmLevel();

        public ICharm Charm { get; } = NullCharm.Default;
        public int Level { get; } = 0;
        public int Id { get; } = -1;
        public EquipmentType Type { get; } = EquipmentType.Charm;
        public Dictionary<string, string> Name { get; } = new Dictionary<string, string> { [Localization.DefaultLanguage] = "<none>" };
        public int Rarity { get; } = 0;
        public int[] Slots { get; } = new int[0];
        public IEvent Event { get; } = null;
        public IAbility[] Abilities { get; } = new IAbility[0];
        public ICraftMaterial[] CraftMaterials { get; } = new ICraftMaterial[0];

        public void UpdateCharm(ICharm charm)
        {
        }
    }
}
