using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.Core.DataStructures;

namespace MHArmory.ViewModels
{
    public enum DecorationOverrideVisibilityMode
    {
        /// <summary>
        /// Show all jewels.
        /// </summary>
        All,
        /// <summary>
        /// Show only jewels where override checkbox is checked or count is greater than zero.
        /// </summary>
        Modified,
        /// <summary>
        /// Show only jewels where override checkbox is unchecked and count is zero.
        /// </summary>
        Unmodified
    }

    public class DecorationsOverrideViewModel : ViewModelBase
    {
        public bool HasChanged { get; private set; }

        private IEnumerable<JewelOverrideViewModel> jewels;
        public IEnumerable<JewelOverrideViewModel> Jewels
        {
            get { return jewels; }
            set { SetValue(ref jewels, value); }
        }

        private DecorationOverrideVisibilityMode visibilityMode = DecorationOverrideVisibilityMode.All;

        public bool VisibilityModeAll
        {
            set
            {
                if (value && visibilityMode != DecorationOverrideVisibilityMode.All)
                {
                    visibilityMode = DecorationOverrideVisibilityMode.All;
                    ComputeVisibility();
                }
            }
        }

        public bool VisibilityModeModified
        {
            set
            {
                if (value && visibilityMode != DecorationOverrideVisibilityMode.Modified)
                {
                    visibilityMode = DecorationOverrideVisibilityMode.Modified;
                    ComputeVisibility();
                }
            }
        }

        public bool VisibilityModeUnmodified
        {
            set
            {
                if (value && visibilityMode != DecorationOverrideVisibilityMode.Unmodified)
                {
                    visibilityMode = DecorationOverrideVisibilityMode.Unmodified;
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
                    if (Jewels != null)
                        ComputeVisibility();
                }
            }
        }

        private readonly bool isLoadingConfiguration;

        public DecorationsOverrideViewModel(IList<IJewel> jewels)
        {
            Jewels = jewels
                .Select(x => new JewelOverrideViewModel(this, x, 0))
                .ToList();

            Dictionary<int, DecorationOverrideConfigurationItem> decorationOverrides = GlobalData.Instance.Configuration.InParameters?.DecorationOverride?.Items;

            if (decorationOverrides != null)
            {
                isLoadingConfiguration = true;

                try
                {
                    foreach (KeyValuePair<int, DecorationOverrideConfigurationItem> decoOverride in decorationOverrides)
                    {
                        JewelOverrideViewModel vm = Jewels.FirstOrDefault(x => x.Id == decoOverride.Key);
                        if (vm != null)
                        {
                            vm.IsOverriding = decoOverride.Value.IsOverriding;
                            vm.Count = decoOverride.Value.Count;
                        }
                    }
                }
                finally
                {
                    isLoadingConfiguration = false;
                }
            }
        }

        internal void StateChanged()
        {
            if (isLoadingConfiguration)
                return;

            HasChanged = true;
        }

        private void ComputeVisibility()
        {
            if (isLoadingConfiguration)
                return;

            foreach (JewelOverrideViewModel vm in Jewels)
                ComputeVisibility(vm);
        }

        internal void ComputeVisibility(JewelOverrideViewModel jewelOverrideViewModel)
        {
            if (visibilityMode == DecorationOverrideVisibilityMode.Modified)
            {
                if (jewelOverrideViewModel.IsOverriding == false && jewelOverrideViewModel.Count == 0)
                {
                    jewelOverrideViewModel.IsVisible = false;
                    return;
                }
            }
            else if (visibilityMode == DecorationOverrideVisibilityMode.Unmodified)
            {
                if (jewelOverrideViewModel.IsOverriding || jewelOverrideViewModel.Count > 0)
                {
                    jewelOverrideViewModel.IsVisible = false;
                    return;
                }
            }

            jewelOverrideViewModel.ApplySearchText(SearchStatement.Create(searchText));
        }
    }
}
