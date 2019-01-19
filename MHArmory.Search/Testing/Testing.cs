using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MHArmory.Core.DataStructures;
using MHArmory.Search.Contracts;

namespace MHArmory.Search.Testing
{
    public class TestSolverData : ISolverData
    {
        public int[] WeaponSlots { get; private set; }

        public ISolverDataEquipmentModel[] AllHeads { get; private set; }
        public ISolverDataEquipmentModel[] AllChests { get; private set; }
        public ISolverDataEquipmentModel[] AllGloves { get; private set; }
        public ISolverDataEquipmentModel[] AllWaists { get; private set; }
        public ISolverDataEquipmentModel[] AllLegs { get; private set; }
        public ISolverDataEquipmentModel[] AllCharms { get; private set; }

        public SolverDataJewelModel[] AllJewels { get; private set; }

        public IAbility[] DesiredAbilities { get; private set; }

        public string Name { get; } = "Armory - Test SolverData";
        public string Author { get; } = "TanukiSharp Blablabla long name";
        public string Description { get; } = "Test solver search algorithm";
        public int Version { get; } = 51;

        public void Setup(IList<int> weaponSlots, IEnumerable<IArmorPiece> heads, IEnumerable<IArmorPiece> chests, IEnumerable<IArmorPiece> gloves, IEnumerable<IArmorPiece> waists, IEnumerable<IArmorPiece> legs, IEnumerable<ICharmLevel> charms, IEnumerable<SolverDataJewelModel> jewels, IEnumerable<IAbility> desiredAbilities)
        {
            WeaponSlots = weaponSlots.ToArray();

            AllHeads = heads.Select(x => new SolverDataEquipmentModel(x)).Take(6).ToArray();
            AllChests = chests.Select(x => new SolverDataEquipmentModel(x)).Take(6).ToArray();
            AllGloves = gloves.Select(x => new SolverDataEquipmentModel(x)).Take(6).ToArray();
            AllWaists = waists.Select(x => new SolverDataEquipmentModel(x)).Take(6).ToArray();
            AllLegs = legs.Select(x => new SolverDataEquipmentModel(x)).Take(6).ToArray();
            AllCharms = charms.Select(x => new SolverDataEquipmentModel(x)).Take(6).ToArray();

            AllJewels = jewels.Take(32).ToArray();

            DesiredAbilities = desiredAbilities.ToArray();
        }
    }

    public class TestSolver : ISolver, IConfigurable
    {
        public string Name { get; } = "Armory - Test Solver";
        public string Author { get; } = "TanukiSharp";
        public string Description { get; } = "Test solver data with very very long description or maybe a bit too long description";
        public int Version { get; } = 37;

        public event Action<double> SearchProgress = delegate { };

        public void Configure()
        {
            var window = new SampleExtensionConfigurationWindow(this);
            window.ShowDialog();
        }

        public Task<IList<ArmorSetSearchResult>> SearchArmorSets(ISolverData solverData, CancellationToken cancellationToken)
        {
            return Task.FromResult<IList<ArmorSetSearchResult>>(new ArmorSetSearchResult[0]);
        }
    }
}
