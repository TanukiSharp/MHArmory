using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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

        public ICommand AddSortItemCommand { get; }

        public ObservableCollection<SearchResultSortItemViewModel> SortItems { get; } = new ObservableCollection<SearchResultSortItemViewModel>();

        public ICommand MoveSelfUpCommand { get; }
        public ICommand MoveSelfDownCommand { get; }
        public ICommand RemoveSelfCommand { get; }

        public SearchResultProcessingContainerViewModel(SearchResultProcessingViewModel parent)
        {
            this.parent = parent;

            IsEditMode = parent.IsEditMode;

            AddSortItemCommand = new AnonymousCommand(OnAddSortItem);

            MoveSelfUpCommand = new AnonymousCommand(OnMoveSelfUp);
            MoveSelfDownCommand = new AnonymousCommand(OnMoveSelfDown);
            RemoveSelfCommand = new AnonymousCommand(OnRemoveSelf);

            parent.EditModeChanged += Parent_EditModeChanged;
        }

        private void OnAddSortItem(object parameter)
        {
            bool isFirst = SortItems.Count == 0;

            var x = new SearchResultSortItemViewModel(this);
            x.SetFirst(isFirst);

            SortItems.Add(x);
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
                SortItems,
                item
            );

            if (result)
                UpdateSortItems();

            return result;
        }

        public bool MoveSortItemDown(SearchResultSortItemViewModel item)
        {
            bool result = ReorganizableCollectionUtilities<SearchResultSortItemViewModel>.MoveDown(
                SortItems,
                item
            );

            if (result)
                UpdateSortItems();

            return result;
        }

        public bool RemoveSortItem(SearchResultSortItemViewModel item)
        {
            bool result = ReorganizableCollectionUtilities<SearchResultSortItemViewModel>.Remove(
                SortItems,
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
