﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MHArmory.ViewModels;

namespace MHArmory
{
    /// <summary>
    /// Interaction logic for DecorationsOverrideWindow.xaml
    /// </summary>
    public partial class DecorationsOverrideWindow : Window
    {
        private readonly RootViewModel rootViewModel;
        private readonly DecorationsOverrideViewModel decorationsOverrideViewModel;

        public bool HasDecorationOverrideChanged { get; private set; }

        public DecorationsOverrideWindow(RootViewModel rootViewModel)
        {
            InitializeComponent();

            this.rootViewModel = rootViewModel;

            decorationsOverrideViewModel = new DecorationsOverrideViewModel(GlobalData.Instance.Jewels);

            DataContext = decorationsOverrideViewModel;
        }

        protected override void OnClosed(EventArgs e)
        {
            InParametersConfiguration config = GlobalData.Instance.Configuration.InParameters;

            if (decorationsOverrideViewModel.HasChanged)
            {
                Dictionary<int, DecorationOverrideConfigurationItem> items = config.DecorationOverride.Items;

                items.Clear();

                foreach (JewelOverrideViewModel vm in decorationsOverrideViewModel.Jewels.Where(x => x.IsOverriding || x.Count > 0))
                {
                    items.Add(vm.Id, new DecorationOverrideConfigurationItem
                    {
                        IsOverriding = vm.IsOverriding,
                        Count = vm.Count
                    });
                }

                GlobalData.Instance.Configuration.Save();

                rootViewModel.CreateSolverData();

                HasDecorationOverrideChanged = true;
            }

            base.OnClosed(e);
        }
    }
}