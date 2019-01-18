using MHArmory.Configurations;
using MHArmory.Core.DataStructures;
using MHArmory.Search.Default;
using MHArmory.Search.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MHArmory.ViewModels
{
    public class ExtensionViewModel : ViewModelBase
    {
        public IExtension Extension { get; }

        private bool isActive;
        public bool IsActive
        {
            get { return isActive; }
            set
            {
                if (SetValue(ref isActive, value))
                    parent.OnExtensionSelectionChanged(this);
            }
        }

        public bool IsConfigurable { get; }
        public ICommand ConfigureCommand { get; }

        private readonly ExtensionCategoryViewModelBase parent;

        public ExtensionViewModel(ExtensionCategoryViewModelBase parent, IExtension extension)
        {
            this.parent = parent;
            Extension = extension;

            if (extension is IConfigurable configurable)
            {
                IsConfigurable = true;
                ConfigureCommand = new AnonymousCommand(configurable.Configure);
            }
        }

        public override string ToString()
        {
            return $"{Extension.Name} [{Extension.Description}]";
        }
    }

    public enum ExtensionCategory
    {
        SolverData,
        Solver,
    }

    public abstract class ExtensionCategoryViewModelBase : ViewModelBase
    {
        public string Name { get; }
        public ExtensionCategory Category { get; }
        public IList<ExtensionViewModel> Extensions { get; }

        protected ExtensionCategoryViewModelBase(
            string name,
            ExtensionCategory category,
            IList<IExtension> extenions
        )
        {
            if (extenions == null)
                throw new ArgumentNullException(nameof(extenions));
            if (extenions.Count(x => x != null) == 0)
                throw new ArgumentException("At least one extension is required", nameof(extenions));

            for (int i = 0; i < extenions.Count; i++)
            {
                for (int j = i + 1; j < extenions.Count; j++)
                {
                    if (extenions[i].Name == extenions[j].Name)
                        throw new ArgumentException($"Extension with name '{extenions[i].Name}' is duplicated", nameof(extenions));
                }
            }

            Name = name;
            Category = category;
            Extensions = extenions
                .Where(x => x != null)
                .Select(x => new ExtensionViewModel(this, x))
                .ToList();
        }

        protected internal abstract void OnExtensionSelectionChanged(ExtensionViewModel extension);
    }

    public class SingleSelectionExtensionCategoryViewModel : ExtensionCategoryViewModelBase
    {
        public delegate void SingleSelectionExtensionSelectionChangedHandler(SingleSelectionExtensionCategoryViewModel category, ExtensionViewModel oldExtension, ExtensionViewModel newExtension);

        private ExtensionViewModel selected;
        public ExtensionViewModel Selected
        {
            get { return selected; }
            set
            {
                if (value == null)
                    throw new ArgumentException(nameof(Selected));

                ExtensionViewModel oldValue = selected;

                if (SetValue(ref selected, value))
                {
                    if (oldValue != null)
                        oldValue.IsActive = false;
                    selected.IsActive = true;
                }
            }
        }

        private readonly SingleSelectionExtensionSelectionChangedHandler onSelectionChanged;

        public SingleSelectionExtensionCategoryViewModel(
            string name,
            ExtensionCategory category,
            IList<IExtension> extenions,
            string selectedExtensionName,
            SingleSelectionExtensionSelectionChangedHandler onSelectionChanged
        ) : base(name, category, extenions)
        {
            this.onSelectionChanged = onSelectionChanged;

            bool hasSelection = false;

            foreach (ExtensionViewModel ext in Extensions)
            {
                if (ext.Extension.Name == selectedExtensionName)
                {
                    ext.IsActive = true;
                    hasSelection = true;
                }
            }

            if (hasSelection == false)
                Extensions[0].IsActive = true;
        }

        protected internal override void OnExtensionSelectionChanged(ExtensionViewModel extension)
        {
            if (extension.IsActive == false)
                return;

            ExtensionViewModel oldExtension = Selected;
            Selected = extension;

            onSelectionChanged(this, oldExtension, extension);
        }
    }

    public class MultiSelectionExtensionCategoryViewModel : ExtensionCategoryViewModelBase
    {
        public delegate void MultiSelectionExtensionSelectionChangedHandler(MultiSelectionExtensionCategoryViewModel category, ExtensionViewModel extension);

        private readonly MultiSelectionExtensionSelectionChangedHandler onSelectionChanged;

        public MultiSelectionExtensionCategoryViewModel(
            string name,
            ExtensionCategory category,
            IList<IExtension> extenions,
            IEnumerable<string> selectedExtensionNames,
            MultiSelectionExtensionSelectionChangedHandler onSelectionChanged
        ) : base(name, category, extenions)
        {
            this.onSelectionChanged = onSelectionChanged;

            bool hasSelection = false;

            foreach (string extName in selectedExtensionNames.Distinct())
            {
                foreach (ExtensionViewModel ext in Extensions)
                {
                    if (ext.Extension.Name == extName)
                    {
                        ext.IsActive = true;
                        hasSelection = true;
                    }
                }
            }

            if (hasSelection == false)
                Extensions[0].IsActive = true;
        }

        protected internal override void OnExtensionSelectionChanged(ExtensionViewModel extension)
        {
            onSelectionChanged(this, extension);
        }
    }

    public class ExtensionSelectorViewModel : ViewModelBase
    {
        public ExtensionCategoryViewModelBase[] Categories { get; }

        private readonly RootViewModel root;

        public ExtensionSelectorViewModel(RootViewModel root)
        {
            this.root = root;

            Categories = new ExtensionCategoryViewModelBase[]
            {
                new SingleSelectionExtensionCategoryViewModel(
                    nameof(SolverData),
                    ExtensionCategory.SolverData,
                    AvailableExtensions.SolverData,
                    GlobalData.Instance.Configuration.Extensions.SolverData,
                    OnSolverDataChanged
                ),
                new SingleSelectionExtensionCategoryViewModel(
                    nameof(Solver),
                    ExtensionCategory.Solver,
                    AvailableExtensions.Solvers,
                    GlobalData.Instance.Configuration.Extensions.Solver,
                    OnSolverChanged
                ),
            };
        }

        private void OnSolverDataChanged(ExtensionCategoryViewModelBase category, ExtensionViewModel oldExtension, ExtensionViewModel newExtension)
        {
            root.CreateSolverData();

            GlobalData.Instance.Configuration.Extensions.SolverData = newExtension.Extension.Name;
            ConfigurationManager.Save(GlobalData.Instance.Configuration);
        }

        private void OnSolverChanged(ExtensionCategoryViewModelBase category, ExtensionViewModel oldExtension, ExtensionViewModel newExtension)
        {
            var oldValue = oldExtension?.Extension as ISolver;
            var newValue = newExtension?.Extension as ISolver;

            if (oldValue != null)
                oldValue.SearchProgress -= SolverSearchProgress;

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
}
