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

        public static CommandBinding CreateCommandBinding(ICommand source, Action<object> target)
        {
            return new CommandBinding(source, (s, e) => target(e.Parameter));
        }
    }
}
