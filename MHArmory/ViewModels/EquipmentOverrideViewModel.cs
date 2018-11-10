using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MHArmory.Core.DataStructures;

namespace MHArmory.ViewModels
{
    public enum EquipmentOverrideVisibilityMode
    {
        /// <summary>
        /// Shows all sets.
        /// </summary>
        All,
        /// <summary>
        /// Show all sets where all armor pieces are owned.
        /// </summary>
        AllPossessed,
        /// <summary>
        /// Show only sets where at least one piece is owned.
        /// </summary>
        SomePossessed,
        /// <summary>
        /// Show only sets where at least one piece is not owned.
        /// </summary>
        SomeUnpossessed,
        /// <summary>
        /// Show only sets where no armor piece is owned.
        /// </summary>
        AllUnpossessed
    }

    public class EquipmentGroupViewModel : ViewModelBase
    {
        public string Name { get; }
        public EquipmentViewModel Head { get; }
        public EquipmentViewModel Chest { get; }
        public EquipmentViewModel Gloves { get; }
        public EquipmentViewModel Waist { get; }
        public EquipmentViewModel Legs { get; }

        private bool isVisible = true;
        public bool IsVisible
        {
            get { return isVisible; }
            set { SetValue(ref isVisible, value); }
        }

        public bool PossessNone
        {
            get
            {
                return allPieces.All(x => x.IsPossessed == false);
            }
        }

        public bool PossessAll
        {
            get
            {
                return allPieces.All(x => x.IsPossessed);
            }
        }

        public bool PossessAny
        {
            get
            {
                return allPieces.Any(x => x.IsPossessed);
            }
        }

        public ICommand ToggleAllCommand { get; }

        private readonly IList<EquipmentViewModel> allPieces;

        private readonly EquipmentOverrideViewModel parent;

        public EquipmentGroupViewModel(EquipmentOverrideViewModel parent, IEnumerable<EquipmentViewModel> equipments)
            : this(
                  parent,
                  equipments.FirstOrDefault(x => x.Type == EquipmentType.Head),
                  equipments.FirstOrDefault(x => x.Type == EquipmentType.Chest),
                  equipments.FirstOrDefault(x => x.Type == EquipmentType.Gloves),
                  equipments.FirstOrDefault(x => x.Type == EquipmentType.Waist),
                  equipments.FirstOrDefault(x => x.Type == EquipmentType.Legs)
                  )
        {
        }

        public EquipmentGroupViewModel(
            EquipmentOverrideViewModel parent,
            EquipmentViewModel head,
            EquipmentViewModel chest,
            EquipmentViewModel gloves,
            EquipmentViewModel waist,
            EquipmentViewModel legs
        )
        {
            this.parent = parent;

            Head = head;
            Chest = chest;
            Gloves = gloves;
            Waist = waist;
            Legs = legs;

            allPieces = new List<EquipmentViewModel>();

            if (head != null)
                allPieces.Add(head);
            if (chest != null)
                allPieces.Add(chest);
            if (gloves != null)
                allPieces.Add(gloves);
            if (waist != null)
                allPieces.Add(waist);
            if (legs != null)
                allPieces.Add(legs);

            Name = FindGroupName(head, chest, gloves, waist, legs);

            ToggleAllCommand = new AnonymousCommand(OnToggleAll);
        }

        public void ApplySearchText(SearchStatement searchStatement)
        {
            if (searchStatement == null || searchStatement.IsEmpty)
            {
                IsVisible = true;
                return;
            }

            IsVisible =
                searchStatement.IsMatching(Name) ||
                allPieces.Any(x => searchStatement.IsMatching(x.Name));
        }

        private void OnToggleAll()
        {
            bool allChecked = allPieces.All(x => x.IsPossessed);

            foreach (EquipmentViewModel equipment in allPieces)
                equipment.IsPossessed = allChecked == false;
        }

        public static string FindGroupName(params IEquipment[] equipments)
        {
            if (equipments == null || equipments.Length == 0)
                return null;

            string baseName = null;
            int firstPartMinLength = 0;
            int lastPartMinLength = 0;

            for (int i = 0; i < equipments.Length; i++)
            {
                if (equipments[i] == null)
                    continue;

                if (baseName == null)
                {
                    baseName = equipments[i].Name;
                    firstPartMinLength = baseName.Length;
                    lastPartMinLength = baseName.Length;
                    continue;
                }

                int c;
                string name = equipments[i].Name;

                for (c = 0; c < name.Length && c < baseName.Length; c++)
                {
                    if (name[c] != baseName[c])
                        break;
                }

                if (c < firstPartMinLength)
                    firstPartMinLength = c;

                for (c = 0; c < name.Length && c < baseName.Length; c++)
                {
                    if (name[name.Length - c - 1] != baseName[baseName.Length - c - 1])
                        break;
                }

                if (c < lastPartMinLength)
                    lastPartMinLength = c;
            }

            if (firstPartMinLength == 0)
                return null;

            if (lastPartMinLength == 0 || firstPartMinLength == lastPartMinLength)
            {
                if (firstPartMinLength == baseName.Length)
                    return baseName;

                return baseName.Substring(0, firstPartMinLength).Trim();
            }

            string firstPart = baseName.Substring(0, firstPartMinLength).Trim();
            string lastPart = baseName.Substring(baseName.Length - lastPartMinLength).Trim();

            return $"{firstPart} {lastPart}";
        }
    }

    public class EquipmentOverrideViewModel : ViewModelBase
    {
        private readonly RootViewModel rootViewModel;

        private IList<EquipmentGroupViewModel> armorSets;
        public IList<EquipmentGroupViewModel> ArmorSets
        {
            get { return armorSets; }
            private set { SetValue(ref armorSets, value); }
        }

        private string searchText;
        public string SearchText
        {
            get { return searchText; }
            set
            {
                if (SetValue(ref searchText, value))
                    ComputeVisibility();
            }
        }

        private string status;
        public string Status
        {
            get { return status; }
            private set { SetValue(ref status, value); }
        }

        private EquipmentOverrideVisibilityMode visibilityMode = EquipmentOverrideVisibilityMode.All;
        public EquipmentOverrideVisibilityMode VisibilityMode
        {
            get { return visibilityMode; }
            set
            {
                if (SetValue(ref visibilityMode, value))
                    ComputeVisibility();
            }
        }

        public ICommand CancelCommand { get; }

        public EquipmentOverrideViewModel(RootViewModel rootViewModel)
        {
            this.rootViewModel = rootViewModel;

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

        public void ComputeVisibility()
        {
            var searchStatement = SearchStatement.Create(SearchText);

            foreach (EquipmentGroupViewModel vm in ArmorSets)
                ComputeVisibility(vm, searchStatement);

            UpdateStatus();
        }

        private void UpdateStatus()
        {
            Status = $"{ArmorSets.Count(x => x.IsVisible)} sets";
        }

        private void ComputeVisibility(EquipmentGroupViewModel group, SearchStatement searchStatement)
        {
            if (visibilityMode == EquipmentOverrideVisibilityMode.AllPossessed)
            {
                if (group.PossessAll == false)
                {
                    group.IsVisible = false;
                    return;
                }
            }
            else if (visibilityMode == EquipmentOverrideVisibilityMode.SomePossessed)
            {
                if (group.PossessAny == false)
                {
                    group.IsVisible = false;
                    return;
                }
            }
            else if (visibilityMode == EquipmentOverrideVisibilityMode.SomeUnpossessed)
            {
                if (group.PossessAll || group.PossessNone)
                {
                    group.IsVisible = false;
                    return;
                }
            }
            else if (visibilityMode == EquipmentOverrideVisibilityMode.AllUnpossessed)
            {
                if (group.PossessAny)
                {
                    group.IsVisible = false;
                    return;
                }
            }

            if (searchStatement == null)
                searchStatement = SearchStatement.Create(searchText);

            group.ApplySearchText(searchStatement);
        }

        internal void NotifyDataLoaded()
        {
            IEnumerable<IArmorPiece> allArmoprPieces =
                GlobalData.Instance.Heads
                .Concat(GlobalData.Instance.Chests)
                .Concat(GlobalData.Instance.Gloves)
                .Concat(GlobalData.Instance.Waists)
                .Concat(GlobalData.Instance.Legs);

            ArmorSets = allArmoprPieces
                .GroupBy(x => x.Id, x => new EquipmentViewModel(rootViewModel, x))
                .Select(x => new EquipmentGroupViewModel(this, x))
                .OrderBy(x => x.Name)
                .ToList();

            UpdateStatus();
        }
    }
}
