using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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

        private void OnCreateNew(object parameter)
        {
            Containers.Add(new SearchResultProcessingContainerViewModel(this));
        }
    }
}
