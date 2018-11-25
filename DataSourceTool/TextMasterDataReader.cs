using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using MHArmory.Core.DataStructures;
using Microsoft.Extensions.Logging;

namespace DataSourceTool
{
    public struct TextMasterDataHeader
    {
        public uint Magic { get; }
        public uint Version { get; }
        public uint LanguageId { get; }
        public uint KeyCount { get; }
        public uint ValueCount { get; }
        public uint KeyBlockSize { get; }
        public uint ValueBlockSize { get; }
        public string Name { get; }

        private TextMasterDataHeader(
            uint magic,
            uint version,
            uint languageId,
            uint keyCount,
            uint valueCount,
            uint keyBlockSize,
            uint valueBlockSize,
            string name
        )
        {
            Magic = magic;
            Version = version;
            LanguageId = languageId;
            KeyCount = keyCount;
            ValueCount = valueCount;
            KeyBlockSize = keyBlockSize;
            ValueBlockSize = valueBlockSize;
            Name = name;
        }

        public static TextMasterDataHeader Read(BinaryReader reader)
        {
            uint magic = reader.ReadUInt32();
            uint version = reader.ReadUInt32();
            uint languageId = reader.ReadUInt32();
            reader.BaseStream.Seek(8, SeekOrigin.Current); // Skip unknown1 and unknown2
            uint keyCount = reader.ReadUInt32();
            uint valueCount = reader.ReadUInt32();
            uint keyBlockSize = reader.ReadUInt32();
            uint valueBlockSize = reader.ReadUInt32();
            int nameSize = reader.ReadInt32();
            string name = Encoding.UTF8.GetString(reader.ReadBytes(nameSize + 1), 0, nameSize);

            return new TextMasterDataHeader(
                magic,
                version,
                languageId,
                keyCount,
                valueCount,
                keyBlockSize,
                valueBlockSize,
                name
            );
        }
    }

    public struct TextMasterDataInfoEntry
    {
        public uint Index { get; }
        public uint Offset { get; }

        private TextMasterDataInfoEntry(uint index, uint offset)
        {
            Index = index;
            Offset = offset;
        }

        public static TextMasterDataInfoEntry Read(BinaryReader reader)
        {
            uint stringIndex = reader.ReadUInt32();
            reader.BaseStream.Seek(12, SeekOrigin.Current); // Skip unknown1, unknown2, unknown3 and unknown4
            uint keyOffset = reader.ReadUInt32();
            reader.BaseStream.Seek(12, SeekOrigin.Current); // Skip unknown5, unknown6 and unknown7

            return new TextMasterDataInfoEntry(stringIndex, keyOffset);
        }

        public override string ToString()
        {
            return $"[{Index}] {Offset}";
        }
    }

    public class TextMasterDataReader
    {
        public IList<EquipmentInfo> Equipments { get; }

        private readonly List<EquipmentInfo> equipments = new List<EquipmentInfo>();
        private readonly BinaryReader reader;

        private readonly ILogger logger = new ConsoleLogger();

        public TextMasterDataReader(BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            this.reader = reader;

            Equipments = new ReadOnlyCollection<EquipmentInfo>(equipments);
        }

        private const string EquipmentStartPattern = "AM_";
        private const string EquipmentEndPattern = "_NAME";

        private static readonly string[] ArmorPieceType = new[]
        {
            "HEAD",
            "BODY",
            "ARM",
            "WAIST",
            "LEG",
            "ACCE"
        };

        private static readonly string[] SkipValues = new[]
        {
            string.Empty,
            "Unavailable",
            "Invalid Message",
        };

        private static readonly IDictionary<string, string> ReplacementValues = new Dictionary<string, string>
        {
            ["<ICON ALPHA>"] = " α",
            ["<ICON BETA>"] = " β",
            ["<ICON GAMMA>"] = " γ",
        };

        public struct EquipmentInfo
        {
            public int Id { get; }
            public EquipmentType EquipmentType { get; }
            public string Name { get; }

            public EquipmentInfo(int id, EquipmentType equipmentType, string name)
            {
                Id = id;
                EquipmentType = equipmentType;
                Name = name;
            }
        }

        public void Read()
        {
            equipments.Clear();

            var header = TextMasterDataHeader.Read(reader);

            var infoEntries = new TextMasterDataInfoEntry[header.KeyCount];

            for (int i = 0; i < infoEntries.Length; i++)
                infoEntries[i] = TextMasterDataInfoEntry.Read(reader);

            reader.BaseStream.Seek(2048, SeekOrigin.Current); // Skip unknown

            byte[] keyBlock = reader.ReadBytes((int)header.KeyBlockSize);
            byte[] valueBlock = reader.ReadBytes((int)header.ValueBlockSize);

            int keyOffset = 0;
            int valueOffset = 0;

            for (int i = 0; i < infoEntries.Length; i++)
            {
                string key = GetNext(keyBlock, ref keyOffset);
                string value = GetNext(valueBlock, ref valueOffset);

                if (key.EndsWith("_EXP"))
                    continue;

                if (Array.IndexOf(SkipValues, value) > -1)
                    continue;

                if (key.StartsWith(EquipmentStartPattern) && key.EndsWith(EquipmentEndPattern))
                {
                    int index = Array.FindIndex(ArmorPieceType, s => string.Compare(key, EquipmentStartPattern.Length, s, 0, s.Length) == 0);
                    if (index > -1)
                    {
                        int start = EquipmentStartPattern.Length + ArmorPieceType[index].Length;
                        int length = key.Length - start - EquipmentEndPattern.Length;
                        if (int.TryParse(key.AsSpan(start, length), out int id))
                        {
                            foreach (KeyValuePair<string, string> kv in ReplacementValues)
                            {
                                if (value.Contains(kv.Key))
                                    value = value.Replace(kv.Key, kv.Value);
                            }

                            equipments.Add(new EquipmentInfo(id, (EquipmentType)(index + 1), value));
                        }
                        else
                            logger?.LogError($"Could not determine identifier from key '{key}' (value: '{value}')");
                    }
                    else
                        logger?.LogError($"Could not determine equipment type from key '{key}' (value: '{value}')");
                }
                else
                    logger?.LogError($"Could not determine any information from key '{key}' (value: '{value}')");
            }
        }

        private string GetNext(byte[] data, ref int offset)
        {
            if (offset >= data.Length)
                return null;

            int index = Array.IndexOf<byte>(data, 0, offset);

            if (index < 0)
                index = data.Length;

            string result = Encoding.UTF8.GetString(data, offset, index - offset);

            offset = index + 1;

            return result;
        }
    }
}
