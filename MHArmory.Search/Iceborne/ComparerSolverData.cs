using MHArmory.Core.DataStructures;
using MHArmory.Search.Contracts;
using MHArmory.Search.Default;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHArmory.Search.Iceborne
{
    public class ComparerSolverData : ISolverData
    {
        public IEquipment Weapon { get; private set; }

        public ISolverDataEquipmentModel[] AllHeads { get; private set; }

        public ISolverDataEquipmentModel[] AllChests { get; private set; }

        public ISolverDataEquipmentModel[] AllGloves { get; private set; }

        public ISolverDataEquipmentModel[] AllWaists { get; private set; }

        public ISolverDataEquipmentModel[] AllLegs { get; private set; }

        public ISolverDataEquipmentModel[] AllCharms { get; private set; }

        public SolverDataJewelModel[] AllJewels { get; private set; }

        public IAbility[] DesiredAbilities { get; private set; }

        public string Name { get; } = "Comparer Solver Data";

        public string Author { get; } = "ChaosSaber";

        public string Description { get; } = "Solver Data which compares every equipment against each other and unselects every equipment which is worse then another";

        public int Version { get; } = 1;



        public bool IncludeLowerTier { get; set; }
        public bool IfEqualUseArmorWithBetterSlots { get; set; }



        private List<SolverDataEquipmentModel> inputHeads;
        private List<SolverDataEquipmentModel> inputChests;
        private List<SolverDataEquipmentModel> inputGloves;
        private List<SolverDataEquipmentModel> inputWaists;
        private List<SolverDataEquipmentModel> inputLegs;
        private List<SolverDataEquipmentModel> inputCharms;
        private List<SolverDataJewelModel> inputDecos;

        //struct SkillDecoInfo
        //{
        //    bool decoExists;
        //    int slotSize;
        //    bool hasLevel4Deco;
        //    int maxSkillPointsPerDeco;
        //    bool combiDecoWithOtherDesiredSkill;
        //}
        //private Dictionary<ISkill, SkillDecoInfo> decos = new Dictionary<ISkill, SkillDecoInfo>();


        enum HunterRank { LowRank, HighRank, MasterRank };
        //private HunterRank hunterRank;

        public void Setup(IEquipment weapon, IEnumerable<IArmorPiece> heads, IEnumerable<IArmorPiece> chests, IEnumerable<IArmorPiece> gloves, IEnumerable<IArmorPiece> waists, IEnumerable<IArmorPiece> legs, IEnumerable<ICharmLevel> charms, IEnumerable<SolverDataJewelModel> jewels, IEnumerable<IAbility> desiredAbilities)
        {
            Weapon = weapon;
            inputHeads = heads.Select(x => new SolverDataEquipmentModel(x)).ToList();
            inputChests = chests.Select(x => new SolverDataEquipmentModel(x)).ToList();
            inputGloves = gloves.Select(x => new SolverDataEquipmentModel(x)).ToList();
            inputWaists = waists.Select(x => new SolverDataEquipmentModel(x)).ToList();
            inputLegs = legs.Select(x => new SolverDataEquipmentModel(x)).ToList();
            inputCharms = charms.Select(x => new SolverDataEquipmentModel(x)).ToList();
            inputDecos = jewels.ToList();
            DesiredAbilities = desiredAbilities.ToArray();


            //int maxRarity = inputHeads.MaxOrZero(x => x.Equipment.Rarity);
            //maxRarity = Math.Max(maxRarity, inputChests.MaxOrZero(x => x.Equipment.Rarity));
            //maxRarity = Math.Max(maxRarity, inputGloves.MaxOrZero(x => x.Equipment.Rarity));
            //maxRarity = Math.Max(maxRarity, inputWaists.MaxOrZero(x => x.Equipment.Rarity));
            //maxRarity = Math.Max(maxRarity, inputLegs.MaxOrZero(x => x.Equipment.Rarity));
            //if (maxRarity > 8)
            //    hunterRank = HunterRank.MasterRank;
            //else if (maxRarity > 4)
            //    hunterRank = HunterRank.HighRank;
            //else
            //    hunterRank = HunterRank.LowRank;



            CreateDecoMap();
            FilterDecos();
            UpdateDecoMap();

        }

        void CreateDecoMap()
        {

        }

        void FilterDecos()
        {

        }

        void UpdateDecoMap()
        {

        }
    }
}
