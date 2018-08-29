﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MHArmory.Configurations;

namespace MHArmory.ViewModels
{
    public class SearchResultProcessingViewModel : ViewModelBase
    {
        public ICommand CreateNewCommand { get; }

        private bool isEditMode;
        public bool IsEditMode
        {
            get { return isEditMode; }
            set
            {
                if (SetValue(ref isEditMode, value))
                    EditModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler EditModeChanged;

        private bool hasSelection;
        public bool HasSelection
        {
            get { return hasSelection; }
            private set { SetValue(ref hasSelection, value); }
        }

        private SearchResultProcessingContainerViewModel selectedContainer;
        public SearchResultProcessingContainerViewModel SelectedContainer
        {
            get { return selectedContainer; }
            set
            {
                if (SetValue(ref selectedContainer, value))
                    HasSelection = selectedContainer != null;
            }
        }

        private bool hasContainers;
        public bool HasContainers
        {
            get { return hasContainers; }
            private set { SetValue(ref hasContainers, value); }
        }

        public ObservableCollection<SearchResultProcessingContainerViewModel> Containers { get; } = new ObservableCollection<SearchResultProcessingContainerViewModel>();

        private readonly RootViewModel rootViewModel;

        public SearchResultProcessingViewModel(RootViewModel rootViewModel)
        {
            this.rootViewModel = rootViewModel;

            Containers.CollectionChanged += Containers_CollectionChanged;

            CreateNewCommand = new AnonymousCommand(OnCreateNew);
        }

        internal void NotifyConfigurationLoaded()
        {
            LoadConfiguration();
        }

        internal void ActiveContainerChanged()
        {
            rootViewModel.ApplySorting();
        }

        private void Containers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            HasContainers = Containers.Count > 0;
            if (HasContainers == false)
                IsEditMode = false;
        }

        private void LoadConfiguration()
        {
            SearchResultProcessingConfiguration config = GlobalData.Instance.Configuration.SearchResultProcessing;

            if (config.Sorting != null)
            {
                Containers.Clear();
                foreach (SearchResultSortItemConfiguration x in config.Sorting)
                {
                    var container = new SearchResultProcessingContainerViewModel(this)
                    {
                        Name = x.Name
                    };

                    foreach (SearchResultSortCriteria criteria in x.Criterias)
                        container.SortItems.Add(new SearchResultSortItemViewModel(container) { SortCriteria = criteria });

                    Containers.Add(container);
                }

                if (config.ActiveSortingIndex >= 0 && config.ActiveSortingIndex < Containers.Count)
                    MakeContainerActive(Containers[config.ActiveSortingIndex]);
            }
        }

        public void SaveConfiguration()
        {
            SearchResultProcessingConfiguration config = GlobalData.Instance.Configuration.SearchResultProcessing;

            config.Sorting = Containers
                .Select(x => new SearchResultSortItemConfiguration
                {
                    Name = x.Name,
                    Criterias = x.SortItems.Select(s => s.SortCriteria).ToArray()
                })
                .ToArray();

            int index = -1;
            for (int i = 0; i < Containers.Count; i++)
            {
                if (Containers[i].IsActive)
                {
                    index = i;
                    break;
                }
            }
            config.ActiveSortingIndex = index;

            ConfigurationManager.Save(GlobalData.Instance.Configuration);
        }

        public bool ApplySort(ref IEnumerable<ArmorSetViewModel> input)
        {
            if (input == null || input.Any() == false)
                return false;

            SearchResultProcessingContainerViewModel activeContainer = Containers.FirstOrDefault(x => x.IsActive);
            if (activeContainer == null || activeContainer.SortItems.Count == 0)
                return false;

            IOrderedEnumerable<ArmorSetViewModel> result = input.OrderBy(x => 1); // wasting a bit of CPU cycles for productivity purpose :(

            foreach (SearchResultSortCriteria sortCriteria in activeContainer.SortItems.Select(x => x.SortCriteria))
            {
                switch (sortCriteria)
                {
                    case SearchResultSortCriteria.BaseDefense:
                        result = result.ThenByDescending(x => x.TotalBaseDefense);
                        break;
                    case SearchResultSortCriteria.MaxUnaugmentedDefense:
                        result = result.ThenByDescending(x => x.TotalMaxDefense);
                        break;
                    case SearchResultSortCriteria.MaxAugmentedDefense:
                        result = result.ThenByDescending(x => x.TotalAugmentedDefense);
                        break;
                    case SearchResultSortCriteria.AverageRarity:
                        result = result.ThenBy(x => x.ArmorPieces.Average(a => (float)a.Rarity));
                        break;
                    case SearchResultSortCriteria.HighestRarity:
                        result = result.ThenBy(x => x.ArmorPieces.Max(a => a.Rarity));
                        break;
                    case SearchResultSortCriteria.TotalRarity:
                        result = result.ThenBy(x => x.TotalRarity);
                        break;
                    case SearchResultSortCriteria.SpareSlotCount:
                        result = result.ThenByDescending(x => x.SpareSlotCount);
                        break;
                    case SearchResultSortCriteria.SpareSlotSizeSquare:
                        result = result
                            .ThenByDescending(x => x.SpareSlotSizeSquare)
                            .ThenByDescending(x => x.SpareSlots.Count(s => s > 0)); // For same score, gives precedence to slot count. (only 221 and 3--, sets 221 before 3--)
                        break;
                    case SearchResultSortCriteria.SpareSlotSizeCube:
                        result = result.ThenByDescending(x => x.SpareSlotSizeCube);
                        break;
                    case SearchResultSortCriteria.FireResistance:
                        result = result.ThenByDescending(x => x.TotalFireResistance);
                        break;
                    case SearchResultSortCriteria.WaterResistance:
                        result = result.ThenByDescending(x => x.TotalWaterResistance);
                        break;
                    case SearchResultSortCriteria.ThunderResistance:
                        result = result.ThenByDescending(x => x.TotalThunderResistance);
                        break;
                    case SearchResultSortCriteria.IceResistance:
                        result = result.ThenByDescending(x => x.TotalIceResistance);
                        break;
                    case SearchResultSortCriteria.DragonResistance:
                        result = result.ThenByDescending(x => x.TotalDragonResistance);
                        break;
                }
            }

            input = result.ToList();

            return true;
        }

        public bool MoveContainerUp(SearchResultProcessingContainerViewModel container)
        {
            return ReorganizableCollectionUtilities<SearchResultProcessingContainerViewModel>.MoveUp(
                Containers,
                container,
                () => SelectedContainer,
                x => SelectedContainer = x
            );
        }

        public bool MoveContainerDown(SearchResultProcessingContainerViewModel container)
        {
            return ReorganizableCollectionUtilities<SearchResultProcessingContainerViewModel>.MoveDown(
                Containers,
                container,
                () => SelectedContainer,
                x => SelectedContainer = x
            );
        }

        public bool RemoveContainer(SearchResultProcessingContainerViewModel container)
        {
            return ReorganizableCollectionUtilities<SearchResultProcessingContainerViewModel>.Remove(
                Containers,
                container,
                x => SelectedContainer == x,
                () => SelectedContainer = null
            );
        }

        public void MakeContainerActive(SearchResultProcessingContainerViewModel container)
        {
            foreach (SearchResultProcessingContainerViewModel x in Containers)
                x.IsActive = x == container;
        }

        private void OnCreateNew(object parameter)
        {
            Containers.Add(new SearchResultProcessingContainerViewModel(this));
        }
    }
}