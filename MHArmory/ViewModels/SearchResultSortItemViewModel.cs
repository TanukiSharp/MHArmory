using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MHArmory.Core.WPF;

namespace MHArmory.ViewModels
{
    public class SearchResultSortItemViewModel : ViewModelBase
    {
        private string headerText;
        public string HeaderText
        {
            get { return headerText; }
            private set { SetValue(ref headerText, value); }
        }

        private SearchResultSortCriteria sortCriteria;
        public SearchResultSortCriteria SortCriteria
        {
            get { return sortCriteria; }
            set { SetValue(ref sortCriteria, value); }
        }

        public ICommand MoveSelfUpCommand { get; }
        public ICommand MoveSelfDownCommand { get; }
        public ICommand RemoveSelfCommand { get; }

        public void SetFirst(bool isFirst)
        {
            if (isFirst)
                HeaderText = "Order by";
            else
                HeaderText = "Then by";
        }

        private readonly SearchResultProcessingContainerViewModel parent;

        public SearchResultSortItemViewModel(SearchResultProcessingContainerViewModel parent)
        {
            this.parent = parent;

            MoveSelfUpCommand = new AnonymousCommand(OnMoveSelfUp);
            MoveSelfDownCommand = new AnonymousCommand(OnMoveSelfDown);
            RemoveSelfCommand = new AnonymousCommand(OnRemoveSelf);
        }

        private void OnMoveSelfUp(object parameter)
        {
            parent.MoveSortItemUp(this);
        }

        private void OnMoveSelfDown(object parameter)
        {
            parent.MoveSortItemDown(this);
        }

        private void OnRemoveSelf(object parameter)
        {
            parent.RemoveSortItem(this);
        }
    }
}
