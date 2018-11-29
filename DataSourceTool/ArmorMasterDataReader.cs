using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MHArmory.Core.DataStructures;

namespace DataSourceTool
{
    public struct ArmorMasterDataHeader
    {
        public ushort Identifier { get; }
        public uint EntryCount { get; }

        public ArmorMasterDataHeader(BinaryReader reader)
        {
            Identifier = reader.ReadUInt16();
            EntryCount = reader.ReadUInt32();
        }
    }

    public enum ArmorType
    {
        Regular,
        FullArmorSet,
        Layered
    }

    public struct ArmorMasterDataEntry
    {
        //public ushort Id { get; }
        public int Index { get; }
        public ArmorType ArmorType { get; }
        public EquipmentType EquipmentType { get; }
        public string Name { get; }

        public ArmorMasterDataEntry(BinaryReader reader, ISet<KeyValueInfo> equipmentNames)
        {
            //Id = reader.ReadUInt16();

            reader.BaseStream.Seek(9, SeekOrigin.Current);

            ArmorType = (ArmorType)reader.ReadByte();

            if (ArmorType == ArmorType.FullArmorSet)
            {
            }

            EquipmentType = (EquipmentType)(reader.ReadByte() + 1);

            reader.BaseStream.Seek(44, SeekOrigin.Current);

            ushort index = reader.ReadUInt16();

            KeyValueInfo kvi = equipmentNames.FirstOrDefault(x => x.Index == index);

            Name = kvi.Value;
            Index = (int)kvi.Index;

            reader.BaseStream.Seek(3, SeekOrigin.Current);
        }

        public override bool Equals(object obj)
        {
            if (obj is ArmorMasterDataEntry other)
                return Index == other.Index;
            return false;
        }

        public override int GetHashCode()
        {
            return Index;
        }

        public static bool operator ==(ArmorMasterDataEntry left, ArmorMasterDataEntry right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ArmorMasterDataEntry left, ArmorMasterDataEntry right)
        {
            return !(left == right);
        }
    }

    public class ArmorMasterDataReader
    {
        private readonly BinaryReader reader;

        public ArmorMasterDataReader(BinaryReader reader)
        {
            this.reader = reader;
        }

        public IList<ArmorMasterDataEntry> Read(ISet<KeyValueInfo> equipmentNames)
        {
            var header = new ArmorMasterDataHeader(reader);

            var entries = new List<ArmorMasterDataEntry>();

            for (int i = 0; i < header.EntryCount; i++)
                entries.Add(new ArmorMasterDataEntry(reader, equipmentNames));

            return entries;
        }
    }
}
