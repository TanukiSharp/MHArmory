using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MHArmory.Core.DataStructures;
using MHArmory.Search.Contracts;

namespace MHArmory.Search.OpenCL
{
    internal class SearchDataSerializer
    {
        public SerializedSearchParameters Serialize(ISolverData data)
        {
            var parameters = new SerializedSearchParameters();
            
            var heads = data.AllHeads.Where(x => x.IsSelected).Select(x => x.Equipment).ToList();
            var chests = data.AllChests.Where(x => x.IsSelected).Select(x => x.Equipment).ToList();
            var gloves = data.AllGloves.Where(x => x.IsSelected).Select(x => x.Equipment).ToList();
            var waists = data.AllWaists.Where(x => x.IsSelected).Select(x => x.Equipment).ToList();
            var legs = data.AllLegs.Where(x => x.IsSelected).Select(x => x.Equipment).ToList();
            var charms = data.AllCharms.Where(x => x.IsSelected).Select(x => x.Equipment).ToList();

            var allArmorPieces = heads
                .Concat(chests)
                .Concat(gloves)
                .Concat(waists)
                .Concat(legs)
                .Cast<IArmorPiece>()
                .ToList();

            var allEquipment = allArmorPieces.Concat(charms).ToList();

            var armorSetParts = allArmorPieces
                .Where(x => x.ArmorSetSkills != null)
                .SelectMany(x => x.ArmorSetSkills.SelectMany(y => y.Parts))
                .ToList();

            var desiredAbilitiesSkillSet = new HashSet<int>(data.DesiredAbilities.Select(x => x.Skill.Id));
            IEnumerable<IArmorSetSkillPart> desiredArmorSetParts = armorSetParts.Where(x => x.GrantedSkills.Any(y => desiredAbilitiesSkillSet.Contains(y.Skill.Id)));
            IEnumerable<int> desiredSkillIDs = data.DesiredAbilities.Select(x => x.Skill.Id);

            var maps = new SearchIDMaps();

            maps.EquipmentIdMap = CreateIDMap(allEquipment.Select(x => (x.Id, x.Type)));
            if (maps.EquipmentIdMap.Count > SearchLimits.TotalEquipments)
            {
                throw new OpenCLSerializationException($"Tried to serialize {maps.EquipmentIdMap.Count} equipment pieces, maximum allowed is {SearchLimits.TotalEquipments}");
            }

            maps.JewelIdMap = CreateIDMap(data.AllJewels.Select(x => x.Jewel.Id));
            if (maps.EquipmentIdMap.Count > SearchLimits.TotalJewels)
            {
                throw new OpenCLSerializationException($"Tried to serialize {maps.JewelIdMap.Count} distinct jewels, maximum allowed is {SearchLimits.TotalJewels}");
            }

            maps.SetIdMap = CreateIDMap(desiredArmorSetParts.Select(x => x.Id));
            if (maps.SetIdMap.Count > SearchLimits.MaxSetSkills)
            {
                throw new OpenCLSerializationException($"Tried to serialize {maps.SetIdMap.Count} distinct set parts, maximum allowed is {SearchLimits.MaxSetSkills}");
            }

            maps.SkillIdMap = CreateIDMap(desiredSkillIDs);
            if (maps.SkillIdMap.Count > SearchLimits.MaxDesiredSkills)
            {
                throw new OpenCLSerializationException($"Tried to serialize {maps.SkillIdMap.Count} distinct skills, maximum allowed is {SearchLimits.MaxDesiredSkills}");
            }

            var headerStream = new MemoryStream();
            var headerWriter = new BinaryWriter(headerStream);
            SerializeWeaponSlots(data.WeaponSlots, headerWriter);
            headerWriter.Write((byte)heads.Count);
            headerWriter.Write((byte)chests.Count);
            headerWriter.Write((byte)gloves.Count);
            headerWriter.Write((byte)waists.Count);
            headerWriter.Write((byte)legs.Count);
            headerWriter.Write((byte)charms.Count);
            headerWriter.Write((byte)data.AllJewels.Length);
            headerWriter.Write((byte)data.DesiredAbilities.Length);
            headerWriter.Write((byte)maps.SetIdMap.Count);
            parameters.Header = headerStream.ToArray();

            parameters.Equipment = SerializeEquipment(maps, allEquipment);
            parameters.DesiredSkills = SerializeDesiredSkills(maps, data.DesiredAbilities);
            parameters.Decorations = SerializeDecorations(maps, data.AllJewels);
            parameters.Combinations = (uint) (heads.Count * chests.Count * gloves.Count * waists.Count * legs.Count * charms.Count);
            parameters.SearchIDMaps = maps;
            return parameters;
        }

        private IDictionary<T, byte> CreateIDMap<T>(IEnumerable<T> keys)
        {
            byte currentIndex = 0;
            var map = new Dictionary<T, byte>();
            foreach (T key in keys)
            {
                if (!map.ContainsKey(key))
                {
                    map.Add(key, currentIndex);
                    currentIndex++;
                }
            }
            return map;
        }

        private void SerializeWeaponSlots(int[] weaponSlots, BinaryWriter writer)
        {
            byte[] slots = new byte[LengthConstants.SlotArrayLength];
            foreach (int weaponSlot in weaponSlots)
            {
                slots[weaponSlot]++;
            }
            writer.Write(slots);
        }

        private byte[] SerializeEquipment(SearchIDMaps maps, IEnumerable<IEquipment> equipments)
        {
            var ms = new MemoryStream();
            var writer = new BinaryWriter(ms);
            foreach (IEquipment equipment in equipments)
            {
                byte mappedID = maps.EquipmentIdMap[(equipment.Id, equipment.Type)];
                writer.Write((ushort)mappedID);
                writer.Write((ushort)equipment.Id);

                SerializeEquipmentAbilities(maps, equipment, writer);
                if (equipment is IArmorPiece armorPiece)
                {
                    SerializeArmorSetSkills(maps, armorPiece, writer);
                }
                else
                {
                    for (int i = 0; i < SearchLimits.SetSkillsPerEquipment; i++)
                    {
                        SerializeNullSetSkill(writer);
                    }
                }
                byte[] slots = new byte[LengthConstants.SlotArrayLength];
                foreach (int slot in equipment.Slots)
                {
                    slots[slot]++;
                }
                writer.Write(slots);
            }
            return ms.ToArray();
        }

        private void SerializeEquipmentAbilities(SearchIDMaps maps, IEquipment equipment, BinaryWriter writer)
        {
            IAbility[] abilities = equipment.Abilities ?? new IAbility[0];
            if (abilities.Length > SearchLimits.SkillsPerEquipment)
            {
                throw new OpenCLSerializationException($"Equipment {equipment.Name} has {abilities.Length} abilities");
            }
            foreach (IAbility ability in abilities)
            {
                SerializeAbility(maps, ability, writer);
            }
            for (int i = abilities.Length; i < SearchLimits.SkillsPerEquipment; i++)
            {
                SerializeNullAbility(writer);
            }
        }

        private void SerializeArmorSetSkills(SearchIDMaps maps, IArmorPiece armorPiece, BinaryWriter writer)
        {
            IArmorSetSkillPart[] parts = armorPiece.ArmorSetSkills?.SelectMany(x => x.Parts).ToArray() ?? new IArmorSetSkillPart[0];
            int skillCount = parts.SelectMany(x => x.GrantedSkills).Count(x => maps.SkillIdMap.ContainsKey(x.Skill.Id));
            if (skillCount > SearchLimits.SetSkillsPerEquipment)
            {
                throw new OpenCLSerializationException($"Equipment {armorPiece.Name} has {skillCount} set abilities");
            }
            foreach (IArmorSetSkillPart part in parts)
            {
                SerializeSetSkill(maps, part, writer);
            }
            for (int i = skillCount; i < SearchLimits.SetSkillsPerEquipment; i++)
            {
                SerializeNullSetSkill(writer);
            }
        }

        private void SerializeSetSkill(SearchIDMaps maps, IArmorSetSkillPart part, BinaryWriter writer)
        {
            bool mapped = maps.SetIdMap.TryGetValue(part.Id, out byte mappedId);
            if (!mapped)
            {
                return;
            }
            IEnumerable<IAbility> desiredSkills = part.GrantedSkills.Where(x => maps.SkillIdMap.ContainsKey(x.Skill.Id));
            foreach (IAbility ability in desiredSkills)
            {
                writer.Write((byte)mappedId);
                writer.Write((byte)part.RequiredArmorPieces);
                SerializeAbility(maps, ability, writer);
            }
        }

        private void SerializeNullSetSkill(BinaryWriter writer)
        {
            writer.Write((byte)0xFF);
            writer.Write((byte)0);
            SerializeNullAbility(writer);
        }

        private void SerializeAbility(SearchIDMaps maps, IAbility ability, BinaryWriter writer)
        {
            bool mapped = maps.SkillIdMap.TryGetValue(ability.Skill.Id, out byte mappedId);
            if (mapped)
            {
                writer.Write((byte)mappedId);
                writer.Write((byte)ability.Level);
            }
            else
            {
                SerializeNullAbility(writer);
            }
        }

        private void SerializeNullAbility(BinaryWriter writer)
        {
            writer.Write((byte)0xFF);
            writer.Write((byte)0);
        }

        private byte[] SerializeDecorations(SearchIDMaps maps, SolverDataJewelModel[] jewels)
        {
            var ms = new MemoryStream();
            var writer = new BinaryWriter(ms);
            IOrderedEnumerable<SolverDataJewelModel> orderedJewels = jewels.OrderByDescending(x => x.Jewel.SlotSize);
            foreach (SolverDataJewelModel jewel in orderedJewels)
            {
                sbyte available = (sbyte)Math.Min(jewel.Available, sbyte.MaxValue);
                byte mappedId = maps.JewelIdMap[jewel.Jewel.Id];
                IAbility ability = jewel.Jewel.Abilities[0]; // Fuck it lol, too complex otherwise.
                writer.Write((byte)mappedId);
                writer.Write((ushort)jewel.Jewel.Id);
                writer.Write((byte)jewel.Jewel.SlotSize);
                writer.Write(available);
                SerializeAbility(maps, ability, writer);
            }
            byte[] result = ms.ToArray();
            return result;
        }

        private byte[] SerializeDesiredSkills(SearchIDMaps maps, IAbility[] desiredAbilities)
        {
            var ms = new MemoryStream();
            var writer = new BinaryWriter(ms);
            foreach (IAbility desiredAbility in desiredAbilities)
            {
                SerializeAbility(maps, desiredAbility, writer);
            }
            byte[] result = ms.ToArray();
            return result;
        }
    }
}
