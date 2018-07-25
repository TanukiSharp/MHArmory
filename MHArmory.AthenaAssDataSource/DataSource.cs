using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MHArmory.AthenaAssDataSource.DataStructures;
using MHArmory.Core;
using MHArmory.Core.DataStructures;

namespace MHArmory.AthenaAssDataSource
{
    public class DataSource : IArmorDataSource, ISkillDataSource, ICharmDataSource, IJewelDataSource
    {
        private readonly string dataFolderPath;

        private readonly string skillsFilePath;
        private readonly string charmsFilePath;
        private readonly string jewelsFilePath;

        public const string HeadsFilename = "head.txt";
        public const string ChestsFilename = "body.txt";
        public const string GlovesFilename = "arms.txt";
        public const string WaistsFilename = "waist.txt";
        public const string LegsFilename = "legs.txt";
        public const string SkillsFilename = "skills.txt";
        public const string CharmsFilename = "charms.txt";
        public const string JewelsFilename = "decorations.txt";

        public DataSource()
        {
            string filename = Path.Combine(AppContext.BaseDirectory, "ass_path.txt");
            if (File.Exists(filename) == false)
            {
                File.Create(filename);
                throw new FormatException($"Store location of ASS data folder in file '{filename}'");
            }

            dataFolderPath = File.ReadAllText(filename).Trim();

            if (Directory.Exists(dataFolderPath) == false)
                throw new FormatException($"Directory '{dataFolderPath}' not found");

            skillsFilePath = Path.Combine(dataFolderPath, SkillsFilename);
            charmsFilePath = Path.Combine(dataFolderPath, CharmsFilename);
            jewelsFilePath = Path.Combine(dataFolderPath, JewelsFilename);

            LoadData();
        }

        private void LoadData()
        {
            LoadArmorSetSkillsPhase1();
            LoadSkills();
            LoadJewels();
            LoadCharms();
            LoadArmorPieces();
            LoadArmorSetSkillsPhase2();
        }

        private IList<string[]> eventArmors;
        private IList<ArmorSetSkillPrimitive> armorSetSkillPrimitives;

        private void LoadJewels()
        {
            string[] allLines = File.ReadAllLines(jewelsFilePath);

            (int headerIndex, int dataIndex) = FindIndexes(allLines);
            if (headerIndex < 0)
            {
                Console.WriteLine($"[ERROR] Failed to create data loader for decorations");
                jewels = Task.FromResult<IJewel[]>(null);
                return;
            }

            var localJewels = new List<IJewel>();
            var dataLoader = new DataLoader<JewelPrimitive>(allLines[headerIndex].Substring(1).Split(','));

            for (int i = dataIndex; i < allLines.Length; i++)
            {
                JewelPrimitive jewelPrimitive = dataLoader.CreateObject(allLines[i].Split(','), i + 1);

                if (string.IsNullOrWhiteSpace(jewelPrimitive.Name))
                    continue;

                IAbility ability = nonTaskAbilities.FirstOrDefault(a => a.Skill.Name == jewelPrimitive.Skill && a.Level == 1);

                if (ability == null)
                    throw new FormatException($"Cannot find skill '{jewelPrimitive.Name}'");

                 localJewels.Add(new Jewel(jewelPrimitive.Name, jewelPrimitive.Rarity, jewelPrimitive.SlotSize, new IAbility[] { ability }));
            }

            jewels = Task.FromResult(localJewels.ToArray());
        }

        private void LoadCharms()
        {
            string[] allLines = File.ReadAllLines(charmsFilePath);

            (int headerIndex, int dataIndex) = FindIndexes(allLines);
            if (headerIndex < 0)
            {
                Console.WriteLine($"[ERROR] Failed to create data loader for charms");
                charms = Task.FromResult<ICharm[]>(null);
                return;
            }

            var dataLoader = new DataLoader<CharmPrimitive>(allLines[headerIndex].Substring(1).Split(','));

            var localCharms = new Dictionary<string, List<ICharmLevel>>();
            var noSlots = new int[0];

            for (int i = dataIndex; i < allLines.Length; i++)
            {
                CharmPrimitive charmPrimitive = dataLoader.CreateObject(allLines[i].Split(','), i + 1);

                if (string.IsNullOrWhiteSpace(charmPrimitive.Name))
                    continue;

                (string charmName, int level) = DetermineCharmNameAndLevel(charmPrimitive.Name);

                if (localCharms.TryGetValue(charmName, out List<ICharmLevel> charmLevels) == false)
                {
                    charmLevels = new List<ICharmLevel>();
                    localCharms.Add(charmName, charmLevels);
                }

                charmLevels.Add(new CharmLevel(level, charmPrimitive.Name, 0, noSlots, ParseAbilities(charmPrimitive)));
            }

            charms = Task.FromResult(localCharms
                .Select(kv => new Charm(kv.Key, kv.Value.ToArray()))
                .Cast<ICharm>()
                .ToArray()
            );
        }

        private (string charmName, int level) DetermineCharmNameAndLevel(string name)
        {
            int index = name.LastIndexOf(' ');

            if (index < 0)
                return (name, 1);

            switch (name.Substring(index + 1))
            {
                case "III": return (name.Substring(0, index), 3);
                case "II": return (name.Substring(0, index), 2);
                case "I": return (name.Substring(0, index), 1);
            }

            return (name, 1);
        }

        private void LoadArmorSetSkillsPhase1()
        {
            string filename = Path.Combine(dataFolderPath, "set_skills.txt");
            armorSetSkillPrimitives = LoadArmorSetSkills(filename).ToList();
        }

        private void LoadArmorSetSkillsPhase2()
        {
            var armorSets = new List<IArmorSet>();

            foreach (IGrouping<string, ArmorSetSkillPrimitive> armorSetSkillGroup in armorSetSkillPrimitives.GroupBy(x => x.Name))
            {
                IArmorSetSkill[] armorSetSkills = armorSetSkillGroup
                    .Select(FromArmorSetSkillPrimitive)
                    .ToArray();

                IArmorPiece[] armorSetPieces = armorPieces.Result
                    .Where(x => x.Abilities.Any(a => a.Skill.Name == armorSetSkillGroup.Key))
                    .ToArray();

                var armorSet = new ArmorSet(false, armorSetPieces, armorSetSkills);

                armorSets.Add(armorSet);
            }

            // TODO: do something with armorSets
        }

        private IArmorSetSkill FromArmorSetSkillPrimitive(ArmorSetSkillPrimitive primitive)
        {
            ISkill skillGranted = nonTaskSkills.First(x => x.Name == primitive.SkillGranted);
            return new ArmorSetSkill(primitive.PiecesNeeded, skillGranted.Abilities.Where(a => a.Level == 1).ToArray());
        }

        private void LoadSkills()
        {
            string[] allLines = File.ReadAllLines(skillsFilePath);

            (int headerIndex, int dataIndex) = FindIndexes(allLines);
            if (headerIndex < 0)
            {
                Console.WriteLine($"[ERROR] Failed to create data loader for skills");
                nonTaskSkills = null;
                nonTaskAbilities = null;
                return;
            }

            var dataLoader = new DataLoader<SkillPrimitive>(allLines[headerIndex].Substring(1).Split(','));

            int id = 1;
            var localSkills = new List<ISkill>();

            for (int i = dataIndex; i < allLines.Length; i++)
            {
                SkillPrimitive skillPrimitive = dataLoader.CreateObject(allLines[i].Split(','), i + 1);

                if (string.IsNullOrWhiteSpace(skillPrimitive.Name))
                    continue;

                localSkills.Add(new DataStructures.Skill(id++, skillPrimitive));
            }

            foreach (ArmorSetSkillPrimitive armorSetSkillPrimitive in armorSetSkillPrimitives)
            {
                localSkills.Add(new DataStructures.Skill(id++, armorSetSkillPrimitive));
            }

            nonTaskAbilities = localSkills
                .SelectMany(s => s.Abilities)
                .Distinct()
                .ToArray();

            nonTaskSkills = localSkills.ToArray();
        }

        private void LoadArmorPieces()
        {
            string headsFilePath = Path.Combine(dataFolderPath, HeadsFilename);
            string chestsFilePath = Path.Combine(dataFolderPath, ChestsFilename);
            string glovesFilePath = Path.Combine(dataFolderPath, GlovesFilename);
            string waistsFilePath = Path.Combine(dataFolderPath, WaistsFilename);
            string legsFilePath = Path.Combine(dataFolderPath, LegsFilename);

            try
            {
                eventArmors = File.ReadAllLines(Path.Combine(dataFolderPath, "events.txt"))
                    .Select(x => x.Split(','))
                    .ToList();

                var heads = LoadArmorPieceParts(EquipmentType.Head, headsFilePath).ToList();
                var chests = LoadArmorPieceParts(EquipmentType.Chest, chestsFilePath).ToList();
                var gloves = LoadArmorPieceParts(EquipmentType.Gloves, glovesFilePath).ToList();
                var waists = LoadArmorPieceParts(EquipmentType.Waist, waistsFilePath).ToList();
                var legs = LoadArmorPieceParts(EquipmentType.Legs, legsFilePath).ToList();

                var allPrimitives = heads
                    .Concat(chests)
                    .Concat(gloves)
                    .Concat(waists)
                    .Concat(legs)
                    .ToList();

                PreUpdateArmorSets(allPrimitives);

                IList<ArmorPiece> nonTaskAllPieces = allPrimitives
                    .Select(x => MapArmorPiecePrimitive(x))
                    .ToList();

                PostUpdateArmorSets(allPrimitives, nonTaskAllPieces);

                armorPieces = Task.FromResult(nonTaskAllPieces.Cast<IArmorPiece>().ToArray());
            }
            catch (Exception ex)
            {
                armorPieces = Task.FromResult<IArmorPiece[]>(null);
                Console.WriteLine(ex.ToString());
            }
        }

        private (int headerIndex, int dataIndex) FindIndexes(string[] allLines)
        {
            for (int i = 0; i < allLines.Length; i++)
            {
                if (allLines[i].StartsWith("#"))
                {
                    for (int j = i + 1; j < allLines.Length; j++)
                    {
                        if (allLines[j].StartsWith("#") == false)
                            return (i, j);
                    }
                }
            }

            return (-1, -1);
        }

        private ArmorPiece MapArmorPiecePrimitive(ArmorPiecePrimitive primitive)
        {
            return new ArmorPiece(
                primitive.Id,
                primitive.Name,
                primitive.Type,
                primitive.Rarity,
                ParseSlots(primitive.Slots),
                ParseAbilities(primitive),
                new ArmorPieceDefense(primitive.MinDef, primitive.MaxDef, primitive.AugmentedDef),
                new ArmorPieceResistances(primitive.FireRes, primitive.WaterRes, primitive.ThunderRes, primitive.IceRes, primitive.DragonRes),
                CreateArmorPieceAttributes(primitive),
                ArmorPieceAssets.Null,
                null // TODO: update armor set
            );
        }

        private void PreUpdateArmorSets(IList<ArmorPiecePrimitive> allPrimitives)
        {
            var list = allPrimitives.Where(x => x.Restriction == "Full").ToList();

            foreach (ArmorPiecePrimitive piece in list)
            {
                if (piece.FullArmorSetIds != null)
                    continue;

                int armorSetIndex = -1;
                for (int i = 0; i < eventArmors.Count; i++)
                {
                    if (eventArmors[i].Contains(piece.Name))
                    {
                        armorSetIndex = i;
                        break;
                    }
                }

                if (armorSetIndex < 0)
                    continue;

                string[] setPieces = eventArmors[armorSetIndex];

                int[] armorPieceIds = allPrimitives
                    .Where(x => setPieces.Contains(x.Name))
                    .Select(x => x.Id)
                    .ToArray();

                if (armorPieceIds.Length != 5)
                    continue;

                foreach (ArmorPiecePrimitive setPiece in allPrimitives.Where(x => setPieces.Contains(x.Name)))
                    setPiece.FullArmorSetIds = armorPieceIds;
            }
        }

        private void PostUpdateArmorSets(IList<ArmorPiecePrimitive> allPrimitives, IList<ArmorPiece> allArmorPieces)
        {
            foreach (ArmorPiecePrimitive primitive in allPrimitives.Where(x => x.FullArmorSetIds != null))
            {
                if (primitive.FullArmorSet != null)
                    continue;

                IArmorPiece[] setPieces = primitive.FullArmorSetIds
                    .Select(id => allArmorPieces.First(p => p.Id == id))
                    .Cast<IArmorPiece>()
                    .ToArray();

                IEnumerable<ArmorPiecePrimitive> setPrimitives = primitive.FullArmorSetIds
                    .Select(id => allPrimitives.First(p => p.Id == id));

                IArmorSet armorSet = new ArmorSet(true, setPieces, null);

                foreach (ArmorPiecePrimitive setPrimitive in setPrimitives)
                    setPrimitive.FullArmorSet = armorSet;
            }
        }

        private IArmorPieceAttributes CreateArmorPieceAttributes(ArmorPiecePrimitive primitive)
        {
            switch (primitive.Restriction)
            {
                case "M": return new ArmorPieceAttributes(Gender.Male);
                case "F": return new ArmorPieceAttributes(Gender.Female);
            }
            return new ArmorPieceAttributes(Gender.Both);
        }

        private int[] ParseSlots(string slots)
        {
            if (slots == null || slots.Length == 0 || slots == "0")
                return new int[0];

            return slots.Select(c => c - '0').ToArray();
        }

        private IAbility[] ParseAbilities(ArmorPiecePrimitive primitive)
        {
            var result = new List<IAbility>(3);

            result.Add(ParseOneAbility(primitive.Skill1, primitive.PointSkill1));
            result.Add(ParseOneAbility(primitive.Skill2, primitive.PointSkill2));
            result.Add(ParseOneAbility(primitive.Skill3, primitive.PointSkill3));

            return result.Where(x => x != null).ToArray();
        }

        private IAbility[] ParseAbilities(CharmPrimitive primitive)
        {
            var result = new List<IAbility>(2);

            result.Add(ParseOneAbility(primitive.Skill1, primitive.Points1));
            result.Add(ParseOneAbility(primitive.Skill2, primitive.Points2));

            return result.Where(x => x != null).ToArray();
        }

        private IAbility ParseOneAbility(string name, int level)
        {
            if (string.IsNullOrEmpty(name) || level <= 0)
                return null;

            ISkill skill = nonTaskSkills.FirstOrDefault(s => s.Name == name);

            if (skill == null)
                return null;

            return skill.Abilities.FirstOrDefault(a => a.Level == level);
        }

        private int armorPieceId = 1;

        private IEnumerable<ArmorPiecePrimitive> LoadArmorPieceParts(EquipmentType type, string filename)
        {
            string[] allLines = File.ReadAllLines(filename);

            (int headerIndex, int dataIndex) = FindIndexes(allLines);
            if (headerIndex < 0)
            {
                Console.WriteLine($"[ERROR] Failed to create data loader for {type}");
                yield break;
            }

            var dataLoader = new DataLoader<ArmorPiecePrimitive>(allLines[headerIndex].Substring(1).Split(','));

            for (int i = dataIndex; i < allLines.Length; i++)
            {
                ArmorPiecePrimitive armorPiece = dataLoader.CreateObject(allLines[i].Split(','), i + 1);

                if (string.IsNullOrWhiteSpace(armorPiece.Name))
                    continue;

                armorPiece.Id = armorPieceId++;
                armorPiece.Type = type;

                yield return armorPiece;
            }
        }

        private IEnumerable<ArmorSetSkillPrimitive> LoadArmorSetSkills(string filename)
        {
            string[] allLines = File.ReadAllLines(filename);

            (int headerIndex, int dataIndex) = FindIndexes(allLines);
            if (headerIndex < 0)
            {
                Console.WriteLine($"[ERROR] Failed to create data loader for armor set skills");
                yield break;
            }

            var dataLoader = new DataLoader<ArmorSetSkillPrimitive>(allLines[headerIndex].Substring(1).Split(','));

            for (int i = dataIndex; i < allLines.Length; i++)
            {
                ArmorSetSkillPrimitive armorSetSkill = dataLoader.CreateObject(allLines[i].Split(','), i + 1);

                if (string.IsNullOrWhiteSpace(armorSetSkill.Name))
                    continue;

                armorSetSkill.Id = i + 1;

                yield return armorSetSkill;
            }
        }

        string IArmorDataSource.Description => $"Athena's ASS [path: '{dataFolderPath}', files: {HeadsFilename}, {ChestsFilename}, {GlovesFilename}, {WaistsFilename} or {LegsFilename}]";
        string ISkillDataSource.Description => $"Athena's ASS [{skillsFilePath}]";
        string ICharmDataSource.Description => $"Athena's ASS [{charmsFilePath}]";
        string IJewelDataSource.Description => $"Athena's ASS [{jewelsFilePath}]";

        private Task<IArmorPiece[]> armorPieces;
        private Task<ICharm[]> charms;
        private Task<IJewel[]> jewels;
        private ISkill[] nonTaskSkills;
        private IAbility[] nonTaskAbilities;

        public Task<IAbility[]> GetAbilities()
        {
            return Task.FromResult(nonTaskAbilities);
        }

        public Task<IArmorPiece[]> GetArmorPieces()
        {
            return armorPieces;
        }

        public Task<ICharm[]> GetCharms()
        {
            return charms;
        }

        public Task<IJewel[]> GetJewels()
        {
            return jewels;
        }

        public Task<ISkill[]> GetSkills()
        {
            return Task.FromResult(nonTaskSkills);
        }
    }
}
