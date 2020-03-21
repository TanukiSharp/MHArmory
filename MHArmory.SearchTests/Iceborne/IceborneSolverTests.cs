using Microsoft.VisualStudio.TestTools.UnitTesting;
using MHArmory.Search.Iceborne;
using MHArmory.Search.Default;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MHArmory.Core.DataStructures;
using MHArmory.Search.Contracts;

namespace MHArmory.Search.Iceborne.Tests
{
    [TestClass()]
    public class IceborneSolverTests
    {
        private readonly IList<IArmorPiece> heads;
        private readonly IList<IArmorPiece> chests;
        private readonly IList<IArmorPiece> gloves;
        private readonly IList<IArmorPiece> waists;
        private readonly IList<IArmorPiece> legs;
        private readonly ICharm[] charms;
        private readonly ISkill[] skills;
        private readonly IJewel[] decos;

        public IceborneSolverTests()
        {
            var source = new ArmoryDataSource.DataSource(null);
            Task<IArmorPiece[]> taskArmor = source.GetArmorPieces();
            Task<ICharm[]> taskCharms = source.GetCharms();
            Task<ISkill[]> taskSkills = source.GetSkills();
            Task<IJewel[]> taskDecos = source.GetJewels();
            IArmorPiece[] armorPieces = taskArmor.Result;
            heads = armorPieces.Where(x => x.Type == EquipmentType.Head).ToList();
            chests = armorPieces.Where(x => x.Type == EquipmentType.Chest).ToList();
            gloves = armorPieces.Where(x => x.Type == EquipmentType.Gloves).ToList();
            waists = armorPieces.Where(x => x.Type == EquipmentType.Waist).ToList();
            legs = armorPieces.Where(x => x.Type == EquipmentType.Legs).ToList();
            charms = taskCharms.Result;
            skills = taskSkills.Result;
            decos = taskDecos.Result;
        }

        private ISkill getSkillByName(string name)
        {
            ISkill skill = skills.FirstOrDefault(s => s.Name["EN"] == name);
            if (skill == null)
            {
                throw new ArgumentException($"No skill with name {name}");
            }
            return skill;
        }

        private void DeselectOther(ISolverDataEquipmentModel[] equipments, string name)
        {
            bool found = false;
            foreach(ISolverDataEquipmentModel piece in equipments)
            {
                if (piece.Equipment.Name["EN"] == name)
                {
                    found = true;
                    if (piece.IsSelected == false)
                        throw new InvalidOperationException($"Equipment with name {name} was not already selected");
                }
                else
                    piece.IsSelected = false;
            }
            if (!found)
                throw new ArgumentException($"Equipment with name {name} not found");
        }


        [TestMethod()]
        public async Task SearchTest_GS_AcidShredder()
        {
            var weapon = new Weapon(0, WeaponType.GreatSword, new int[] { 4 }, new IAbility[0], null);
            var solverData = new SolverData();
            var desiredAbilities = new IAbility[]
            {
                new Ability(getSkillByName("Heat Guard"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Health Boost"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Critical Eye"), 7, new Dictionary<string, string>()),
                new Ability(getSkillByName("Critical Boost"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Weakness Exploit"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Focus"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Handicraft"), 5, new Dictionary<string, string>()),
                new Ability(getSkillByName("Latent Power"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Agitator"), 4, new Dictionary<string, string>()),
                new Ability(getSkillByName("Non-elemental Boost"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Master's Touch"), 1, new Dictionary<string, string>()),
            };
            solverData.Setup(weapon, heads, chests, gloves, waists, legs, charms.SelectMany(c => c.Levels), decos.Select(d => new Contracts.SolverDataJewelModel(d, int.MaxValue)), desiredAbilities); ;
            DeselectOther(solverData.AllHeads, "Kaiser Crown β+");
            DeselectOther(solverData.AllChests, "Damascus Mail β+");
            DeselectOther(solverData.AllGloves, "Kaiser Vambraces β+");
            DeselectOther(solverData.AllWaists, "Kaiser Coil β+");
            DeselectOther(solverData.AllLegs, "Garuga Greaves β+");
            DeselectOther(solverData.AllCharms, "Handicraft Charm 4");
            var solver = new IceborneSolver();
            var cancellationToken = new System.Threading.CancellationToken();
            IList<ArmorSetSearchResult> armors = await solver.SearchArmorSets(solverData, cancellationToken);
            Assert.AreEqual(1, armors.Count);
        }

        [TestMethod()]
        public async Task SearchTest_GS_RoyalVenusBlade()
        {
            var weapon = new Weapon(0, WeaponType.GreatSword, new int[] { 2 }, new IAbility[0], null);
            var solverData = new SolverData();
            var desiredAbilities = new IAbility[]
            {
                new Ability(getSkillByName("Attack Boost"), 6, new Dictionary<string, string>()),
                new Ability(getSkillByName("Critical Eye"), 4, new Dictionary<string, string>()),
                new Ability(getSkillByName("Agitator"), 4, new Dictionary<string, string>()),
                new Ability(getSkillByName("Critical Boost"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Weakness Exploit"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Focus"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Health Boost"), 2, new Dictionary<string, string>()),
                new Ability(getSkillByName("Handicraft"), 2, new Dictionary<string, string>()),
                new Ability(getSkillByName("Heat Guard"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Latent Power"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Non-elemental Boost"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Master's Touch"), 1, new Dictionary<string, string>()),
            };
            solverData.Setup(weapon, heads, chests, gloves, waists, legs, charms.SelectMany(c => c.Levels), decos.Select(d => new Contracts.SolverDataJewelModel(d, int.MaxValue)), desiredAbilities); ;
            DeselectOther(solverData.AllHeads, "Kaiser Crown β+");
            DeselectOther(solverData.AllChests, "Damascus Mail β+");
            DeselectOther(solverData.AllGloves, "Kaiser Vambraces β+");
            DeselectOther(solverData.AllWaists, "Kaiser Coil β+");
            DeselectOther(solverData.AllLegs, "Garuga Greaves β+");
            DeselectOther(solverData.AllCharms, "Attack Charm 4");
            var solver = new IceborneSolver();
            var cancellationToken = new System.Threading.CancellationToken();
            IList<ArmorSetSearchResult> armors = await solver.SearchArmorSets(solverData, cancellationToken);
            Assert.AreEqual(1, armors.Count);
        }

        [TestMethod()]
        public async Task SearchTest_DB_AnjaTrueCriticalElement_SkillsRandom()
        {
            var weapon = new Weapon(0, WeaponType.DualBlades, new int[] { 2 }, new IAbility[0], null);
            var solverData = new SolverData();
            var desiredAbilities = new IAbility[]
            {
                new Ability(getSkillByName("Agitator"), 2, new Dictionary<string, string>()),
                new Ability(getSkillByName("Fire Attack"), 6, new Dictionary<string, string>()),
                new Ability(getSkillByName("Protective Polish"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Handicraft"), 2, new Dictionary<string, string>()),
                new Ability(getSkillByName("Critical Boost"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Weakness Exploit"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Slinger Capacity Secret"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Critical Eye"), 7, new Dictionary<string, string>()),
                new Ability(getSkillByName("True Critical Element"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Item Prolonger"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Slinger Capacity"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Windproof"), 3, new Dictionary<string, string>()),
            };
            solverData.Setup(weapon, heads, chests, gloves, waists, legs, charms.SelectMany(c => c.Levels), decos.Select(d => new Contracts.SolverDataJewelModel(d, int.MaxValue)), desiredAbilities); ;
            DeselectOther(solverData.AllHeads, "Silver Solhelm α+");
            DeselectOther(solverData.AllChests, "Silver Solmail α+");
            DeselectOther(solverData.AllGloves, "Silver Solbraces β+");
            DeselectOther(solverData.AllWaists, "Silver Solcoil β+");
            DeselectOther(solverData.AllLegs, "Garuga Greaves β+");
            DeselectOther(solverData.AllCharms, "Master's Charm 4");
            var solver = new IceborneSolver();
            var cancellationToken = new System.Threading.CancellationToken();
            IList<ArmorSetSearchResult> armors = await solver.SearchArmorSets(solverData, cancellationToken);
            Assert.AreEqual(1, armors.Count);
        }

        [TestMethod()]
        public async Task SearchTest_DB_AnjaTrueCriticalElement_SkillsSortedBySlotsize()
        {
            var weapon = new Weapon(0, WeaponType.DualBlades, new int[] {  }, new IAbility[0], null);
            var solverData = new SolverData();
            var desiredAbilities = new IAbility[]
            {
                new Ability(getSkillByName("Windproof"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Handicraft"), 2, new Dictionary<string, string>()),
                new Ability(getSkillByName("Critical Boost"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Agitator"), 2, new Dictionary<string, string>()),
                new Ability(getSkillByName("Protective Polish"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Weakness Exploit"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Fire Attack"), 6, new Dictionary<string, string>()),
                new Ability(getSkillByName("Slinger Capacity"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Critical Eye"), 7, new Dictionary<string, string>()),
                new Ability(getSkillByName("Item Prolonger"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("True Critical Element"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Slinger Capacity Secret"), 1, new Dictionary<string, string>()),
            };
            solverData.Setup(weapon, heads, chests, gloves, waists, legs, charms.SelectMany(c => c.Levels), decos.Select(d => new Contracts.SolverDataJewelModel(d, int.MaxValue)), desiredAbilities); ;
            DeselectOther(solverData.AllHeads, "Silver Solhelm α+");
            DeselectOther(solverData.AllChests, "Silver Solmail α+");
            DeselectOther(solverData.AllGloves, "Silver Solbraces β+");
            DeselectOther(solverData.AllWaists, "Silver Solcoil β+");
            DeselectOther(solverData.AllLegs, "Garuga Greaves β+");
            DeselectOther(solverData.AllCharms, "Master's Charm 4");
            var solver = new IceborneSolver();
            var cancellationToken = new System.Threading.CancellationToken();
            IList<ArmorSetSearchResult> armors = await solver.SearchArmorSets(solverData, cancellationToken);
            Assert.AreEqual(1, armors.Count);
        }

        [TestMethod()]
        public async Task SearchTest_DB_AnjaTrueCriticalElement_SkillsSortedByLevel()
        {
            var weapon = new Weapon(0, WeaponType.DualBlades, new int[] {  }, new IAbility[0], null);
            var solverData = new SolverData();
            var desiredAbilities = new IAbility[]
            {
                new Ability(getSkillByName("Critical Eye"), 7, new Dictionary<string, string>()),
                new Ability(getSkillByName("Fire Attack"), 6, new Dictionary<string, string>()),
                new Ability(getSkillByName("Windproof"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Critical Boost"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Weakness Exploit"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Item Prolonger"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Slinger Capacity"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Handicraft"), 2, new Dictionary<string, string>()),
                new Ability(getSkillByName("Agitator"), 2, new Dictionary<string, string>()),
                new Ability(getSkillByName("Protective Polish"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Slinger Capacity Secret"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("True Critical Element"), 1, new Dictionary<string, string>()),
            };
            solverData.Setup(weapon, heads, chests, gloves, waists, legs, charms.SelectMany(c => c.Levels), decos.Select(d => new Contracts.SolverDataJewelModel(d, int.MaxValue)), desiredAbilities);
            DeselectOther(solverData.AllHeads, "Silver Solhelm α+");
            DeselectOther(solverData.AllChests, "Silver Solmail α+");
            DeselectOther(solverData.AllGloves, "Silver Solbraces β+");
            DeselectOther(solverData.AllWaists, "Silver Solcoil β+");
            DeselectOther(solverData.AllLegs, "Garuga Greaves β+");
            DeselectOther(solverData.AllCharms, "Master's Charm 4");
            var solver = new IceborneSolver();
            var cancellationToken = new System.Threading.CancellationToken();
            IList<ArmorSetSearchResult> armors = await solver.SearchArmorSets(solverData, cancellationToken);
            Assert.AreEqual(1, armors.Count);
        }

        [TestMethod()]
        public async Task SearchTest_DB_AnjaMastersTouch()
        {
            var weapon = new Weapon(0, WeaponType.DualBlades, new int[] {  }, new IAbility[0], null);
            var solverData = new SolverData();
            var desiredAbilities = new IAbility[]
            {
                new Ability(getSkillByName("Critical Eye"), 7, new Dictionary<string, string>()),
                new Ability(getSkillByName("Fire Attack"), 6, new Dictionary<string, string>()),
                new Ability(getSkillByName("Critical Boost"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Weakness Exploit"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Handicraft"), 2, new Dictionary<string, string>()),
                new Ability(getSkillByName("Critical Element"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Divine Blessing"), 2, new Dictionary<string, string>()),
                new Ability(getSkillByName("Critical Draw"), 2, new Dictionary<string, string>()),
                new Ability(getSkillByName("Health Boost"), 2, new Dictionary<string, string>()),
                new Ability(getSkillByName("Latent Power"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Heat Guard"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Master's Touch"), 1, new Dictionary<string, string>()),

            };
            solverData.Setup(weapon, heads, chests, gloves, waists, legs, charms.SelectMany(c => c.Levels), decos.Select(d => new Contracts.SolverDataJewelModel(d, int.MaxValue)), desiredAbilities); ;
            DeselectOther(solverData.AllHeads, "Rimeguard Helm β+");
            DeselectOther(solverData.AllChests, "Kaiser Mail β+");
            DeselectOther(solverData.AllGloves, "Kaiser Vambraces β+");
            DeselectOther(solverData.AllWaists, "Kaiser Coil β+");
            DeselectOther(solverData.AllLegs, "Rimeguard Greaves β+");
            DeselectOther(solverData.AllCharms, "Blaze Charm 5");
            var solver = new IceborneSolver();
            var cancellationToken = new System.Threading.CancellationToken();
            IList<ArmorSetSearchResult> armors = await solver.SearchArmorSets(solverData, cancellationToken);
            Assert.AreEqual(1, armors.Count);
        }

        [TestMethod()]
        public async Task SearchTest_DB_LavaMastersTouch()
        {
            var weapon = new Weapon(0, WeaponType.DualBlades, new int[] { 4, 2 }, new IAbility[0], null);
            var solverData = new SolverData();
            var desiredAbilities = new IAbility[]
            {
                new Ability(getSkillByName("Critical Eye"), 5, new Dictionary<string, string>()),
                new Ability(getSkillByName("Fire Attack"), 6, new Dictionary<string, string>()),
                new Ability(getSkillByName("Critical Boost"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Weakness Exploit"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Handicraft"), 2, new Dictionary<string, string>()),
                new Ability(getSkillByName("Critical Element"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Divine Blessing"), 2, new Dictionary<string, string>()),
                new Ability(getSkillByName("Critical Draw"), 2, new Dictionary<string, string>()),
                new Ability(getSkillByName("Health Boost"), 2, new Dictionary<string, string>()),
                new Ability(getSkillByName("Latent Power"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Heat Guard"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Master's Touch"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Agitator"), 5, new Dictionary<string, string>()),

            };
            solverData.Setup(weapon, heads, chests, gloves, waists, legs, charms.SelectMany(c => c.Levels), decos.Select(d => new Contracts.SolverDataJewelModel(d, int.MaxValue)), desiredAbilities); ;
            DeselectOther(solverData.AllHeads, "Rimeguard Helm β+");
            DeselectOther(solverData.AllChests, "Kaiser Mail β+");
            DeselectOther(solverData.AllGloves, "Kaiser Vambraces β+");
            DeselectOther(solverData.AllWaists, "Kaiser Coil β+");
            DeselectOther(solverData.AllLegs, "Rimeguard Greaves β+");
            DeselectOther(solverData.AllCharms, "Blaze Charm 5");
            var solver = new IceborneSolver();
            var cancellationToken = new System.Threading.CancellationToken();
            IList<ArmorSetSearchResult> armors = await solver.SearchArmorSets(solverData, cancellationToken);
            Assert.AreEqual(1, armors.Count);
        }

        [TestMethod()]
        public async Task SearchTest_DB_RathMastersTouch()
        {
            var weapon = new Weapon(0, WeaponType.DualBlades, new int[] { 2 }, new IAbility[0], null);
            var solverData = new SolverData();
            var desiredAbilities = new IAbility[]
            {
                new Ability(getSkillByName("Critical Eye"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Fire Attack"), 6, new Dictionary<string, string>()),
                new Ability(getSkillByName("Critical Boost"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Weakness Exploit"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Handicraft"), 4, new Dictionary<string, string>()),
                new Ability(getSkillByName("Critical Element"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Divine Blessing"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Critical Draw"), 2, new Dictionary<string, string>()),
                new Ability(getSkillByName("Latent Power"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Heat Guard"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Master's Touch"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Agitator"), 4, new Dictionary<string, string>()),

            };
            solverData.Setup(weapon, heads, chests, gloves, waists, legs, charms.SelectMany(c => c.Levels), decos.Select(d => new Contracts.SolverDataJewelModel(d, int.MaxValue)), desiredAbilities);
            DeselectOther(solverData.AllHeads, "Rimeguard Helm β+");
            DeselectOther(solverData.AllChests, "Kaiser Mail β+");
            DeselectOther(solverData.AllGloves, "Kaiser Vambraces β+");
            DeselectOther(solverData.AllWaists, "Kaiser Coil β+");
            DeselectOther(solverData.AllLegs, "Rimeguard Greaves β+");
            DeselectOther(solverData.AllCharms, "Blaze Charm 5");
            var solver = new IceborneSolver();
            var cancellationToken = new System.Threading.CancellationToken();
            IList<ArmorSetSearchResult> armors = await solver.SearchArmorSets(solverData, cancellationToken);
            Assert.AreEqual(1, armors.Count);
        }


        [TestMethod()]
        public async Task SearchTest_DB_JyuraTrueCriticalElement()
        {
            var weapon = new Weapon(0, WeaponType.DualBlades, new int[] { 3, 1 }, new IAbility[0], null);
            var solverData = new SolverData();
            var desiredAbilities = new IAbility[]
            {
                new Ability(getSkillByName("Critical Eye"), 7, new Dictionary<string, string>()),
                new Ability(getSkillByName("Water Attack"), 6, new Dictionary<string, string>()),
                new Ability(getSkillByName("Critical Boost"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Weakness Exploit"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Handicraft"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("True Critical Element"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Agitator"), 4, new Dictionary<string, string>()),
                new Ability(getSkillByName("Item Prolonger"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Slinger Capacity"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Windproof"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Protective Polish"), 1, new Dictionary<string, string>()),

            };
            solverData.Setup(weapon, heads, chests, gloves, waists, legs, charms.SelectMany(c => c.Levels), decos.Select(d => new Contracts.SolverDataJewelModel(d, int.MaxValue)), desiredAbilities);
            DeselectOther(solverData.AllHeads, "Silver Solhelm β+");
            DeselectOther(solverData.AllChests, "Silver Solmail β+");
            DeselectOther(solverData.AllGloves, "Silver Solbraces β+");
            DeselectOther(solverData.AllWaists, "Silver Solcoil β+");
            DeselectOther(solverData.AllLegs, "Garuga Greaves β+");
            DeselectOther(solverData.AllCharms, "Flood Charm 5");
            var solver = new IceborneSolver();
            var cancellationToken = new System.Threading.CancellationToken();
            IList<ArmorSetSearchResult> armors = await solver.SearchArmorSets(solverData, cancellationToken);
            Assert.AreEqual(1, armors.Count);
        }

        [TestMethod()]
        public async Task SearchTest_DB_RoyalTrueCriticalElement()
        {
            var weapon = new Weapon(0, WeaponType.DualBlades, new int[] { 3 }, new IAbility[0], null);
            var solverData = new SolverData();
            var desiredAbilities = new IAbility[]
            {
                new Ability(getSkillByName("Critical Eye"), 5, new Dictionary<string, string>()),
                new Ability(getSkillByName("Water Attack"), 6, new Dictionary<string, string>()),
                new Ability(getSkillByName("Critical Boost"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Weakness Exploit"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Handicraft"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("True Critical Element"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Item Prolonger"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Slinger Capacity"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Windproof"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Protective Polish"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Free Elem/Ammo Up"), 3, new Dictionary<string, string>()),


            };
            solverData.Setup(weapon, heads, chests, gloves, waists, legs, charms.SelectMany(c => c.Levels), decos.Select(d => new Contracts.SolverDataJewelModel(d, int.MaxValue)), desiredAbilities);
            DeselectOther(solverData.AllHeads, "Silver Solhelm β+");
            DeselectOther(solverData.AllChests, "Silver Solmail β+");
            DeselectOther(solverData.AllGloves, "Silver Solbraces β+");
            DeselectOther(solverData.AllWaists, "Silver Solcoil β+");
            DeselectOther(solverData.AllLegs, "Garuga Greaves β+");
            DeselectOther(solverData.AllCharms, "Awakening Charm 3");
            var solver = new IceborneSolver();
            var cancellationToken = new System.Threading.CancellationToken();
            IList<ArmorSetSearchResult> armors = await solver.SearchArmorSets(solverData, cancellationToken);
            Assert.AreEqual(1, armors.Count);
        }

        [TestMethod()]
        public async Task SearchTest_DB_JyuraMastersTouch()
        {
            var weapon = new Weapon(0, WeaponType.DualBlades, new int[] { 2, 1 }, new IAbility[0], null);
            var solverData = new SolverData();
            var desiredAbilities = new IAbility[]
            {
                new Ability(getSkillByName("Critical Eye"), 6, new Dictionary<string, string>()),
                new Ability(getSkillByName("Water Attack"), 6, new Dictionary<string, string>()),
                new Ability(getSkillByName("Critical Boost"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Weakness Exploit"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Handicraft"), 2, new Dictionary<string, string>()),
                new Ability(getSkillByName("Latent Power"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Critical Element"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Agitator"), 4, new Dictionary<string, string>()),
                new Ability(getSkillByName("Divine Blessing"), 2, new Dictionary<string, string>()),
                new Ability(getSkillByName("Critical Draw"), 2, new Dictionary<string, string>()),
                new Ability(getSkillByName("Health Boost"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Heat Guard"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Master's Touch"), 1, new Dictionary<string, string>()),


            };
            solverData.Setup(weapon, heads, chests, gloves, waists, legs, charms.SelectMany(c => c.Levels), decos.Select(d => new Contracts.SolverDataJewelModel(d, int.MaxValue)), desiredAbilities);
            DeselectOther(solverData.AllHeads, "Rimeguard Helm β+");
            DeselectOther(solverData.AllChests, "Kaiser Mail β+");
            DeselectOther(solverData.AllGloves, "Kaiser Vambraces β+");
            DeselectOther(solverData.AllWaists, "Kaiser Coil β+");
            DeselectOther(solverData.AllLegs, "Rimeguard Greaves β+");
            DeselectOther(solverData.AllCharms, "Flood Charm 5");
            var solver = new IceborneSolver();
            var cancellationToken = new System.Threading.CancellationToken();
            IList<ArmorSetSearchResult> armors = await solver.SearchArmorSets(solverData, cancellationToken);
            Assert.AreEqual(1, armors.Count);
        }

        [TestMethod()]
        public async Task SearchTest_DB_RoyalMastersTouch()
        {
            var weapon = new Weapon(0, WeaponType.DualBlades, new int[] { 3 }, new IAbility[0], null);
            var solverData = new SolverData();
            var desiredAbilities = new IAbility[]
            {
                new Ability(getSkillByName("Critical Eye"), 4, new Dictionary<string, string>()),
                new Ability(getSkillByName("Water Attack"), 6, new Dictionary<string, string>()),
                new Ability(getSkillByName("Critical Boost"), 2, new Dictionary<string, string>()),
                new Ability(getSkillByName("Weakness Exploit"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Handicraft"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Latent Power"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Critical Element"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Agitator"), 2, new Dictionary<string, string>()),
                new Ability(getSkillByName("Divine Blessing"), 2, new Dictionary<string, string>()),
                new Ability(getSkillByName("Critical Draw"), 2, new Dictionary<string, string>()),
                new Ability(getSkillByName("Health Boost"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Heat Guard"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Master's Touch"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Free Elem/Ammo Up"), 3, new Dictionary<string, string>()),


            };
            solverData.Setup(weapon, heads, chests, gloves, waists, legs, charms.SelectMany(c => c.Levels), decos.Select(d => new Contracts.SolverDataJewelModel(d, int.MaxValue)), desiredAbilities);
            DeselectOther(solverData.AllHeads, "Rimeguard Helm β+");
            DeselectOther(solverData.AllChests, "Kaiser Mail β+");
            DeselectOther(solverData.AllGloves, "Kaiser Vambraces β+");
            DeselectOther(solverData.AllWaists, "Kaiser Coil β+");
            DeselectOther(solverData.AllLegs, "Rimeguard Greaves β+");
            DeselectOther(solverData.AllCharms, "Awakening Charm 3");
            var solver = new IceborneSolver();
            var cancellationToken = new System.Threading.CancellationToken();
            IList<ArmorSetSearchResult> armors = await solver.SearchArmorSets(solverData, cancellationToken);
            Assert.AreEqual(1, armors.Count);
        }

        [TestMethod()]
        public async Task SearchTest_LS_WyvernBladeLuna()
        {
            var weapon = new Weapon(0, WeaponType.LongSword, new int[] { 1 }, new IAbility[0], null);
            var solverData = new SolverData();
            var desiredAbilities = new IAbility[]
            {
                new Ability(getSkillByName("Critical Eye"), 4, new Dictionary<string, string>()),
                new Ability(getSkillByName("Critical Boost"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Weakness Exploit"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Latent Power"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Agitator"), 5, new Dictionary<string, string>()),
                new Ability(getSkillByName("Health Boost"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Heat Guard"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Master's Touch"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Peak Performance"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Attack Boost"), 7, new Dictionary<string, string>()),
                new Ability(getSkillByName("Recovery Up"), 1, new Dictionary<string, string>()),


            };
            solverData.Setup(weapon, heads, chests, gloves, waists, legs, charms.SelectMany(c => c.Levels), decos.Select(d => new Contracts.SolverDataJewelModel(d, int.MaxValue)), desiredAbilities);
            DeselectOther(solverData.AllHeads, "Kaiser Crown β+");
            DeselectOther(solverData.AllChests, "Rex Roar Mail β+");
            DeselectOther(solverData.AllGloves, "Kaiser Vambraces β+");
            DeselectOther(solverData.AllWaists, "Kaiser Coil β+");
            DeselectOther(solverData.AllLegs, "Garuga Greaves β+");
            DeselectOther(solverData.AllCharms, "Challenger Charm 4");
            var solver = new IceborneSolver();
            var cancellationToken = new System.Threading.CancellationToken();
            IList<ArmorSetSearchResult> armors = await solver.SearchArmorSets(solverData, cancellationToken);
            Assert.AreEqual(1, armors.Count);
        }

        [TestMethod()]
        public async Task SearchTest_LS_MagdarosVolcansword()
        {
            var weapon = new Weapon(0, WeaponType.LongSword, new int[] { 2 }, new IAbility[0], null);
            var solverData = new SolverData();
            var desiredAbilities = new IAbility[]
            {
                new Ability(getSkillByName("Critical Eye"), 7, new Dictionary<string, string>()),
                new Ability(getSkillByName("Critical Boost"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Weakness Exploit"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Latent Power"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Agitator"), 4, new Dictionary<string, string>()),
                new Ability(getSkillByName("Heat Guard"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Master's Touch"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Attack Boost"), 7, new Dictionary<string, string>()),
                new Ability(getSkillByName("Blast Attack"), 2, new Dictionary<string, string>()),
                new Ability(getSkillByName("Handicraft"), 5, new Dictionary<string, string>()),
            };
            solverData.Setup(weapon, heads, chests, gloves, waists, legs, charms.SelectMany(c => c.Levels), decos.Select(d => new Contracts.SolverDataJewelModel(d, int.MaxValue)), desiredAbilities);
            DeselectOther(solverData.AllHeads, "Kaiser Crown β+");
            DeselectOther(solverData.AllChests, "Rex Roar Mail β+");
            DeselectOther(solverData.AllGloves, "Kaiser Vambraces β+");
            DeselectOther(solverData.AllWaists, "Kaiser Coil β+");
            DeselectOther(solverData.AllLegs, "Garuga Greaves β+");
            DeselectOther(solverData.AllCharms, "Handicraft Charm 4");
            var solver = new IceborneSolver();
            var cancellationToken = new System.Threading.CancellationToken();
            IList<ArmorSetSearchResult> armors = await solver.SearchArmorSets(solverData, cancellationToken);
            Assert.AreEqual(1, armors.Count);
        }

        [TestMethod()]
        public async Task SearchTest_LS_MagdarosVolcansword2()
        {
            var weapon = new Weapon(0, WeaponType.LongSword, new int[] { 2 }, new IAbility[0], null);
            var solverData = new SolverData();
            var desiredAbilities = new IAbility[]
            {
                new Ability(getSkillByName("Critical Eye"), 7, new Dictionary<string, string>()),
                new Ability(getSkillByName("Critical Boost"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Weakness Exploit"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Latent Power"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Agitator"), 5, new Dictionary<string, string>()),
                new Ability(getSkillByName("Heat Guard"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Master's Touch"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Attack Boost"), 5, new Dictionary<string, string>()),
                new Ability(getSkillByName("Blast Attack"), 2, new Dictionary<string, string>()),
                new Ability(getSkillByName("Handicraft"), 5, new Dictionary<string, string>()),
                new Ability(getSkillByName("Health Boost"), 1, new Dictionary<string, string>()),


            };
            solverData.Setup(weapon, heads, chests, gloves, waists, legs, charms.SelectMany(c => c.Levels), decos.Select(d => new Contracts.SolverDataJewelModel(d, int.MaxValue)), desiredAbilities);
            DeselectOther(solverData.AllHeads, "Kaiser Crown β+");
            DeselectOther(solverData.AllChests, "Rex Roar Mail β+");
            DeselectOther(solverData.AllGloves, "Kaiser Vambraces β+");
            DeselectOther(solverData.AllWaists, "Kaiser Coil β+");
            DeselectOther(solverData.AllLegs, "Garuga Greaves β+");
            DeselectOther(solverData.AllCharms, "Handicraft Charm 4");
            var solver = new IceborneSolver();
            var cancellationToken = new System.Threading.CancellationToken();
            IList<ArmorSetSearchResult> armors = await solver.SearchArmorSets(solverData, cancellationToken);
            Assert.AreEqual(1, armors.Count);
        }

        [TestMethod()]
        public async Task SearchTest_LS_GreatDemonHalberd()
        {
            var weapon = new Weapon(0, WeaponType.LongSword, new int[] { 4 }, new IAbility[0], null);
            var solverData = new SolverData();
            var desiredAbilities = new IAbility[]
            {
                new Ability(getSkillByName("Critical Eye"), 7, new Dictionary<string, string>()),
                new Ability(getSkillByName("Critical Boost"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Weakness Exploit"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Latent Power"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Agitator"), 5, new Dictionary<string, string>()),
                new Ability(getSkillByName("Heat Guard"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Master's Touch"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Attack Boost"), 7, new Dictionary<string, string>()),
                new Ability(getSkillByName("Blast Attack"), 2, new Dictionary<string, string>()),
                new Ability(getSkillByName("Handicraft"), 4, new Dictionary<string, string>()),
                new Ability(getSkillByName("Health Boost"), 1, new Dictionary<string, string>()),


            };
            solverData.Setup(weapon, heads, chests, gloves, waists, legs, charms.SelectMany(c => c.Levels), decos.Select(d => new Contracts.SolverDataJewelModel(d, int.MaxValue)), desiredAbilities);
            DeselectOther(solverData.AllHeads, "Kaiser Crown β+");
            DeselectOther(solverData.AllChests, "Rex Roar Mail β+");
            DeselectOther(solverData.AllGloves, "Kaiser Vambraces β+");
            DeselectOther(solverData.AllWaists, "Kaiser Coil β+");
            DeselectOther(solverData.AllLegs, "Garuga Greaves β+");
            DeselectOther(solverData.AllCharms, "Handicraft Charm 4");
            var solver = new IceborneSolver();
            var cancellationToken = new System.Threading.CancellationToken();
            IList<ArmorSetSearchResult> armors = await solver.SearchArmorSets(solverData, cancellationToken);
            Assert.AreEqual(1, armors.Count);
        }

        [TestMethod()]
        public async Task SearchTest_DB_AnjaTrueCriticalElement_ShouldNotBeFound()
        {
            var weapon = new Weapon(0, WeaponType.DualBlades, new int[] { }, new IAbility[0], null);
            var solverData = new SolverData();
            var desiredAbilities = new IAbility[]
            {
                new Ability(getSkillByName("Critical Eye"), 7, new Dictionary<string, string>()),
                new Ability(getSkillByName("Fire Attack"), 6, new Dictionary<string, string>()),
                new Ability(getSkillByName("Windproof"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Critical Boost"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Weakness Exploit"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Item Prolonger"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Slinger Capacity"), 3, new Dictionary<string, string>()),
                new Ability(getSkillByName("Handicraft"), 2, new Dictionary<string, string>()),
                new Ability(getSkillByName("Agitator"), 2, new Dictionary<string, string>()),
                new Ability(getSkillByName("Protective Polish"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("Slinger Capacity Secret"), 1, new Dictionary<string, string>()),
                new Ability(getSkillByName("True Critical Element"), 1, new Dictionary<string, string>()),
            };
            solverData.Setup(weapon, heads, chests, gloves, waists, legs, charms.SelectMany(c => c.Levels), decos.Select(d => new Contracts.SolverDataJewelModel(d, int.MaxValue)), desiredAbilities);
            DeselectOther(solverData.AllHeads, "Silver Solhelm β+");
            DeselectOther(solverData.AllChests, "Silver Solmail β+");
            DeselectOther(solverData.AllGloves, "Silver Solbraces β+");
            DeselectOther(solverData.AllWaists, "Silver Solcoil β+");
            DeselectOther(solverData.AllLegs, "Rex Roar Greaves β+");
            DeselectOther(solverData.AllCharms, "Blaze Charm 5");
            var solver = new IceborneSolver();
            var cancellationToken = new System.Threading.CancellationToken();
            IList<ArmorSetSearchResult> armors = await solver.SearchArmorSets(solverData, cancellationToken);
            Assert.AreEqual(0, armors.Count);
        }
    }
}
