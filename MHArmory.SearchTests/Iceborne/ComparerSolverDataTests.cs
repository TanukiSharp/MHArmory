using Microsoft.VisualStudio.TestTools.UnitTesting;
using MHArmory.Search.Iceborne;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.Core.DataStructures;
using MHArmory.Search.Contracts;

namespace MHArmory.Search.Iceborne.Tests
{
    [TestClass()]
    public class ComparerSolverDataTests
    {
        private static IEquipment CreateWeapon()
        {
            return new Weapon(0, WeaponType.Bow, Array.Empty<int>(), Array.Empty<IAbility>(), null);
        }

        private static IArmorPiece CreateArmor(int id, EquipmentType type, int rarity, int[] slots, IAbility[] abilities, IArmorSetSkill[] setSkills)
        {
            return new ArmorPiece(id, new Dictionary<string, string>(), type, rarity, slots,
                abilities, setSkills, new ArmorPieceDefense(0, 0, 0),
                new ArmorPieceResistances(0, 0, 0, 0, 0), new ArmorPieceAttributes(Gender.Both),
                null, null, null, null);
        }

        private static ISkill CreateSkill(int id)
        {
            return new Skill(id, new Dictionary<string, string>(), new Dictionary<string, string>(), null, null);
        }

        private static IAbility CreateAbility(ISkill skill, int level)
        {
            return new Ability(skill, level, new Dictionary<string, string>());
        }

        private static ICharmLevel CreateCharm(int id, IAbility[] abilities)
        {
            return new CharmLevel(id, 1, new Dictionary<string, string>(), 12, Array.Empty<int>(), abilities, null, null);
        }

        private static SolverDataJewelModel CreateDeco(int id, int slotSize, IAbility[] abilities, int available, bool generic = false)
        {
            return new SolverDataJewelModel(new Jewel(id, new Dictionary<string, string>(), 12, slotSize, abilities), available, generic);
        }

        private static IArmorSetSkill CreateArmorSetSkill(int id, int skill)
        {
            return new ArmorSetSkill(id, new Dictionary<string, string>(), new IArmorSetSkillPart[] { new ArmorSetSkillPart(id, 1, new IAbility[] { CreateAbility(CreateSkill(skill), 1) }) });
        }

        [TestMethod()]
        public void SetupSortOutExcludedSkillsTest()
        {
            var comparer = new ComparerSolverData();

            IEnumerable<IArmorPiece> CreateArmors(EquipmentType type)
            {
                return new List<IArmorPiece>()
                {
                    CreateArmor(0, type, 12, new int[] { }, new IAbility[] {CreateAbility(CreateSkill(0), 1)},null),
                    CreateArmor(1, type, 12, new int[] { }, new IAbility[] {CreateAbility(CreateSkill(1), 1)},null)
                };
            }

            IEnumerable<IArmorPiece> heads = CreateArmors(EquipmentType.Head);
            IEnumerable<IArmorPiece> chests = CreateArmors(EquipmentType.Chest);
            IEnumerable<IArmorPiece> arms = CreateArmors(EquipmentType.Gloves);
            IEnumerable<IArmorPiece> waist = CreateArmors(EquipmentType.Waist);
            IEnumerable<IArmorPiece> legs = CreateArmors(EquipmentType.Legs);
            var charms = new List<ICharmLevel>()
            {
                CreateCharm(0, new IAbility[] { CreateAbility(CreateSkill(0), 1)}),
                CreateCharm(1, new IAbility[] { CreateAbility(CreateSkill(1), 1)})
            };

            comparer.Setup(CreateWeapon(), heads, chests, arms, waist, legs, charms, new List<SolverDataJewelModel>(), new IAbility[] { CreateAbility(CreateSkill(0), 0) });

            Assert.Equals(1, comparer.AllHeads.Count());
            Assert.Equals(1, comparer.AllHeads[0].Equipment.Id);
            Assert.IsTrue(comparer.AllHeads[0].IsSelected);

            Assert.Equals(1, comparer.AllChests.Count());
            Assert.Equals(1, comparer.AllChests[0].Equipment.Id);
            Assert.IsTrue(comparer.AllChests[0].IsSelected);

            Assert.Equals(1, comparer.AllGloves.Count());
            Assert.Equals(1, comparer.AllGloves[0].Equipment.Id);
            Assert.IsTrue(comparer.AllGloves[0].IsSelected);

            Assert.Equals(1, comparer.AllWaists.Count());
            Assert.Equals(1, comparer.AllWaists[0].Equipment.Id);
            Assert.IsTrue(comparer.AllWaists[0].IsSelected);

            Assert.Equals(1, comparer.AllLegs.Count());
            Assert.Equals(1, comparer.AllLegs[0].Equipment.Id);
            Assert.IsTrue(comparer.AllLegs[0].IsSelected);

            Assert.Equals(1, comparer.AllCharms.Count());
            Assert.Equals(1, comparer.AllCharms[0].Equipment.Id);
            Assert.IsTrue(comparer.AllCharms[0].IsSelected);
        }

        [TestMethod()]
        public void SetupInlcudeLowerTierTest()
        {
            var comparer = new ComparerSolverData();
            comparer.IncludeLowerTier = true;

            IEnumerable<IArmorPiece> CreateArmors(EquipmentType type)
            {
                return new List<IArmorPiece>()
                {
                    CreateArmor(0, type, 8, new int[] { }, new IAbility[] {CreateAbility(CreateSkill(0), 1)},null),
                    CreateArmor(1, type, 12, new int[] { }, new IAbility[] {CreateAbility(CreateSkill(1), 1)},null)
                };
            }

            IEnumerable<IArmorPiece> heads = CreateArmors(EquipmentType.Head);
            IEnumerable<IArmorPiece> chests = CreateArmors(EquipmentType.Chest);
            IEnumerable<IArmorPiece> arms = CreateArmors(EquipmentType.Gloves);
            IEnumerable<IArmorPiece> waist = CreateArmors(EquipmentType.Waist);
            IEnumerable<IArmorPiece> legs = CreateArmors(EquipmentType.Legs);
            var charms = new List<ICharmLevel>()
            {
                CreateCharm(0, new IAbility[] { CreateAbility(CreateSkill(0), 1)}),
                CreateCharm(1, new IAbility[] { CreateAbility(CreateSkill(1), 1)})
            };

            comparer.Setup(CreateWeapon(), heads, chests, arms, waist, legs, charms, new List<SolverDataJewelModel>(), new IAbility[] { });

            Assert.Equals(2, comparer.AllHeads.Count());
            Assert.Equals(2, comparer.AllChests.Count());
            Assert.Equals(2, comparer.AllGloves.Count());
            Assert.Equals(2, comparer.AllWaists.Count());
            Assert.Equals(2, comparer.AllLegs.Count());
            Assert.Equals(2, comparer.AllCharms.Count());
        }


        [TestMethod()]
        public void SetupDontInlcudeLowerTierTest()
        {
            var comparer = new ComparerSolverData();
            comparer.IncludeLowerTier = false;

            IEnumerable<IArmorPiece> CreateArmors(EquipmentType type)
            {
                return new List<IArmorPiece>()
                {
                    CreateArmor(0, type, 8, new int[] { }, new IAbility[] {CreateAbility(CreateSkill(0), 1)},null),
                    CreateArmor(1, type, 12, new int[] { }, new IAbility[] {CreateAbility(CreateSkill(1), 1)},null)
                };
            }

            IEnumerable<IArmorPiece> heads = CreateArmors(EquipmentType.Head);
            IEnumerable<IArmorPiece> chests = CreateArmors(EquipmentType.Chest);
            IEnumerable<IArmorPiece> arms = CreateArmors(EquipmentType.Gloves);
            IEnumerable<IArmorPiece> waist = CreateArmors(EquipmentType.Waist);
            IEnumerable<IArmorPiece> legs = CreateArmors(EquipmentType.Legs);
            var charms = new List<ICharmLevel>()
            {
                CreateCharm(0, new IAbility[] { CreateAbility(CreateSkill(0), 1)}),
                CreateCharm(1, new IAbility[] { CreateAbility(CreateSkill(1), 1)})
            };

            comparer.Setup(CreateWeapon(), heads, chests, arms, waist, legs, charms, new List<SolverDataJewelModel>(), new IAbility[] { });

            Assert.Equals(1, comparer.AllHeads.Count());
            Assert.Equals(1, comparer.AllHeads[0].Equipment.Id);
            Assert.IsTrue(comparer.AllHeads[0].IsSelected);

            Assert.Equals(1, comparer.AllChests.Count());
            Assert.Equals(1, comparer.AllChests[0].Equipment.Id);
            Assert.IsTrue(comparer.AllChests[0].IsSelected);

            Assert.Equals(1, comparer.AllGloves.Count());
            Assert.Equals(1, comparer.AllGloves[0].Equipment.Id);
            Assert.IsTrue(comparer.AllGloves[0].IsSelected);

            Assert.Equals(1, comparer.AllWaists.Count());
            Assert.Equals(1, comparer.AllWaists[0].Equipment.Id);
            Assert.IsTrue(comparer.AllWaists[0].IsSelected);

            Assert.Equals(1, comparer.AllLegs.Count());
            Assert.Equals(1, comparer.AllLegs[0].Equipment.Id);
            Assert.IsTrue(comparer.AllLegs[0].IsSelected);

            Assert.Equals(1, comparer.AllCharms.Count());
            Assert.Equals(1, comparer.AllCharms[0].Equipment.Id);
            Assert.IsTrue(comparer.AllCharms[0].IsSelected);
        }


        [TestMethod()]
        public void SetupUncomparable_differentSKills_NoSlots_Test()
        {
            var comparer = new ComparerSolverData();

            IEnumerable<IArmorPiece> CreateArmors(EquipmentType type)
            {
                return new List<IArmorPiece>()
                {
                    CreateArmor(0, type, 12, new int[] { }, new IAbility[] {CreateAbility(CreateSkill(0), 1)},null),
                    CreateArmor(1, type, 12, new int[] { }, new IAbility[] {CreateAbility(CreateSkill(1), 1)},null)
                };
            }

            IEnumerable<IArmorPiece> heads = CreateArmors(EquipmentType.Head);
            IEnumerable<IArmorPiece> chests = CreateArmors(EquipmentType.Chest);
            IEnumerable<IArmorPiece> arms = CreateArmors(EquipmentType.Gloves);
            IEnumerable<IArmorPiece> waist = CreateArmors(EquipmentType.Waist);
            IEnumerable<IArmorPiece> legs = CreateArmors(EquipmentType.Legs);
            var charms = new List<ICharmLevel>();

            comparer.Setup(CreateWeapon(), heads, chests, arms, waist, legs, charms, new List<SolverDataJewelModel>(), new IAbility[] { CreateAbility(CreateSkill(0), 7), CreateAbility(CreateSkill(1), 7) });

            Assert.Equals(2, comparer.AllHeads.Count());
            Assert.IsTrue(comparer.AllHeads[0].IsSelected);
            Assert.IsTrue(comparer.AllHeads[1].IsSelected);

            Assert.Equals(2, comparer.AllChests.Count());
            Assert.IsTrue(comparer.AllChests[0].IsSelected);
            Assert.IsTrue(comparer.AllChests[1].IsSelected);

            Assert.Equals(2, comparer.AllGloves.Count());
            Assert.IsTrue(comparer.AllGloves[0].IsSelected);
            Assert.IsTrue(comparer.AllGloves[1].IsSelected);

            Assert.Equals(2, comparer.AllWaists.Count());
            Assert.IsTrue(comparer.AllWaists[0].IsSelected);
            Assert.IsTrue(comparer.AllWaists[1].IsSelected);

            Assert.Equals(2, comparer.AllLegs.Count());
            Assert.IsTrue(comparer.AllLegs[0].IsSelected);
            Assert.IsTrue(comparer.AllLegs[1].IsSelected);
        }

        [TestMethod()]
        public void SetupUncomparable_differentDecoSlot_Test()
        {
            var comparer = new ComparerSolverData();

            IEnumerable<IArmorPiece> CreateArmors(EquipmentType type)
            {
                return new List<IArmorPiece>()
                {
                    CreateArmor(0, type, 12, new int[] {2, 2}, new IAbility[] {CreateAbility(CreateSkill(0), 1)}, null),
                    CreateArmor(1, type, 12, new int[] {3, 1}, new IAbility[] {CreateAbility(CreateSkill(0), 1)}, null)
                };
            }

            IEnumerable<IArmorPiece> heads = CreateArmors(EquipmentType.Head);
            IEnumerable<IArmorPiece> chests = CreateArmors(EquipmentType.Chest);
            IEnumerable<IArmorPiece> arms = CreateArmors(EquipmentType.Gloves);
            IEnumerable<IArmorPiece> waist = CreateArmors(EquipmentType.Waist);
            IEnumerable<IArmorPiece> legs = CreateArmors(EquipmentType.Legs);
            var charms = new List<ICharmLevel>();

            comparer.Setup(CreateWeapon(), heads, chests, arms, waist, legs, charms, new List<SolverDataJewelModel>(), new IAbility[] { CreateAbility(CreateSkill(0), 7)});

            Assert.Equals(2, comparer.AllHeads.Count());
            Assert.IsTrue(comparer.AllHeads[0].IsSelected);
            Assert.IsTrue(comparer.AllHeads[1].IsSelected);

            Assert.Equals(2, comparer.AllChests.Count());
            Assert.IsTrue(comparer.AllChests[0].IsSelected);
            Assert.IsTrue(comparer.AllChests[1].IsSelected);

            Assert.Equals(2, comparer.AllGloves.Count());
            Assert.IsTrue(comparer.AllGloves[0].IsSelected);
            Assert.IsTrue(comparer.AllGloves[1].IsSelected);

            Assert.Equals(2, comparer.AllWaists.Count());
            Assert.IsTrue(comparer.AllWaists[0].IsSelected);
            Assert.IsTrue(comparer.AllWaists[1].IsSelected);

            Assert.Equals(2, comparer.AllLegs.Count());
            Assert.IsTrue(comparer.AllLegs[0].IsSelected);
            Assert.IsTrue(comparer.AllLegs[1].IsSelected);
        }

        [TestMethod()]
        public void SetupUncomparable_NoFittingDeco_Test()
        {
            var comparer = new ComparerSolverData();

            IEnumerable<IArmorPiece> CreateArmors(EquipmentType type)
            {
                return new List<IArmorPiece>()
                {
                    CreateArmor(0, type, 12, new int[] { }, new IAbility[] {CreateAbility(CreateSkill(0), 1), CreateAbility(CreateSkill(1), 1)}, null),
                    CreateArmor(1, type, 12, new int[] {1}, new IAbility[] {CreateAbility(CreateSkill(0), 1)}, null)
                };
            }

            IEnumerable<IArmorPiece> heads = CreateArmors(EquipmentType.Head);
            IEnumerable<IArmorPiece> chests = CreateArmors(EquipmentType.Chest);
            IEnumerable<IArmorPiece> arms = CreateArmors(EquipmentType.Gloves);
            IEnumerable<IArmorPiece> waist = CreateArmors(EquipmentType.Waist);
            IEnumerable<IArmorPiece> legs = CreateArmors(EquipmentType.Legs);
            var charms = new List<ICharmLevel>();
            var decos = new List<SolverDataJewelModel>
            {
                CreateDeco(0, 2, new IAbility[]{CreateAbility(CreateSkill(1), 1) }, 5)
            };

            comparer.Setup(CreateWeapon(), heads, chests, arms, waist, legs, charms, decos, new IAbility[] { CreateAbility(CreateSkill(0), 7) });

            Assert.Equals(2, comparer.AllHeads.Count());
            Assert.IsTrue(comparer.AllHeads[0].IsSelected);
            Assert.IsTrue(comparer.AllHeads[1].IsSelected);

            Assert.Equals(2, comparer.AllChests.Count());
            Assert.IsTrue(comparer.AllChests[0].IsSelected);
            Assert.IsTrue(comparer.AllChests[1].IsSelected);

            Assert.Equals(2, comparer.AllGloves.Count());
            Assert.IsTrue(comparer.AllGloves[0].IsSelected);
            Assert.IsTrue(comparer.AllGloves[1].IsSelected);

            Assert.Equals(2, comparer.AllWaists.Count());
            Assert.IsTrue(comparer.AllWaists[0].IsSelected);
            Assert.IsTrue(comparer.AllWaists[1].IsSelected);

            Assert.Equals(2, comparer.AllLegs.Count());
            Assert.IsTrue(comparer.AllLegs[0].IsSelected);
            Assert.IsTrue(comparer.AllLegs[1].IsSelected);
        }

        [TestMethod()]
        public void SetupUncomparable_MoreSlots_NotEnoughDecos_Test()
        {
            var comparer = new ComparerSolverData();

            IEnumerable<IArmorPiece> CreateArmors(EquipmentType type)
            {
                return new List<IArmorPiece>()
                {
                    CreateArmor(0, type, 12, new int[] { }, new IAbility[] {CreateAbility(CreateSkill(0), 3)}, null),
                    CreateArmor(1, type, 12, new int[] {1, 1, 1}, new IAbility[] {CreateAbility(CreateSkill(0), 1)}, null)
                };
            }

            IEnumerable<IArmorPiece> heads = CreateArmors(EquipmentType.Head);
            IEnumerable<IArmorPiece> chests = CreateArmors(EquipmentType.Chest);
            IEnumerable<IArmorPiece> arms = CreateArmors(EquipmentType.Gloves);
            IEnumerable<IArmorPiece> waist = CreateArmors(EquipmentType.Waist);
            IEnumerable<IArmorPiece> legs = CreateArmors(EquipmentType.Legs);
            var charms = new List<ICharmLevel>();
            var decos = new List<SolverDataJewelModel>
            {
                CreateDeco(0, 1, new IAbility[]{CreateAbility(CreateSkill(0), 1) }, 1)
            };

            comparer.Setup(CreateWeapon(), heads, chests, arms, waist, legs, charms, decos, new IAbility[] { CreateAbility(CreateSkill(0), 7) });

            Assert.Equals(2, comparer.AllHeads.Count());
            Assert.IsTrue(comparer.AllHeads[0].IsSelected);
            Assert.IsTrue(comparer.AllHeads[1].IsSelected);

            Assert.Equals(2, comparer.AllChests.Count());
            Assert.IsTrue(comparer.AllChests[0].IsSelected);
            Assert.IsTrue(comparer.AllChests[1].IsSelected);

            Assert.Equals(2, comparer.AllGloves.Count());
            Assert.IsTrue(comparer.AllGloves[0].IsSelected);
            Assert.IsTrue(comparer.AllGloves[1].IsSelected);

            Assert.Equals(2, comparer.AllWaists.Count());
            Assert.IsTrue(comparer.AllWaists[0].IsSelected);
            Assert.IsTrue(comparer.AllWaists[1].IsSelected);

            Assert.Equals(2, comparer.AllLegs.Count());
            Assert.IsTrue(comparer.AllLegs[0].IsSelected);
            Assert.IsTrue(comparer.AllLegs[1].IsSelected);
        }

        [TestMethod()]
        public void SetupComparable_MoreSlots_EnoughDecos_Test()
        {
            var comparer = new ComparerSolverData();

            IEnumerable<IArmorPiece> CreateArmors(EquipmentType type)
            {
                return new List<IArmorPiece>()
                {
                    CreateArmor(0, type, 12, new int[] { }, new IAbility[] {CreateAbility(CreateSkill(0), 3)}, null),
                    CreateArmor(1, type, 12, new int[] {1, 1, 1}, new IAbility[] {CreateAbility(CreateSkill(0), 1)}, null)
                };
            }

            IEnumerable<IArmorPiece> heads = CreateArmors(EquipmentType.Head);
            IEnumerable<IArmorPiece> chests = CreateArmors(EquipmentType.Chest);
            IEnumerable<IArmorPiece> arms = CreateArmors(EquipmentType.Gloves);
            IEnumerable<IArmorPiece> waist = CreateArmors(EquipmentType.Waist);
            IEnumerable<IArmorPiece> legs = CreateArmors(EquipmentType.Legs);
            var charms = new List<ICharmLevel>();
            var decos = new List<SolverDataJewelModel>
            {
                CreateDeco(0, 1, new IAbility[]{CreateAbility(CreateSkill(0), 1) }, 7)
            };

            comparer.Setup(CreateWeapon(), heads, chests, arms, waist, legs, charms, decos, new IAbility[] { CreateAbility(CreateSkill(0), 7) });

            Assert.Equals(2, comparer.AllHeads.Count());
            Assert.IsFalse(comparer.AllHeads[0].IsSelected);
            Assert.IsTrue(comparer.AllHeads[1].IsSelected);

            Assert.Equals(2, comparer.AllChests.Count());
            Assert.IsFalse(comparer.AllChests[0].IsSelected);
            Assert.IsTrue(comparer.AllChests[1].IsSelected);

            Assert.Equals(2, comparer.AllGloves.Count());
            Assert.IsFalse(comparer.AllGloves[0].IsSelected);
            Assert.IsTrue(comparer.AllGloves[1].IsSelected);

            Assert.Equals(2, comparer.AllWaists.Count());
            Assert.IsFalse(comparer.AllWaists[0].IsSelected);
            Assert.IsTrue(comparer.AllWaists[1].IsSelected);

            Assert.Equals(2, comparer.AllLegs.Count());
            Assert.IsFalse(comparer.AllLegs[0].IsSelected);
            Assert.IsTrue(comparer.AllLegs[1].IsSelected);
        }

        [TestMethod()]
        public void SetupEqual_TakeBoth_Test()
        {
            var comparer = new ComparerSolverData();
            comparer.IfEqualUseArmorWithBetterSlots = false;

            IEnumerable<IArmorPiece> CreateArmors(EquipmentType type)
            {
                return new List<IArmorPiece>()
                {
                    CreateArmor(0, type, 12, new int[] { }, new IAbility[] {CreateAbility(CreateSkill(0), 3)}, null),
                    CreateArmor(1, type, 12, new int[] {1, 1}, new IAbility[] {CreateAbility(CreateSkill(0), 1)}, null)
                };
            }

            IEnumerable<IArmorPiece> heads = CreateArmors(EquipmentType.Head);
            IEnumerable<IArmorPiece> chests = CreateArmors(EquipmentType.Chest);
            IEnumerable<IArmorPiece> arms = CreateArmors(EquipmentType.Gloves);
            IEnumerable<IArmorPiece> waist = CreateArmors(EquipmentType.Waist);
            IEnumerable<IArmorPiece> legs = CreateArmors(EquipmentType.Legs);
            var charms = new List<ICharmLevel>();
            var decos = new List<SolverDataJewelModel>
            {
                CreateDeco(0, 1, new IAbility[]{CreateAbility(CreateSkill(0), 1) }, 7)
            };

            comparer.Setup(CreateWeapon(), heads, chests, arms, waist, legs, charms, decos, new IAbility[] { CreateAbility(CreateSkill(0), 7) });

            Assert.Equals(2, comparer.AllHeads.Count());
            Assert.IsTrue(comparer.AllHeads[0].IsSelected);
            Assert.IsTrue(comparer.AllHeads[1].IsSelected);

            Assert.Equals(2, comparer.AllChests.Count());
            Assert.IsTrue(comparer.AllChests[0].IsSelected);
            Assert.IsTrue(comparer.AllChests[1].IsSelected);

            Assert.Equals(2, comparer.AllGloves.Count());
            Assert.IsTrue(comparer.AllGloves[0].IsSelected);
            Assert.IsTrue(comparer.AllGloves[1].IsSelected);

            Assert.Equals(2, comparer.AllWaists.Count());
            Assert.IsTrue(comparer.AllWaists[0].IsSelected);
            Assert.IsTrue(comparer.AllWaists[1].IsSelected);

            Assert.Equals(2, comparer.AllLegs.Count());
            Assert.IsTrue(comparer.AllLegs[0].IsSelected);
            Assert.IsTrue(comparer.AllLegs[1].IsSelected);
        }

        [TestMethod()]
        public void SetupEqual_PreferSlots_Test()
        {
            var comparer = new ComparerSolverData();
            comparer.IfEqualUseArmorWithBetterSlots = true;

            IEnumerable<IArmorPiece> CreateArmors(EquipmentType type)
            {
                return new List<IArmorPiece>()
                {
                    CreateArmor(0, type, 12, new int[] { }, new IAbility[] {CreateAbility(CreateSkill(0), 3)}, null),
                    CreateArmor(1, type, 12, new int[] {1, 1}, new IAbility[] {CreateAbility(CreateSkill(0), 1)}, null)
                };
            }

            IEnumerable<IArmorPiece> heads = CreateArmors(EquipmentType.Head);
            IEnumerable<IArmorPiece> chests = CreateArmors(EquipmentType.Chest);
            IEnumerable<IArmorPiece> arms = CreateArmors(EquipmentType.Gloves);
            IEnumerable<IArmorPiece> waist = CreateArmors(EquipmentType.Waist);
            IEnumerable<IArmorPiece> legs = CreateArmors(EquipmentType.Legs);
            var charms = new List<ICharmLevel>();
            var decos = new List<SolverDataJewelModel>
            {
                CreateDeco(0, 1, new IAbility[]{CreateAbility(CreateSkill(0), 1) }, 7)
            };

            comparer.Setup(CreateWeapon(), heads, chests, arms, waist, legs, charms, decos, new IAbility[] { CreateAbility(CreateSkill(0), 7) });

            Assert.Equals(2, comparer.AllHeads.Count());
            Assert.IsFalse(comparer.AllHeads[0].IsSelected);
            Assert.IsTrue(comparer.AllHeads[1].IsSelected);

            Assert.Equals(2, comparer.AllChests.Count());
            Assert.IsFalse(comparer.AllChests[0].IsSelected);
            Assert.IsTrue(comparer.AllChests[1].IsSelected);

            Assert.Equals(2, comparer.AllGloves.Count());
            Assert.IsFalse(comparer.AllGloves[0].IsSelected);
            Assert.IsTrue(comparer.AllGloves[1].IsSelected);

            Assert.Equals(2, comparer.AllWaists.Count());
            Assert.IsFalse(comparer.AllWaists[0].IsSelected);
            Assert.IsTrue(comparer.AllWaists[1].IsSelected);

            Assert.Equals(2, comparer.AllLegs.Count());
            Assert.IsFalse(comparer.AllLegs[0].IsSelected);
            Assert.IsTrue(comparer.AllLegs[1].IsSelected);
        }

        [TestMethod()]
        public void SetupComparable_BettterSlots_Test()
        {
            var comparer = new ComparerSolverData();

            IEnumerable<IArmorPiece> CreateArmors(EquipmentType type)
            {
                return new List<IArmorPiece>()
                {
                    CreateArmor(0, type, 12, new int[] {2, 2}, new IAbility[] {CreateAbility(CreateSkill(0), 1)}, null),
                    CreateArmor(1, type, 12, new int[] {1, 1}, new IAbility[] {CreateAbility(CreateSkill(0), 1)}, null)
                };
            }

            IEnumerable<IArmorPiece> heads = CreateArmors(EquipmentType.Head);
            IEnumerable<IArmorPiece> chests = CreateArmors(EquipmentType.Chest);
            IEnumerable<IArmorPiece> arms = CreateArmors(EquipmentType.Gloves);
            IEnumerable<IArmorPiece> waist = CreateArmors(EquipmentType.Waist);
            IEnumerable<IArmorPiece> legs = CreateArmors(EquipmentType.Legs);
            var charms = new List<ICharmLevel>();
            var decos = new List<SolverDataJewelModel>
            {
                CreateDeco(0, 1, new IAbility[]{CreateAbility(CreateSkill(0), 1) }, 7)
            };

            comparer.Setup(CreateWeapon(), heads, chests, arms, waist, legs, charms, decos, new IAbility[] { CreateAbility(CreateSkill(0), 7) });

            Assert.Equals(2, comparer.AllHeads.Count());
            Assert.IsTrue(comparer.AllHeads[0].IsSelected);
            Assert.IsFalse(comparer.AllHeads[1].IsSelected);

            Assert.Equals(2, comparer.AllChests.Count());
            Assert.IsTrue(comparer.AllChests[0].IsSelected);
            Assert.IsFalse(comparer.AllChests[1].IsSelected);

            Assert.Equals(2, comparer.AllGloves.Count());
            Assert.IsTrue(comparer.AllGloves[0].IsSelected);
            Assert.IsFalse(comparer.AllGloves[1].IsSelected);

            Assert.Equals(2, comparer.AllWaists.Count());
            Assert.IsTrue(comparer.AllWaists[0].IsSelected);
            Assert.IsFalse(comparer.AllWaists[1].IsSelected);

            Assert.Equals(2, comparer.AllLegs.Count());
            Assert.IsTrue(comparer.AllLegs[0].IsSelected);
            Assert.IsFalse(comparer.AllLegs[1].IsSelected);
        }

        [TestMethod()]
        public void SetupComparable_DifferentSkills_SlottableDecos_Test()
        {
            var comparer = new ComparerSolverData();

            IEnumerable<IArmorPiece> CreateArmors(EquipmentType type)
            {
                return new List<IArmorPiece>()
                {
                    CreateArmor(0, type, 12, new int[] {}, new IAbility[] {CreateAbility(CreateSkill(0), 1)}, null),
                    CreateArmor(1, type, 12, new int[] {1}, new IAbility[] {CreateAbility(CreateSkill(1), 1)}, null)
                };
            }

            IEnumerable<IArmorPiece> heads = CreateArmors(EquipmentType.Head);
            IEnumerable<IArmorPiece> chests = CreateArmors(EquipmentType.Chest);
            IEnumerable<IArmorPiece> arms = CreateArmors(EquipmentType.Gloves);
            IEnumerable<IArmorPiece> waist = CreateArmors(EquipmentType.Waist);
            IEnumerable<IArmorPiece> legs = CreateArmors(EquipmentType.Legs);
            var charms = new List<ICharmLevel>();
            var decos = new List<SolverDataJewelModel>
            {
                CreateDeco(0, 1, new IAbility[]{CreateAbility(CreateSkill(0), 1) }, 7),
                CreateDeco(1, 1, new IAbility[]{CreateAbility(CreateSkill(1), 1) }, 7)
            };

            comparer.Setup(CreateWeapon(), heads, chests, arms, waist, legs, charms, decos, new IAbility[] { CreateAbility(CreateSkill(0), 7), CreateAbility(CreateSkill(1), 7) });

            Assert.Equals(2, comparer.AllHeads.Count());
            Assert.IsFalse(comparer.AllHeads[0].IsSelected);
            Assert.IsTrue(comparer.AllHeads[1].IsSelected);

            Assert.Equals(2, comparer.AllChests.Count());
            Assert.IsFalse(comparer.AllChests[0].IsSelected);
            Assert.IsTrue(comparer.AllChests[1].IsSelected);

            Assert.Equals(2, comparer.AllGloves.Count());
            Assert.IsFalse(comparer.AllGloves[0].IsSelected);
            Assert.IsTrue(comparer.AllGloves[1].IsSelected);

            Assert.Equals(2, comparer.AllWaists.Count());
            Assert.IsFalse(comparer.AllWaists[0].IsSelected);
            Assert.IsTrue(comparer.AllWaists[1].IsSelected);

            Assert.Equals(2, comparer.AllLegs.Count());
            Assert.IsFalse(comparer.AllLegs[0].IsSelected);
            Assert.IsTrue(comparer.AllLegs[1].IsSelected);
        }

        [TestMethod()]
        public void SetupEqual_DifferentSkills_SameSlotCount_Test()
        {
            var comparer = new ComparerSolverData();

            IEnumerable<IArmorPiece> CreateArmors(EquipmentType type)
            {
                return new List<IArmorPiece>()
                {
                    CreateArmor(0, type, 12, new int[] {1}, new IAbility[] {CreateAbility(CreateSkill(0), 1)}, null),
                    CreateArmor(1, type, 12, new int[] {1}, new IAbility[] {CreateAbility(CreateSkill(1), 1)}, null)
                };
            }

            IEnumerable<IArmorPiece> heads = CreateArmors(EquipmentType.Head);
            IEnumerable<IArmorPiece> chests = CreateArmors(EquipmentType.Chest);
            IEnumerable<IArmorPiece> arms = CreateArmors(EquipmentType.Gloves);
            IEnumerable<IArmorPiece> waist = CreateArmors(EquipmentType.Waist);
            IEnumerable<IArmorPiece> legs = CreateArmors(EquipmentType.Legs);
            var charms = new List<ICharmLevel>();
            var decos = new List<SolverDataJewelModel>
            {
                CreateDeco(0, 1, new IAbility[]{CreateAbility(CreateSkill(0), 1) }, 7),
                CreateDeco(1, 1, new IAbility[]{CreateAbility(CreateSkill(1), 1) }, 7)
            };

            comparer.Setup(CreateWeapon(), heads, chests, arms, waist, legs, charms, decos, new IAbility[] { CreateAbility(CreateSkill(0), 7), CreateAbility(CreateSkill(1), 7) });

            Assert.Equals(2, comparer.AllHeads.Count());
            Assert.IsTrue(comparer.AllHeads[0].IsSelected);
            Assert.IsTrue(comparer.AllHeads[1].IsSelected);

            Assert.Equals(2, comparer.AllChests.Count());
            Assert.IsTrue(comparer.AllChests[0].IsSelected);
            Assert.IsTrue(comparer.AllChests[1].IsSelected);

            Assert.Equals(2, comparer.AllGloves.Count());
            Assert.IsTrue(comparer.AllGloves[0].IsSelected);
            Assert.IsTrue(comparer.AllGloves[1].IsSelected);

            Assert.Equals(2, comparer.AllWaists.Count());
            Assert.IsTrue(comparer.AllWaists[0].IsSelected);
            Assert.IsTrue(comparer.AllWaists[1].IsSelected);

            Assert.Equals(2, comparer.AllLegs.Count());
            Assert.IsTrue(comparer.AllLegs[0].IsSelected);
            Assert.IsTrue(comparer.AllLegs[1].IsSelected);
        }

        [TestMethod()]
        public void SetupComparable_DifferentSkills_SameSlotCount_BetterSlotSize_Test()
        {
            var comparer = new ComparerSolverData();

            IEnumerable<IArmorPiece> CreateArmors(EquipmentType type)
            {
                return new List<IArmorPiece>()
                {
                    CreateArmor(0, type, 12, new int[] {1}, new IAbility[] {CreateAbility(CreateSkill(0), 1)}, null),
                    CreateArmor(1, type, 12, new int[] {2}, new IAbility[] {CreateAbility(CreateSkill(1), 1)}, null)
                };
            }

            IEnumerable<IArmorPiece> heads = CreateArmors(EquipmentType.Head);
            IEnumerable<IArmorPiece> chests = CreateArmors(EquipmentType.Chest);
            IEnumerable<IArmorPiece> arms = CreateArmors(EquipmentType.Gloves);
            IEnumerable<IArmorPiece> waist = CreateArmors(EquipmentType.Waist);
            IEnumerable<IArmorPiece> legs = CreateArmors(EquipmentType.Legs);
            var charms = new List<ICharmLevel>();
            var decos = new List<SolverDataJewelModel>
            {
                CreateDeco(0, 1, new IAbility[]{CreateAbility(CreateSkill(0), 1) }, 7),
                CreateDeco(1, 1, new IAbility[]{CreateAbility(CreateSkill(1), 1) }, 7)
            };

            comparer.Setup(CreateWeapon(), heads, chests, arms, waist, legs, charms, decos, new IAbility[] { CreateAbility(CreateSkill(0), 7), CreateAbility(CreateSkill(1), 7) });

            Assert.Equals(2, comparer.AllHeads.Count());
            Assert.IsFalse(comparer.AllHeads[0].IsSelected);
            Assert.IsTrue(comparer.AllHeads[1].IsSelected);

            Assert.Equals(2, comparer.AllChests.Count());
            Assert.IsFalse(comparer.AllChests[0].IsSelected);
            Assert.IsTrue(comparer.AllChests[1].IsSelected);

            Assert.Equals(2, comparer.AllGloves.Count());
            Assert.IsFalse(comparer.AllGloves[0].IsSelected);
            Assert.IsTrue(comparer.AllGloves[1].IsSelected);

            Assert.Equals(2, comparer.AllWaists.Count());
            Assert.IsFalse(comparer.AllWaists[0].IsSelected);
            Assert.IsTrue(comparer.AllWaists[1].IsSelected);

            Assert.Equals(2, comparer.AllLegs.Count());
            Assert.IsFalse(comparer.AllLegs[0].IsSelected);
            Assert.IsTrue(comparer.AllLegs[1].IsSelected);
        }

        [TestMethod()]
        public void SetupComparable_MoreSkillsButUnsearched_SameSlots_Test()
        {
            var comparer = new ComparerSolverData();

            IEnumerable<IArmorPiece> CreateArmors(EquipmentType type)
            {
                return new List<IArmorPiece>()
                {
                    CreateArmor(0, type, 12, new int[] {}, new IAbility[] {CreateAbility(CreateSkill(1), 1), CreateAbility(CreateSkill(2), 1)}, null),
                    CreateArmor(1, type, 12, new int[] {}, new IAbility[] {CreateAbility(CreateSkill(1), 1)}, null)
                };
            }

            IEnumerable<IArmorPiece> heads = CreateArmors(EquipmentType.Head);
            IEnumerable<IArmorPiece> chests = CreateArmors(EquipmentType.Chest);
            IEnumerable<IArmorPiece> arms = CreateArmors(EquipmentType.Gloves);
            IEnumerable<IArmorPiece> waist = CreateArmors(EquipmentType.Waist);
            IEnumerable<IArmorPiece> legs = CreateArmors(EquipmentType.Legs);
            var charms = new List<ICharmLevel>();
            var decos = new List<SolverDataJewelModel>
            {
                CreateDeco(0, 1, new IAbility[]{CreateAbility(CreateSkill(0), 1) }, 7),
                CreateDeco(1, 1, new IAbility[]{CreateAbility(CreateSkill(1), 1) }, 7)
            };

            comparer.Setup(CreateWeapon(), heads, chests, arms, waist, legs, charms, decos, new IAbility[] { CreateAbility(CreateSkill(0), 7), CreateAbility(CreateSkill(1), 7) });

            Assert.Equals(2, comparer.AllHeads.Count());
            Assert.IsTrue(comparer.AllHeads[0].IsSelected);
            Assert.IsFalse(comparer.AllHeads[1].IsSelected);

            Assert.Equals(2, comparer.AllChests.Count());
            Assert.IsTrue(comparer.AllChests[0].IsSelected);
            Assert.IsFalse(comparer.AllChests[1].IsSelected);

            Assert.Equals(2, comparer.AllGloves.Count());
            Assert.IsTrue(comparer.AllGloves[0].IsSelected);
            Assert.IsFalse(comparer.AllGloves[1].IsSelected);

            Assert.Equals(2, comparer.AllWaists.Count());
            Assert.IsTrue(comparer.AllWaists[0].IsSelected);
            Assert.IsFalse(comparer.AllWaists[1].IsSelected);

            Assert.Equals(2, comparer.AllLegs.Count());
            Assert.IsTrue(comparer.AllLegs[0].IsSelected);
            Assert.IsFalse(comparer.AllLegs[1].IsSelected);
        }

        [TestMethod()]
        public void SetupComparable_MoreSkillsButUnsearched_MoreSlots_Test()
        {
            var comparer = new ComparerSolverData();

            IEnumerable<IArmorPiece> CreateArmors(EquipmentType type)
            {
                return new List<IArmorPiece>()
                {
                    CreateArmor(0, type, 12, new int[] {}, new IAbility[] {CreateAbility(CreateSkill(1), 1), CreateAbility(CreateSkill(2), 1)}, null),
                    CreateArmor(1, type, 12, new int[] {1}, new IAbility[] {CreateAbility(CreateSkill(1), 1)}, null)
                };
            }

            IEnumerable<IArmorPiece> heads = CreateArmors(EquipmentType.Head);
            IEnumerable<IArmorPiece> chests = CreateArmors(EquipmentType.Chest);
            IEnumerable<IArmorPiece> arms = CreateArmors(EquipmentType.Gloves);
            IEnumerable<IArmorPiece> waist = CreateArmors(EquipmentType.Waist);
            IEnumerable<IArmorPiece> legs = CreateArmors(EquipmentType.Legs);
            var charms = new List<ICharmLevel>();
            var decos = new List<SolverDataJewelModel>
            {
                CreateDeco(0, 1, new IAbility[]{CreateAbility(CreateSkill(0), 1) }, 7),
                CreateDeco(1, 1, new IAbility[]{CreateAbility(CreateSkill(1), 1) }, 7)
            };

            comparer.Setup(CreateWeapon(), heads, chests, arms, waist, legs, charms, decos, new IAbility[] { CreateAbility(CreateSkill(0), 7), CreateAbility(CreateSkill(1), 7) });

            Assert.Equals(2, comparer.AllHeads.Count());
            Assert.IsFalse(comparer.AllHeads[0].IsSelected);
            Assert.IsTrue(comparer.AllHeads[1].IsSelected);

            Assert.Equals(2, comparer.AllChests.Count());
            Assert.IsFalse(comparer.AllChests[0].IsSelected);
            Assert.IsTrue(comparer.AllChests[1].IsSelected);

            Assert.Equals(2, comparer.AllGloves.Count());
            Assert.IsFalse(comparer.AllGloves[0].IsSelected);
            Assert.IsTrue(comparer.AllGloves[1].IsSelected);

            Assert.Equals(2, comparer.AllWaists.Count());
            Assert.IsFalse(comparer.AllWaists[0].IsSelected);
            Assert.IsTrue(comparer.AllWaists[1].IsSelected);

            Assert.Equals(2, comparer.AllLegs.Count());
            Assert.IsFalse(comparer.AllLegs[0].IsSelected);
            Assert.IsTrue(comparer.AllLegs[1].IsSelected);
        }

        [TestMethod()]
        public void SetupEqual_Level4MultiPointDeco_KeepBoth_Test()
        {
            var comparer = new ComparerSolverData();
            comparer.IfEqualUseArmorWithBetterSlots = false;

            IEnumerable<IArmorPiece> CreateArmors(EquipmentType type)
            {
                return new List<IArmorPiece>()
                {
                    CreateArmor(0, type, 12, new int[] {}, new IAbility[] {CreateAbility(CreateSkill(1), 1), CreateAbility(CreateSkill(0), 2)}, null),
                    CreateArmor(1, type, 12, new int[] {4}, new IAbility[] {CreateAbility(CreateSkill(1), 1)}, null)
                };
            }

            IEnumerable<IArmorPiece> heads = CreateArmors(EquipmentType.Head);
            IEnumerable<IArmorPiece> chests = CreateArmors(EquipmentType.Chest);
            IEnumerable<IArmorPiece> arms = CreateArmors(EquipmentType.Gloves);
            IEnumerable<IArmorPiece> waist = CreateArmors(EquipmentType.Waist);
            IEnumerable<IArmorPiece> legs = CreateArmors(EquipmentType.Legs);
            var charms = new List<ICharmLevel>();
            var decos = new List<SolverDataJewelModel>
            {
                CreateDeco(0, 4, new IAbility[]{CreateAbility(CreateSkill(0), 2) }, 7)
            };

            comparer.Setup(CreateWeapon(), heads, chests, arms, waist, legs, charms, decos, new IAbility[] { CreateAbility(CreateSkill(0), 7), CreateAbility(CreateSkill(1), 7) });

            Assert.Equals(2, comparer.AllHeads.Count());
            Assert.IsTrue(comparer.AllHeads[0].IsSelected);
            Assert.IsTrue(comparer.AllHeads[1].IsSelected);

            Assert.Equals(2, comparer.AllChests.Count());
            Assert.IsTrue(comparer.AllChests[0].IsSelected);
            Assert.IsTrue(comparer.AllChests[1].IsSelected);

            Assert.Equals(2, comparer.AllGloves.Count());
            Assert.IsTrue(comparer.AllGloves[0].IsSelected);
            Assert.IsTrue(comparer.AllGloves[1].IsSelected);

            Assert.Equals(2, comparer.AllWaists.Count());
            Assert.IsTrue(comparer.AllWaists[0].IsSelected);
            Assert.IsTrue(comparer.AllWaists[1].IsSelected);

            Assert.Equals(2, comparer.AllLegs.Count());
            Assert.IsTrue(comparer.AllLegs[0].IsSelected);
            Assert.IsTrue(comparer.AllLegs[1].IsSelected);
        }

        [TestMethod()]
        public void SetupEqual_Level4MultiPointDeco_KeepMoreSlots_Test()
        {
            var comparer = new ComparerSolverData();
            comparer.IfEqualUseArmorWithBetterSlots = true;

            IEnumerable<IArmorPiece> CreateArmors(EquipmentType type)
            {
                return new List<IArmorPiece>()
                {
                    CreateArmor(0, type, 12, new int[] {}, new IAbility[] {CreateAbility(CreateSkill(1), 1), CreateAbility(CreateSkill(0), 2)}, null),
                    CreateArmor(1, type, 12, new int[] {4}, new IAbility[] {CreateAbility(CreateSkill(1), 1)}, null)
                };
            }

            IEnumerable<IArmorPiece> heads = CreateArmors(EquipmentType.Head);
            IEnumerable<IArmorPiece> chests = CreateArmors(EquipmentType.Chest);
            IEnumerable<IArmorPiece> arms = CreateArmors(EquipmentType.Gloves);
            IEnumerable<IArmorPiece> waist = CreateArmors(EquipmentType.Waist);
            IEnumerable<IArmorPiece> legs = CreateArmors(EquipmentType.Legs);
            var charms = new List<ICharmLevel>();
            var decos = new List<SolverDataJewelModel>
            {
                CreateDeco(0, 4, new IAbility[]{CreateAbility(CreateSkill(0), 2) }, 7)
            };

            comparer.Setup(CreateWeapon(), heads, chests, arms, waist, legs, charms, decos, new IAbility[] { CreateAbility(CreateSkill(0), 7), CreateAbility(CreateSkill(1), 7) });

            Assert.Equals(2, comparer.AllHeads.Count());
            Assert.IsFalse(comparer.AllHeads[0].IsSelected);
            Assert.IsTrue(comparer.AllHeads[1].IsSelected);

            Assert.Equals(2, comparer.AllChests.Count());
            Assert.IsFalse(comparer.AllChests[0].IsSelected);
            Assert.IsTrue(comparer.AllChests[1].IsSelected);

            Assert.Equals(2, comparer.AllGloves.Count());
            Assert.IsFalse(comparer.AllGloves[0].IsSelected);
            Assert.IsTrue(comparer.AllGloves[1].IsSelected);

            Assert.Equals(2, comparer.AllWaists.Count());
            Assert.IsFalse(comparer.AllWaists[0].IsSelected);
            Assert.IsTrue(comparer.AllWaists[1].IsSelected);

            Assert.Equals(2, comparer.AllLegs.Count());
            Assert.IsFalse(comparer.AllLegs[0].IsSelected);
            Assert.IsTrue(comparer.AllLegs[1].IsSelected);
        }

        [TestMethod()]
        public void SetupEqual_Level4MultiSkillDecoWithOtherSearchedSkill_KeepBoth_Test()
        {
            var comparer = new ComparerSolverData();
            comparer.IfEqualUseArmorWithBetterSlots = false;

            IEnumerable<IArmorPiece> CreateArmors(EquipmentType type)
            {
                return new List<IArmorPiece>()
                {
                    CreateArmor(0, type, 12, new int[] {2}, new IAbility[] {CreateAbility(CreateSkill(1), 1), CreateAbility(CreateSkill(0), 1)}, null),
                    CreateArmor(1, type, 12, new int[] {4}, new IAbility[] {CreateAbility(CreateSkill(1), 1)}, null)
                };
            }

            IEnumerable<IArmorPiece> heads = CreateArmors(EquipmentType.Head);
            IEnumerable<IArmorPiece> chests = CreateArmors(EquipmentType.Chest);
            IEnumerable<IArmorPiece> arms = CreateArmors(EquipmentType.Gloves);
            IEnumerable<IArmorPiece> waist = CreateArmors(EquipmentType.Waist);
            IEnumerable<IArmorPiece> legs = CreateArmors(EquipmentType.Legs);
            var charms = new List<ICharmLevel>();
            var decos = new List<SolverDataJewelModel>
            {
                CreateDeco(0, 2, new IAbility[]{CreateAbility(CreateSkill(0), 1) }, 7),
                CreateDeco(1, 2, new IAbility[]{CreateAbility(CreateSkill(1), 1) }, 7),
                CreateDeco(2, 4, new IAbility[]{CreateAbility(CreateSkill(0), 1), CreateAbility(CreateSkill(1), 1) }, 7)
            };

            comparer.Setup(CreateWeapon(), heads, chests, arms, waist, legs, charms, decos, new IAbility[] { CreateAbility(CreateSkill(0), 7), CreateAbility(CreateSkill(1), 7) });

            Assert.Equals(2, comparer.AllHeads.Count());
            Assert.IsTrue(comparer.AllHeads[0].IsSelected);
            Assert.IsTrue(comparer.AllHeads[1].IsSelected);

            Assert.Equals(2, comparer.AllChests.Count());
            Assert.IsTrue(comparer.AllChests[0].IsSelected);
            Assert.IsTrue(comparer.AllChests[1].IsSelected);

            Assert.Equals(2, comparer.AllGloves.Count());
            Assert.IsTrue(comparer.AllGloves[0].IsSelected);
            Assert.IsTrue(comparer.AllGloves[1].IsSelected);

            Assert.Equals(2, comparer.AllWaists.Count());
            Assert.IsTrue(comparer.AllWaists[0].IsSelected);
            Assert.IsTrue(comparer.AllWaists[1].IsSelected);

            Assert.Equals(2, comparer.AllLegs.Count());
            Assert.IsTrue(comparer.AllLegs[0].IsSelected);
            Assert.IsTrue(comparer.AllLegs[1].IsSelected);
        }

        [TestMethod()]
        public void SetupEqual_Level4MultiSkillDecoWithOtherSearchedSkill_KeepBetterSlots_Test()
        {
            var comparer = new ComparerSolverData();
            comparer.IfEqualUseArmorWithBetterSlots = true;

            IEnumerable<IArmorPiece> CreateArmors(EquipmentType type)
            {
                return new List<IArmorPiece>()
                {
                    CreateArmor(0, type, 12, new int[] {2}, new IAbility[] {CreateAbility(CreateSkill(1), 1), CreateAbility(CreateSkill(0), 1)}, null),
                    CreateArmor(1, type, 12, new int[] {4}, new IAbility[] {CreateAbility(CreateSkill(1), 1)}, null)
                };
            }

            IEnumerable<IArmorPiece> heads = CreateArmors(EquipmentType.Head);
            IEnumerable<IArmorPiece> chests = CreateArmors(EquipmentType.Chest);
            IEnumerable<IArmorPiece> arms = CreateArmors(EquipmentType.Gloves);
            IEnumerable<IArmorPiece> waist = CreateArmors(EquipmentType.Waist);
            IEnumerable<IArmorPiece> legs = CreateArmors(EquipmentType.Legs);
            var charms = new List<ICharmLevel>();
            var decos = new List<SolverDataJewelModel>
            {
                CreateDeco(0, 2, new IAbility[]{CreateAbility(CreateSkill(0), 1) }, 7),
                CreateDeco(1, 2, new IAbility[]{CreateAbility(CreateSkill(1), 1) }, 7),
                CreateDeco(2, 4, new IAbility[]{CreateAbility(CreateSkill(0), 1), CreateAbility(CreateSkill(1), 1) }, 7)
            };

            comparer.Setup(CreateWeapon(), heads, chests, arms, waist, legs, charms, decos, new IAbility[] { CreateAbility(CreateSkill(0), 7), CreateAbility(CreateSkill(1), 7) });

            Assert.Equals(2, comparer.AllHeads.Count());
            Assert.IsFalse(comparer.AllHeads[0].IsSelected);
            Assert.IsTrue(comparer.AllHeads[1].IsSelected);

            Assert.Equals(2, comparer.AllChests.Count());
            Assert.IsFalse(comparer.AllChests[0].IsSelected);
            Assert.IsTrue(comparer.AllChests[1].IsSelected);

            Assert.Equals(2, comparer.AllGloves.Count());
            Assert.IsFalse(comparer.AllGloves[0].IsSelected);
            Assert.IsTrue(comparer.AllGloves[1].IsSelected);

            Assert.Equals(2, comparer.AllWaists.Count());
            Assert.IsFalse(comparer.AllWaists[0].IsSelected);
            Assert.IsTrue(comparer.AllWaists[1].IsSelected);

            Assert.Equals(2, comparer.AllLegs.Count());
            Assert.IsFalse(comparer.AllLegs[0].IsSelected);
            Assert.IsTrue(comparer.AllLegs[1].IsSelected);
        }

        [TestMethod()]
        public void SetupComparable_Level4MultiSkillDecoWithOtherUnsearchedSkill_Test()
        {
            var comparer = new ComparerSolverData();
            comparer.IfEqualUseArmorWithBetterSlots = false;

            IEnumerable<IArmorPiece> CreateArmors(EquipmentType type)
            {
                return new List<IArmorPiece>()
                {
                    CreateArmor(0, type, 12, new int[] {2}, new IAbility[] {CreateAbility(CreateSkill(1), 1), CreateAbility(CreateSkill(0), 1)}, null),
                    CreateArmor(1, type, 12, new int[] {4}, new IAbility[] {CreateAbility(CreateSkill(1), 1)}, null)
                };
            }

            IEnumerable<IArmorPiece> heads = CreateArmors(EquipmentType.Head);
            IEnumerable<IArmorPiece> chests = CreateArmors(EquipmentType.Chest);
            IEnumerable<IArmorPiece> arms = CreateArmors(EquipmentType.Gloves);
            IEnumerable<IArmorPiece> waist = CreateArmors(EquipmentType.Waist);
            IEnumerable<IArmorPiece> legs = CreateArmors(EquipmentType.Legs);
            var charms = new List<ICharmLevel>();
            var decos = new List<SolverDataJewelModel>
            {
                CreateDeco(0, 2, new IAbility[]{CreateAbility(CreateSkill(0), 1) }, 7),
                CreateDeco(1, 2, new IAbility[]{CreateAbility(CreateSkill(1), 1) }, 7),
                CreateDeco(2, 4, new IAbility[]{CreateAbility(CreateSkill(0), 1), CreateAbility(CreateSkill(2), 1) }, 7)
            };

            comparer.Setup(CreateWeapon(), heads, chests, arms, waist, legs, charms, decos, new IAbility[] { CreateAbility(CreateSkill(0), 7), CreateAbility(CreateSkill(1), 7) });

            Assert.Equals(2, comparer.AllHeads.Count());
            Assert.IsTrue(comparer.AllHeads[0].IsSelected);
            Assert.IsFalse(comparer.AllHeads[1].IsSelected);

            Assert.Equals(2, comparer.AllChests.Count());
            Assert.IsTrue(comparer.AllChests[0].IsSelected);
            Assert.IsFalse(comparer.AllChests[1].IsSelected);

            Assert.Equals(2, comparer.AllGloves.Count());
            Assert.IsTrue(comparer.AllGloves[0].IsSelected);
            Assert.IsFalse(comparer.AllGloves[1].IsSelected);

            Assert.Equals(2, comparer.AllWaists.Count());
            Assert.IsTrue(comparer.AllWaists[0].IsSelected);
            Assert.IsFalse(comparer.AllWaists[1].IsSelected);

            Assert.Equals(2, comparer.AllLegs.Count());
            Assert.IsTrue(comparer.AllLegs[0].IsSelected);
            Assert.IsFalse(comparer.AllLegs[1].IsSelected);
        }

        [TestMethod()]
        public void SetupComparable_WorseButWithSetBonus_Test()
        {
            var comparer = new ComparerSolverData();
            comparer.IfEqualUseArmorWithBetterSlots = false;

            IEnumerable<IArmorPiece> CreateArmors(EquipmentType type)
            {
                return new List<IArmorPiece>()
                {
                    CreateArmor(0, type, 12, new int[] {}, new IAbility[] {CreateAbility(CreateSkill(0), 1)}, null),
                    CreateArmor(1, type, 12, new int[] {}, new IAbility[] {CreateAbility(CreateSkill(2), 1)}, new IArmorSetSkill[] { CreateArmorSetSkill(0, 1) }),
                };
            }

            IEnumerable<IArmorPiece> heads = CreateArmors(EquipmentType.Head);
            IEnumerable<IArmorPiece> chests = CreateArmors(EquipmentType.Chest);
            IEnumerable<IArmorPiece> arms = CreateArmors(EquipmentType.Gloves);
            IEnumerable<IArmorPiece> waist = CreateArmors(EquipmentType.Waist);
            IEnumerable<IArmorPiece> legs = CreateArmors(EquipmentType.Legs);
            var charms = new List<ICharmLevel>();
            var decos = new List<SolverDataJewelModel> ();

            comparer.Setup(CreateWeapon(), heads, chests, arms, waist, legs, charms, decos, new IAbility[] { CreateAbility(CreateSkill(0), 7), CreateAbility(CreateSkill(1), 7) });

            Assert.Equals(2, comparer.AllHeads.Count());
            Assert.IsTrue(comparer.AllHeads[0].IsSelected);
            Assert.IsTrue(comparer.AllHeads[1].IsSelected);

            Assert.Equals(2, comparer.AllChests.Count());
            Assert.IsTrue(comparer.AllChests[0].IsSelected);
            Assert.IsTrue(comparer.AllChests[1].IsSelected);

            Assert.Equals(2, comparer.AllGloves.Count());
            Assert.IsTrue(comparer.AllGloves[0].IsSelected);
            Assert.IsTrue(comparer.AllGloves[1].IsSelected);

            Assert.Equals(2, comparer.AllWaists.Count());
            Assert.IsTrue(comparer.AllWaists[0].IsSelected);
            Assert.IsTrue(comparer.AllWaists[1].IsSelected);

            Assert.Equals(2, comparer.AllLegs.Count());
            Assert.IsTrue(comparer.AllLegs[0].IsSelected);
            Assert.IsTrue(comparer.AllLegs[1].IsSelected);
        }

        [TestMethod()]
        public void SetupComparable_2WorseButWithSetBonus_1betterThenOther_Test()
        {
            var comparer = new ComparerSolverData();
            comparer.IfEqualUseArmorWithBetterSlots = false;

            IEnumerable<IArmorPiece> CreateArmors(EquipmentType type)
            {
                return new List<IArmorPiece>()
                {
                    CreateArmor(0, type, 12, new int[] {}, new IAbility[] {CreateAbility(CreateSkill(0), 2)}, null),
                    CreateArmor(1, type, 12, new int[] {}, new IAbility[] {CreateAbility(CreateSkill(2), 1)}, new IArmorSetSkill[] { CreateArmorSetSkill(0, 1) }),
                    CreateArmor(1, type, 12, new int[] {}, new IAbility[] {CreateAbility(CreateSkill(0), 1)}, new IArmorSetSkill[] { CreateArmorSetSkill(0, 1) }),
                };
            }

            IEnumerable<IArmorPiece> heads = CreateArmors(EquipmentType.Head);
            IEnumerable<IArmorPiece> chests = CreateArmors(EquipmentType.Chest);
            IEnumerable<IArmorPiece> arms = CreateArmors(EquipmentType.Gloves);
            IEnumerable<IArmorPiece> waist = CreateArmors(EquipmentType.Waist);
            IEnumerable<IArmorPiece> legs = CreateArmors(EquipmentType.Legs);
            var charms = new List<ICharmLevel>();
            var decos = new List<SolverDataJewelModel>();

            comparer.Setup(CreateWeapon(), heads, chests, arms, waist, legs, charms, decos, new IAbility[] { CreateAbility(CreateSkill(0), 7), CreateAbility(CreateSkill(1), 7) });

            Assert.Equals(3, comparer.AllHeads.Count());
            Assert.IsTrue(comparer.AllHeads[0].IsSelected);
            Assert.IsFalse(comparer.AllHeads[1].IsSelected);
            Assert.IsTrue(comparer.AllHeads[2].IsSelected);

            Assert.Equals(3, comparer.AllChests.Count());
            Assert.IsTrue(comparer.AllChests[0].IsSelected);
            Assert.IsFalse(comparer.AllChests[1].IsSelected);
            Assert.IsTrue(comparer.AllChests[2].IsSelected);

            Assert.Equals(3, comparer.AllGloves.Count());
            Assert.IsTrue(comparer.AllGloves[0].IsSelected);
            Assert.IsFalse(comparer.AllGloves[1].IsSelected);
            Assert.IsTrue(comparer.AllGloves[2].IsSelected);

            Assert.Equals(3, comparer.AllWaists.Count());
            Assert.IsTrue(comparer.AllWaists[0].IsSelected);
            Assert.IsFalse(comparer.AllWaists[1].IsSelected);
            Assert.IsTrue(comparer.AllWaists[2].IsSelected);

            Assert.Equals(3, comparer.AllLegs.Count());
            Assert.IsTrue(comparer.AllLegs[0].IsSelected);
            Assert.IsFalse(comparer.AllLegs[1].IsSelected);
            Assert.IsTrue(comparer.AllLegs[2].IsSelected);
        }

        [TestMethod()]
        public void SetupComparable_2WorseButWithSetBonus_differentSets_Test()
        {
            var comparer = new ComparerSolverData();
            comparer.IfEqualUseArmorWithBetterSlots = false;

            IEnumerable<IArmorPiece> CreateArmors(EquipmentType type)
            {
                return new List<IArmorPiece>()
                {
                    CreateArmor(0, type, 12, new int[] {}, new IAbility[] {CreateAbility(CreateSkill(0), 2)}, null),
                    CreateArmor(1, type, 12, new int[] {}, new IAbility[] {CreateAbility(CreateSkill(2), 1)}, new IArmorSetSkill[] { CreateArmorSetSkill(0, 1) }),
                    CreateArmor(1, type, 12, new int[] {}, new IAbility[] {CreateAbility(CreateSkill(0), 1)}, new IArmorSetSkill[] { CreateArmorSetSkill(1, 1) }),
                };
            }

            IEnumerable<IArmorPiece> heads = CreateArmors(EquipmentType.Head);
            IEnumerable<IArmorPiece> chests = CreateArmors(EquipmentType.Chest);
            IEnumerable<IArmorPiece> arms = CreateArmors(EquipmentType.Gloves);
            IEnumerable<IArmorPiece> waist = CreateArmors(EquipmentType.Waist);
            IEnumerable<IArmorPiece> legs = CreateArmors(EquipmentType.Legs);
            var charms = new List<ICharmLevel>();
            var decos = new List<SolverDataJewelModel>();

            comparer.Setup(CreateWeapon(), heads, chests, arms, waist, legs, charms, decos, new IAbility[] { CreateAbility(CreateSkill(0), 7), CreateAbility(CreateSkill(1), 7) });

            Assert.Equals(3, comparer.AllHeads.Count());
            Assert.IsTrue(comparer.AllHeads[0].IsSelected);
            Assert.IsTrue(comparer.AllHeads[1].IsSelected);
            Assert.IsTrue(comparer.AllHeads[2].IsSelected);

            Assert.Equals(3, comparer.AllChests.Count());
            Assert.IsTrue(comparer.AllChests[0].IsSelected);
            Assert.IsTrue(comparer.AllChests[1].IsSelected);
            Assert.IsTrue(comparer.AllChests[2].IsSelected);

            Assert.Equals(3, comparer.AllGloves.Count());
            Assert.IsTrue(comparer.AllGloves[0].IsSelected);
            Assert.IsTrue(comparer.AllGloves[1].IsSelected);
            Assert.IsTrue(comparer.AllGloves[2].IsSelected);

            Assert.Equals(3, comparer.AllWaists.Count());
            Assert.IsTrue(comparer.AllWaists[0].IsSelected);
            Assert.IsTrue(comparer.AllWaists[1].IsSelected);
            Assert.IsTrue(comparer.AllWaists[2].IsSelected);

            Assert.Equals(3, comparer.AllLegs.Count());
            Assert.IsTrue(comparer.AllLegs[0].IsSelected);
            Assert.IsTrue(comparer.AllLegs[1].IsSelected);
            Assert.IsTrue(comparer.AllLegs[2].IsSelected);
        }
    }
}