using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using MHArmory.Core;
using MHArmory.Core.DataStructures;
using MHArmory.Search;
using MHArmory.Searching;

namespace MHArmory.ViewModels
{
    public class RootViewModel : ViewModelBase
    {
        public ICommand OpenSkillSelectorCommand { get; }
        public ICommand SearchArmorSetsCommand { get; }
        public ICommand AdvancedSearchCommand { get; }

        private SolverData solverData;
        private Solver solver;

        private bool isDataLoading = true;
        public bool IsDataLoading
        {
            get { return isDataLoading; }
            set { SetValue(ref isDataLoading, value); }
        }

        private bool isDataLoaded;
        public bool IsDataLoaded
        {
            get { return isDataLoaded; }
            set { SetValue(ref isDataLoaded, value); }
        }

        private IEnumerable<AbilityViewModel> selectedAbilities;
        public IEnumerable<AbilityViewModel> SelectedAbilities
        {
            get { return selectedAbilities; }
            set { SetValue(ref selectedAbilities, value); }
        }

        private IEnumerable<ArmorSetViewModel> foundArmorSets;

        internal void NotifyConfigurationLoaded()
        {
            InParameters.NotifyConfigurationLoaded();
        }

        public IEnumerable<ArmorSetViewModel> FoundArmorSets
        {
            get { return foundArmorSets; }
            set { SetValue(ref foundArmorSets, value); }
        }

        public InParametersViewModel InParameters { get; }

        private bool isSearching;
        public bool IsSearching
        {
            get { return isSearching; }
            private set { SetValue(ref isSearching, value); }
        }

        private bool isAutoSearch;
        public bool IsAutoSearch
        {
            get { return isAutoSearch; }
            set { SetValue(ref isAutoSearch, value); }
        }

        public RootViewModel()
        {
            OpenSkillSelectorCommand = new AnonymousCommand(OpenSkillSelector);
            SearchArmorSetsCommand = new AnonymousCommand(/*CreateSolverDataAnd*/SearchArmorSets);
            AdvancedSearchCommand = new AnonymousCommand(AdvancedSearch);

            InParameters = new InParametersViewModel(this);
        }

        private void OpenSkillSelector(object parameter)
        {
            RoutedCommands.OpenSkillsSelector.Execute(null);
        }

        private void AdvancedSearch(object parameter)
        {
            var window = new AdvancedSearchWindow(this, solverData)
            {
            };
            window.ShowDialog();
        }

        public void CreateSolverDataAndSearchArmorSets()
        {
            CreateSolverData();
            SearchArmorSets();
        }

        public async void SearchArmorSets()
        {
            try
            {
                await SearchArmorSetsInternal();
            }
            catch (TaskCanceledException)
            {
            }
        }

        private CancellationTokenSource searchCancellationTokenSource;
        private Task previousSearchTask;

        private async Task SearchArmorSetsInternal()
        {
            if (searchCancellationTokenSource != null)
            {
                if (searchCancellationTokenSource.IsCancellationRequested)
                    return;

                searchCancellationTokenSource.Cancel();

                if (previousSearchTask != null)
                {
                    try
                    {
                        await previousSearchTask;
                    }
                    catch
                    {
                    }
                }
            }

            searchCancellationTokenSource = new CancellationTokenSource();
            previousSearchTask = Task.Run(() => SearchArmorSetsInternal(searchCancellationTokenSource.Token));

            IsSearching = true;

            try
            {
                await previousSearchTask;
            }
            finally
            {
                IsSearching = false;

                previousSearchTask = null;
                searchCancellationTokenSource = null;
            }
        }

        public void CreateSolverData()
        {
            solverData = null;

            if (IsDataLoaded == false || SelectedAbilities == null)
                return;

            var desiredAbilities = SelectedAbilities
                .Where(x => x.IsChecked)
                .Select(x => x.Ability)
                .ToList();

            solverData = new SolverData(
                InParameters.Slots.Select(x => x.Value).ToList(),
                null,
                GlobalData.Instance.Heads,
                GlobalData.Instance.Chests,
                GlobalData.Instance.Gloves,
                GlobalData.Instance.Waists,
                GlobalData.Instance.Legs,
                GlobalData.Instance.Charms,
                GlobalData.Instance.Jewels.Select(x => new SolverDataJewelModel(x)).ToList(),
                desiredAbilities
            );

            solverData.Done();

            /*************************************************************/
            var sb = new StringBuilder();

            long hh = solverData.AllHeads.Count(x => x.IsSelected);
            long cc = solverData.AllChests.Count(x => x.IsSelected);
            long gg = solverData.AllGloves.Count(x => x.IsSelected);
            long ww = solverData.AllWaists.Count(x => x.IsSelected);
            long ll = solverData.AllLegs.Count(x => x.IsSelected);
            long ch = solverData.AllCharms.Count(x => x.IsSelected);

            var nfi = new System.Globalization.NumberFormatInfo();
            nfi.NumberGroupSeparator = "'";

            sb.AppendLine($"Heads count:  {hh}");
            sb.AppendLine($"Chests count: {cc}");
            sb.AppendLine($"Gloves count: {gg}");
            sb.AppendLine($"Waists count: {ww}");
            sb.AppendLine($"Legs count:   {ll}");
            sb.AppendLine($"Charms count:   {ch}");
            sb.AppendLine($"Combination count: {(hh * cc * gg * ww * ll * ch).ToString("N0", nfi)}");

            SearchResult = sb.ToString();
            /*************************************************************/
        }

        private async Task SearchArmorSetsInternal(CancellationToken cancellationToken)
        {
            solver = new Solver(solverData);

            solver.DebugData += Solver_DebugData;

            IList<ArmorSetSearchResult> result = await solver.SearchArmorSets(cancellationToken);

            if (result == null)
                FoundArmorSets = null;
            else
            {
                FoundArmorSets = result.Where(x => x.IsMatch).Select(x => new ArmorSetViewModel
                {
                    ArmorPieces = x.ArmorPieces,
                    Charm = x.Charm,
                    Jewels = x.Jewels.Select(j => new ArmorSetJewelViewModel { Jewel = j.Jewel, Count = j.Count }).ToList()
                });
            }

            solver.DebugData -= Solver_DebugData;
        }

        internal void SelectedAbilitiesChanged()
        {
            GlobalData.Instance.Configuration.SelectedAbilities = SelectedAbilities
                .Where(x => x.IsChecked)
                .Select(x => x.Id)
                .ToArray();

            GlobalData.Instance.Configuration.Save();
        }

        private void Solver_DebugData(string debugData)
        {
            SearchResult = debugData;
        }

        private MaximizedSearchCriteria[] sortCriterias = new MaximizedSearchCriteria[]
        {
            MaximizedSearchCriteria.BaseDefense,
            MaximizedSearchCriteria.DragonResistance,
            MaximizedSearchCriteria.SlotSizeCube
        };

        private string searchResult;
        public string SearchResult
        {
            get { return searchResult; }
            private set { SetValue(ref searchResult, value); }
        }
    }
}
