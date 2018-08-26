using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public ObservableCollection<SearchResultProcessingContainerViewModel> Containers { get { return rootViewModel.SearchResultProcessingItems; } }

        private readonly RootViewModel rootViewModel;

        public SearchResultProcessingViewModel(RootViewModel rootViewModel)
        {
            this.rootViewModel = rootViewModel;

            CreateNewCommand = new AnonymousCommand(OnCreateNew);

            LoadConfiguration();
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
