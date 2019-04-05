using System;
using System.Collections.Generic;
using System.Text;

namespace MHArmory.Core.DataStructures
{
    public interface IHasAbilities
    {
        IAbility[] Abilities { get; }
    }

    public interface ILocalizedItem
    {
        int Id { get; }
        Dictionary<string, string> Values { get; }
    }

    public class LocalizedItem : ILocalizedItem
    {
        public int Id { get; }
        public Dictionary<string, string> Values { get; }

        public LocalizedItem(int id, Dictionary<string, string> values)
        {
            Id = id;
            Values = values;
        }
    }

    public interface ICraftMaterial
    {
        ILocalizedItem LocalizedItem { get; }
        int Quantity { get; }
    }

    public class CraftMaterial : ICraftMaterial
    {
        public ILocalizedItem LocalizedItem { get; }
        public int Quantity { get; }

        public CraftMaterial(ILocalizedItem localizedItem, int quantity)
        {
            LocalizedItem = localizedItem;
            Quantity = quantity;
        }

        public override string ToString()
        {
            return $"{Localization.GetDefault(LocalizedItem)} {Quantity}";
        }
    }

    public interface IEquipment : IHasAbilities
    {
        int Id { get; }
        EquipmentType Type { get; }
        Dictionary<string, string> Name { get; }
        int Rarity { get; }
        int[] Slots { get; }
        IEvent Event { get; }
        ICraftMaterial[] CraftMaterials { get; }
    }
}
