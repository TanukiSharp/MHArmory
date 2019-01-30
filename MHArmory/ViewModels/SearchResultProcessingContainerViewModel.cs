using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MHArmory.Core.WPF;

namespace MHArmory.ViewModels
{
    public class SearchResultProcessingContainerViewModel : ViewModelBase
    {
        private readonly SearchResultProcessingViewModel parent;

        private string name = "(Default)";
        public string Name
        {
            get { return name; }
            set { SetValue(ref name, value); }
        }

        private bool isEditMode;
        public bool IsEditMode
        {
            get { return isEditMode; }
            private set { SetValue(ref isEditMode, value); }
        }

        private bool isActive;
        public bool IsActive
        {
            get { return isActive; }
            set
            {
                if (SetValue(ref isActive, value))
                    parent.ActiveContainerChanged();
            }
        }

        public ICommand ApplyCommand { get; }
        public ICommand MakeActiveCommand { get; }
        public ICommand DeactivateCommand { get; }
        public ICommand AddSortItemCommand { get; }

        private readonly ObservableCollection<SearchResultSortItemViewModel> sortItems = new ObservableCollection<SearchResultSortItemViewModel>();
        public ReadOnlyObservableCollection<SearchResultSortItemViewModel> SortItems { get; }

        public ICommand MoveSelfUpCommand { get; }
        public ICommand MoveSelfDownCommand { get; }
        public ICommand RemoveSelfCommand { get; }

        public SearchResultProcessingContainerViewModel(SearchResultProcessingViewModel parent)
        {
            this.parent = parent;

            SortItems = new ReadOnlyObservableCollection<SearchResultSortItemViewModel>(sortItems);

            IsEditMode = parent.IsEditMode;

            ApplyCommand = new AnonymousCommand(OnApply);
            MakeActiveCommand = new AnonymousCommand(OnMakeActive);
            DeactivateCommand = new AnonymousCommand(() => IsActive = false);
            AddSortItemCommand = new AnonymousCommand(OnAddSortItem);

            MoveSelfUpCommand = new AnonymousCommand(OnMoveSelfUp);
            MoveSelfDownCommand = new AnonymousCommand(OnMoveSelfDown);
            RemoveSelfCommand = new AnonymousCommand(OnRemoveSelf);

            parent.EditModeChanged += Parent_EditModeChanged;
        }

        public void AddSortItem(SearchResultSortItemViewModel sortItem)
        {
            bool isFirst = SortItems.Count == 0;
            sortItem.SetFirst(isFirst);
            sortItems.Add(sortItem);
        }

        private void OnApply(object parameter)
        {
            parent.ApplyContainerRules(this);
        }

        private void OnMakeActive(object parameter)
        {
            parent.MakeContainerActive(this);
        }

        private void OnAddSortItem(object parameter)
        {
            AddSortItem(new SearchResultSortItemViewModel(this));
        }

        private void UpdateSortItems()
        {
            bool isFirst = true;

            foreach (SearchResultSortItemViewModel x in SortItems)
            {
                x.SetFirst(isFirst);
                isFirst = false;
            }
        }

        public bool MoveSortItemUp(SearchResultSortItemViewModel item)
        {
            bool result = ReorganizableCollectionUtilities<SearchResultSortItemViewModel>.MoveUp(
                sortItems,
                item
            );

            if (result)
                UpdateSortItems();

            return result;
        }

        public bool MoveSortItemDown(SearchResultSortItemViewModel item)
        {
            bool result = ReorganizableCollectionUtilities<SearchResultSortItemViewModel>.MoveDown(
                sortItems,
                item
            );

            if (result)
                UpdateSortItems();

            return result;
        }

        public bool RemoveSortItem(SearchResultSortItemViewModel item)
        {
            bool result = ReorganizableCollectionUtilities<SearchResultSortItemViewModel>.Remove(
                sortItems,
                item
            );

            if (result)
                UpdateSortItems();

            return result;
        }

        private void OnMoveSelfUp(object parameter)
        {
            parent.MoveContainerUp(this);
        }

        private void OnMoveSelfDown(object parameter)
        {
            parent.MoveContainerDown(this);
        }

        private void OnRemoveSelf(object parameter)
        {
            if (parent.RemoveContainer(this))
                parent.EditModeChanged -= Parent_EditModeChanged;
        }

        private void Parent_EditModeChanged(object sender, EventArgs e)
        {
            IsEditMode = parent.IsEditMode;
        }
    }
}
