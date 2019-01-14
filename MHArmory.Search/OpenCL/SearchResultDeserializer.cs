using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MHArmory.Core.DataStructures;
using MHArmory.Search.Contracts;

namespace MHArmory.Search.OpenCL
{
    internal class SearchResultDeserializer
    {
        public List<ArmorSetSearchResult> Deserialize(ISolverData data, SearchIDMaps maps, SerializedSearchResults serializedResults)
        {
            var equipmentDict = data.AllHeads
                .Concat(data.AllChests)
                .Concat(data.AllGloves)
                .Concat(data.AllWaists)
                .Concat(data.AllLegs)
                .Concat(data.AllCharms)
                .Select(x => x.Equipment)
                .ToDictionary(x => (x.Id, x.Type));
            var decoDict = data.AllJewels.ToDictionary(x => x.Jewel.Id);

            var results = new List<ArmorSetSearchResult>();

            var ms = new MemoryStream(serializedResults.Data);
            var reader = new BinaryReader(ms);

            for (int i = 0; i < serializedResults.ResultCount; i++)
            {
                var result = new ArmorSetSearchResult();
                result.ArmorPieces = new List<IArmorPiece>();
                result.Jewels = new List<ArmorSetJewelResult>();
                result.IsMatch = true;
                const int equipmentTypesWithoutCharm = SearchLimits.EquipmentTypes - 1;
                for (int j = 0; j < equipmentTypesWithoutCharm; j++)
                {
                    ushort id = reader.ReadUInt16();
                    var armor = (IArmorPiece)equipmentDict[(id, (EquipmentType)j + 1)];
                    result.ArmorPieces.Add(armor);
                }

                ushort charmId = reader.ReadUInt16();
                result.Charm = (ICharmLevel)equipmentDict[(charmId, EquipmentType.Charm)];

                byte decorationCount = reader.ReadByte();
                for (int j = 0; j < decorationCount; j++)
                {
                    ushort id = reader.ReadUInt16();
                    byte count = reader.ReadByte();
                    IJewel jewel = decoDict[id].Jewel;
                    var jewelResult = new ArmorSetJewelResult
                    {
                        Jewel = jewel,
                        Count = count
                    };
                    result.Jewels.Add(jewelResult);
                }

                int remainingDecos = LengthConstants.DecorationsPerResult - decorationCount;
                reader.ReadBytes(remainingDecos * LengthConstants.DecorationLength);
                reader.ReadByte(); // Read dummy byte for 0-sized slots
                result.SpareSlots = reader.ReadBytes(SearchLimits.MaxJewelSize).Select(b => (int)b).ToArray();

                results.Add(result);
            }
            return results;
        }
    }
}
