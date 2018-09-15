using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MHArmory.Core;
using MHArmory.Core.DataStructures;
using MHArmory.ArmoryDataSource.DataStructures;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DataSourcesDiffTool
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().Run().Wait();
        }

        private async Task Run()
        {
            ILogger logger = new ConsoleLogger();

            IDataSource source = new MHArmory.AthenaAssDataSource.DataSource(logger, null, null);

            IArmorPiece[] armorPieces = await source.GetArmorPieces();
            IAbility[] abilities = await source.GetAbilities();
            ISkill[] skills = await source.GetSkills();
            ICharm[] charms = await source.GetCharms();
            IJewel[] jewels = await source.GetJewels();

            string outputPath = Path.Combine(AppContext.BaseDirectory, "data");

            if (Directory.Exists(outputPath) == false)
                Directory.CreateDirectory(outputPath);

            //--------------------------------------------------------------------------

            Write(Path.Combine(outputPath, $"{nameof(abilities)}.json"), Export(abilities));

            //--------------------------------------------------------------------------

            IEnumerable<IArmorPiece> heads = armorPieces.Where(x => x.Type == EquipmentType.Head);
            IEnumerable<IArmorPiece> chests = armorPieces.Where(x => x.Type == EquipmentType.Chest);
            IEnumerable<IArmorPiece> arms = armorPieces.Where(x => x.Type == EquipmentType.Gloves);
            IEnumerable<IArmorPiece> waists = armorPieces.Where(x => x.Type == EquipmentType.Waist);
            IEnumerable<IArmorPiece> legs = armorPieces.Where(x => x.Type == EquipmentType.Legs);

            Write(Path.Combine(outputPath, $"{nameof(heads)}.json"), Export(heads));
            Write(Path.Combine(outputPath, $"{nameof(chests)}.json"), Export(chests));
            Write(Path.Combine(outputPath, $"{nameof(arms)}.json"), Export(arms));
            Write(Path.Combine(outputPath, $"{nameof(waists)}.json"), Export(waists));
            Write(Path.Combine(outputPath, $"{nameof(legs)}.json"), Export(legs));

            //--------------------------------------------------------------------------

            IList<IArmorSetSkill> armorSetSkills = armorPieces
                .SelectMany(x => x.ArmorSetSkills)
                .Where(x => x != null)
                .Distinct(new LambdaEqualityComparer<IArmorSetSkill>((x, y) => x.Id == y.Id, x => x.Id))
                .ToList();
            Write(Path.Combine(outputPath, $"{nameof(armorSetSkills)}.json"), Export(armorSetSkills));

            //--------------------------------------------------------------------------

            IList<ICharmLevel> charmLevels = charms
                .SelectMany(x => x.Levels)
                .Distinct(new LambdaEqualityComparer<ICharmLevel>((x, y) => x.Id == y.Id, x => x.Id))
                .ToList();
            Write(Path.Combine(outputPath, $"{nameof(charmLevels)}.json"), Export(charmLevels));

            //--------------------------------------------------------------------------

            Write(Path.Combine(outputPath, $"{nameof(charms)}.json"), Export(charms));

            //--------------------------------------------------------------------------

            IList<IEvent> events = armorPieces
                .Select(x => x.Event)
                .Where(x => x != null)
                .Distinct(new LambdaEqualityComparer<IEvent>((x, y) => x.Id == y.Id, x => x.Id))
                .ToList();
            Write(Path.Combine(outputPath, $"{nameof(events)}.json"), Export(events));

            //--------------------------------------------------------------------------

            Write(Path.Combine(outputPath, $"{nameof(skills)}.json"), Export(skills));
        }

        private void Write(string filename, string content)
        {
            File.WriteAllText(filename, content);
        }

        private string Export(IEnumerable<IAbility> abilities)
        {
            IEnumerable<AbilityPrimitive> result = abilities.Select((x, i) => new AbilityPrimitive
            {
                Id = i,
                Level = x.Level,
                Description = x.Description
            });

            return JsonConvert.SerializeObject(result);
        }

        private string Export(IEnumerable<IArmorPiece> armorPieces)
        {
            IEnumerable<ArmorPiecePrimitive> result = armorPieces.Select(x => new ArmorPiecePrimitive
            {
                Id = x.Id,
                Name = x.Name,
                Rarity = x.Rarity,
                Slots = x.Slots,
                AbilityIds = x.Abilities.Select(a => a.Id).ToList(),
                Attributes = new ArmorAttributesPrimitive
                {
                    Gender = x.Attributes.RequiredGender
                },
                Defense = new ArmorDefensePrimitive
                {
                    Base = x.Defense.Base,
                    Max = x.Defense.Max,
                    Augmented = x.Defense.Augmented
                },
                Resistances = new ArmorResistancesPrimitive
                {
                    Fire = x.Resistances.Fire,
                    Water = x.Resistances.Water,
                    Thunder = x.Resistances.Thunder,
                    Ice = x.Resistances.Ice,
                    Dragon = x.Resistances.Dragon
                },
                ArmorSetSkillIds = x.ArmorSetSkills?.Select(s => s.Id).ToList(),
                FullArmorSetId = x.FullArmorSet?.Id,
                EventId = x.Event?.Id
            });

            return JsonConvert.SerializeObject(result);
        }

        private string Export(IEnumerable<IArmorSetSkill> armorSetSkills)
        {
            IEnumerable<ArmorSetSkillPrimitive> result = armorSetSkills.Select(x => new ArmorSetSkillPrimitive
            {
                Id = x.Id,
                Name = x.Name,
                Parts = x.Parts.Select(p => new ArmorSetSkillPartPrimitive
                {
                    RequiredArmorPieceCount = p.RequiredArmorPieces,
                    GrantedSkills = p.GrantedSkills.Select(a => a.Id).ToList()
                }).ToList(),
            });

            return JsonConvert.SerializeObject(result);
        }

        private string Export(IEnumerable<ICharmLevel> charmLevels)
        {
            IEnumerable<CharmLevelPrimitive> result = charmLevels.Select(x => new CharmLevelPrimitive
            {
                Id = x.Id,
                Name = x.Name,
                Level = x.Level,
                Rarity = x.Rarity,
                AbilityIds = x.Abilities.Select(a => a.Id).ToList(),
                Slots = x.Slots?.ToList(),
                EventId = x.Event?.Id
            });

            return JsonConvert.SerializeObject(result);
        }

        private string Export(IEnumerable<ICharm> charms)
        {
            IEnumerable<CharmPrimitive> result = charms.Select(x => new CharmPrimitive
            {
                Id = x.Id,
                Name = x.Name,
                LevelIds = x.Levels.Select(c => c.Id).ToList()
            });

            return JsonConvert.SerializeObject(result);
        }

        private string Export(IEnumerable<IEvent> events)
        {
            IEnumerable<EventPrimitive> result = events.Select(x => new EventPrimitive
            {
                Id = x.Id,
                Name = x.Name
            });

            return JsonConvert.SerializeObject(result);
        }

        private string Export(IEnumerable<ISkill> skills)
        {
            IEnumerable<SkillPrimitive> result = skills.Select(x => new SkillPrimitive
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                AbilityIds = x.Abilities.Select(a => a.Id).ToList()
            });

            return JsonConvert.SerializeObject(result);
        }
    }

    public class LambdaEqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> equals;
        private readonly Func<T, int> getHashCode;

        public LambdaEqualityComparer(Func<T, T, bool> equals)
            : this(equals, _ => 0)
        {
        }

        public LambdaEqualityComparer(Func<T, T, bool> equals, Func<T, int> getHashCode)
        {
            this.equals = equals;
            this.getHashCode = getHashCode;
        }

        public bool Equals(T x, T y)
        {
            return equals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return getHashCode(obj);
        }
    }

    public class ConsoleLogger : ILogger
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Console.WriteLine($"[{logLevel}] {formatter(state, exception)}");
        }
    }
}
