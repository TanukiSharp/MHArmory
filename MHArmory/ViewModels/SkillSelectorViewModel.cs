using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MHArmory.Core;
using MHArmory.Core.DataStructures;
using MHArmory.Core.WPF;

namespace MHArmory.ViewModels
{
    public enum VisibilityMode
    {
        All,
        Selected,
        Unselected
    }

    public class SkillSelectorViewModel : ViewModelBase
    {
        private IEnumerable<SkillViewModel> skills;
        public IEnumerable<SkillViewModel> Skills
        {
            get { return skills; }
            set
            {
                if (SetValue(ref skills, value))
                    UpdateCategories();
            }
        }

        private IList<NamedValueViewModel<bool>> categories;
        public IList<NamedValueViewModel<bool>> Categories
        {
            get { return categories; }
            private set { SetValue(ref categories, value); }
        }

        private VisibilityMode visibilityMode = VisibilityMode.All;

        public bool VisibilityModeAll
        {
            set
            {
                if (value && visibilityMode != VisibilityMode.All)
                {
                    visibilityMode = VisibilityMode.All;
                    ComputeVisibility();
                }
            }
        }

        public bool VisibilityModeSelected
        {
            set
            {
                if (value && visibilityMode != VisibilityMode.Selected)
                {
                    visibilityMode = VisibilityMode.Selected;
                    ComputeVisibility();
                }
            }
        }

        public bool VisibilityModeUnselected
        {
            set
            {
                if (value && visibilityMode != VisibilityMode.Unselected)
                {
                    visibilityMode = VisibilityMode.Unselected;
                    ComputeVisibility();
                }
            }
        }

        private string searchText;
        public string SearchText
        {
            get { return searchText; }
            set
            {
                if (SetValue(ref searchText, value))
                {
                    if (Skills != null)
                        ComputeVisibility();
                }
            }
        }

        private void ComputeVisibility()
        {
            foreach (SkillViewModel vm in Skills)
                ComputeVisibility(vm);
        }

        internal void ComputeVisibility(SkillViewModel skillViewModel)
        {
            if (visibilityMode == VisibilityMode.Selected)
            {
                if (skillViewModel.HasCheckedAbility == false)
                {
                    skillViewModel.IsVisible = false;
                    return;
                }
            }
            else if (visibilityMode == VisibilityMode.Unselected)
            {
                if (skillViewModel.HasCheckedAbility)
                {
                    skillViewModel.IsVisible = false;
                    return;
                }
            }

            if (Categories.Any(x => x.Value))
            {
                if (IsMatchingByCategory(skillViewModel) == false)
                {
                    skillViewModel.IsVisible = false;
                    return;
                }
            }

            skillViewModel.ApplySearchText(SearchStatement.Create(searchText));
        }

        private bool IsMatchingByCategory(SkillViewModel skillViewModel)
        {
            if (skillViewModel.Categories == null)
                return false;

            foreach (NamedValueViewModel<bool> category in Categories.Where(x => x.Value))
            {
                if (skillViewModel.Categories.Contains(category.Name))
                    return true;
            }

            return false;
        }

        public ICommand CancelCommand { get; }

        public SkillSelectorViewModel()
        {
            CancelCommand = new AnonymousCommand(OnCancel);
        }

        private void OnCancel(object parameter)
        {
            if (parameter is CancellationCommandArgument cancellable)
            {
                if (string.IsNullOrWhiteSpace(SearchText) == false)
                {
                    SearchText = string.Empty;
                    cancellable.IsCancelled = true;
                }
            }
        }

        private void UpdateCategories()
        {
            var categories = new HashSet<string>();

            foreach (SkillViewModel skill in Skills)
            {
                if (skill.Categories == null)
                    continue;

                foreach (string category in skill.Categories)
                    categories.Add(category);
            }

            Categories = categories
                .Select(x => new NamedValueViewModel<bool>(x, false, _ => ComputeVisibility()))
                .ToList();
        }
    }

    public class NamedValueViewModel<T> : ValueViewModel<T>
    {
        public string Name { get; }

        public NamedValueViewModel(string name)
            : this(name, default(T))
        {
        }

        public NamedValueViewModel(string name, T initialValue)
            : this(name, initialValue, null)
        {
        }

        public NamedValueViewModel(string name, Action<T> notifyChanged)
            : this(name, default(T), notifyChanged)
        {
        }

        public NamedValueViewModel(string name, T initialValue, Action<T> notifyChanged)
            : base(initialValue, notifyChanged)
        {
            Name = name;
        }
    }
}
