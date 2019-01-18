using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MHArmory
{
    public static class RoutedCommands
    {
        public static readonly ICommand OpenSkillsSelector = new RoutedCommand("OpenSkillsSelector", typeof(RoutedCommands));
        public static readonly ICommand OpenAdvancedSearch = new RoutedCommand("OpenAdvancedSearch", typeof(RoutedCommands));
        public static readonly ICommand OpenDecorationsOverride = new RoutedCommand("OpenDecorationsOverride", typeof(RoutedCommands));
        public static readonly ICommand OpenEquipmentOverride = new RoutedCommand("OpenEquipmentOverride", typeof(RoutedCommands));
        public static readonly ICommand OpenEquipmentExplorer = new RoutedCommand("OpenEquipmentExplorer", typeof(RoutedCommands));
        public static readonly ICommand OpenSearchResultProcessing = new RoutedCommand("OpenSearchResultProcessing", typeof(RoutedCommands));
        public static readonly ICommand OpenWeapons = new RoutedCommand("OpenWeapons", typeof(RoutedCommand));
        public static readonly ICommand OpenExtensions = new RoutedCommand("OpenExtensions", typeof(RoutedCommands));

        public static readonly ICommand NewLoadout = new RoutedCommand("NewLoadout", typeof(RoutedCommands));
        public static readonly ICommand OpenLoadout = new RoutedCommand("OpenLoadout", typeof(RoutedCommands));
        public static readonly ICommand SaveLoadout = new RoutedCommand("SaveLoadout", typeof(RoutedCommands));
        public static readonly ICommand SaveLoadoutAs = new RoutedCommand("SaveLoadoutAs", typeof(RoutedCommands));
        public static readonly ICommand ManageLoadouts = new RoutedCommand("ManageLoadouts", typeof(RoutedCommands));

        public static readonly ICommand OpenIntegratedHelp = new RoutedCommand("OpenIntegratedHelp", typeof(RoutedCommands));

        public static CommandBinding CreateCommandBinding(ICommand source, Action<object> target)
        {
            return new CommandBinding(source, (s, e) => target(e.Parameter));
        }
    }
}
