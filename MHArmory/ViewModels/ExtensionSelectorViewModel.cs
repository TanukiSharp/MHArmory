using MHArmory.Configurations;
using MHArmory.Core.DataStructures;
using MHArmory.Search;
using MHArmory.Search.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MHArmory.ViewModels
{
    public class ExtensionSelectorViewModel : ViewModelBase
    {
        private static readonly ISolverData[] availableSolverData = new ISolverData[]
        {
            new SolverData(),
            new IncrementalSolverData(), 
            //new TestSolverData(),
        };

        private static readonly ISolver[] availableSolvers = new ISolver[]
        {
            new Solver(),
            //new TestSolver(),
        };

        public ExtensionSelector<ISolverData> SolverData { get; }
        public ExtensionSelector<ISolver> Solver { get; }

        private readonly RootViewModel root;

        public ExtensionSelectorViewModel(RootViewModel root)
        {
            this.root = root;

            SolverData = new ExtensionSelector<ISolverData>(
                nameof(SolverData),
                availableSolverData,
                GlobalData.Instance.Configuration.Extensions.SolverData,
                OnSolverDataChanged
            );

            Solver = new ExtensionSelector<ISolver>(
                nameof(Solver),
                availableSolvers,
                GlobalData.Instance.Configuration.Extensions.Solver,
                OnSolverChanged
            );
        }

        private void OnSolverDataChanged(ISolverData oldValue, ISolverData newValue)
        {
            root.CreateSolverData();

            GlobalData.Instance.Configuration.Extensions.SolverData = newValue.Name;
            ConfigurationManager.Save(GlobalData.Instance.Configuration);

            if (oldValue is IDisposable disposable)
                disposable.Dispose();
        }

        private void OnSolverChanged(ISolver oldValue, ISolver newValue)
        {
            if (oldValue != null)
            {
                oldValue.SearchProgress -= SolverSearchProgress;

                if (oldValue is IDisposable disposable)
                    disposable.Dispose();
            }

            if (newValue != null)
                newValue.SearchProgress += SolverSearchProgress;

            GlobalData.Instance.Configuration.Extensions.Solver = newValue.Name;
            ConfigurationManager.Save(GlobalData.Instance.Configuration);
        }

        private void SolverSearchProgress(double progressRatio)
        {
            root.SearchProgression = progressRatio;
        }
    }

    // ==========================================================================================================

    public class ExtensionSelector<T> : Selector<T> where T : class, IExtension
    {
        public ExtensionSelector(string propertyName, IList<T> availableValues, string selectedExtensionName, Action<T, T> onChanged)
            : base(propertyName, availableValues, FindExtensionIndex(availableValues, selectedExtensionName), onChanged)
        {
            for (int i = 0; i < availableValues.Count; i++)
            {
                for (int j = i + 1; j < availableValues.Count; j++)
                {
                    if (availableValues[i].Name == availableValues[j].Name)
                        throw new ArgumentException($"Invalid '{nameof(availableValues)}' argument, '{availableValues[i].Name}' is duplicated", nameof(availableValues));
                }
            }
        }

        private static int FindExtensionIndex(IList<T> extensions, string name)
        {
            for (int i = 0; i < extensions.Count; i++)
            {
                if (extensions[i].Name == name)
                    return i;
            }

            return 0;
        }

        public static implicit operator T(ExtensionSelector<T> selector)
        {
            return selector.SelectedValue;
        }
    }

    public class Selector<T> : ViewModelBase where T : class
    {
        private readonly string propertyName;
        private readonly Action<T, T> onChanged;

        public IList<T> AvailableValues { get; }

        private T selectedValue;
        public T SelectedValue
        {
            get { return selectedValue; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(propertyName, $"Null value is illegal for '{propertyName}'");

                T oldValue = selectedValue;
                if (SetValue(ref selectedValue, value))
                    onChanged?.Invoke(oldValue, selectedValue);
            }
        }

        public static implicit operator T(Selector<T> selector)
        {
            return selector.SelectedValue;
        }

        public Selector(string propertyName, IList<T> availableValues, int selectedIndex, Action<T, T> onChanged)
        {
            if (availableValues == null)
                throw new ArgumentNullException(nameof(availableValues));

            if (availableValues.Count == 0)
                throw new ArgumentException($"{nameof(availableValues)} is invalid", nameof(availableValues));

            if (selectedIndex < 0 || selectedIndex >= availableValues.Count)
                throw new ArgumentException($"{nameof(selectedIndex)} ({selectedIndex}) is invalid", nameof(selectedIndex));

            this.propertyName = propertyName;
            this.onChanged = onChanged;

            AvailableValues = availableValues;

            SelectedValue = availableValues[selectedIndex];
        }
    }

    /*
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

    public class TestSolver : ISolver
    {
        public string Name { get; } = "Armory - Test Solver";
        public string Author { get; } = "TanukiSharp";
        public string Description { get; } = "Test solver data with very very long description or maybe a bit too long description";
        public int Version { get; } = 37;

        public event Action<double> SearchProgress = delegate { };

        public Task<IList<ArmorSetSearchResult>> SearchArmorSets(ISolverData solverData, CancellationToken cancellationToken)
        {
            return Task.FromResult<IList<ArmorSetSearchResult>>(new ArmorSetSearchResult[0]);
        }
    }
    */
}
