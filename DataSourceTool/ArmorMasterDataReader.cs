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
        Regular = 0,
        Unknown = 1,
        Layered = 2,
        FullArmorSet = 3,
    }

    public struct ArmorMasterDataEntry
    {
        public ushort Id { get; }
        //public int Index { get; }
        public ArmorType ArmorType { get; }
        public EquipmentType EquipmentType { get; }
        public string Name { get; }

        public ArmorMasterDataEntry(BinaryReader reader, ISet<KeyValueInfo> equipmentNames)
        {
            //Id = reader.ReadUInt16();

            reader.BaseStream.Seek(9, SeekOrigin.Current);

            ArmorType = (ArmorType)reader.ReadByte();

            EquipmentType = (EquipmentType)(reader.ReadByte() + 1);

            reader.BaseStream.Seek(42, SeekOrigin.Current);

            Id = reader.ReadUInt16();

            ushort gmdNameIndex = reader.ReadUInt16();

            KeyValueInfo kvi = equipmentNames.FirstOrDefault(x => x.Index == gmdNameIndex);

            Name = kvi.Value;
            //Index = (int)kvi.Index;

            reader.BaseStream.Seek(3, SeekOrigin.Current);
        }

        public override string ToString()
        {
            return $"{Name} [{Id} {EquipmentType}] ({ArmorType})";
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
            {
                var entry = new ArmorMasterDataEntry(reader, equipmentNames);
                if (entry.ArmorType != ArmorType.Unknown && entry.Name != null)
                    entries.Add(entry);
            }

            return entries;
        }
    }
}
