using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Resources;

namespace MHArmory.ViewModels
{
    public enum HelpCategory
    {
        GettingStarted,
        DecorationsOverride,
        EquipmentOverride,
        KeyboardShortcuts,
        Sorting
    }

    public class HelpViewModel : ViewModelBase
    {
        public HelpCategory HelpCategory { get; }

        public string Title { get; }
        public string Content { get; }

        public HelpViewModel(HelpCategory helpCategory)
        {
            HelpCategory = helpCategory;

            Assembly asm = typeof(App).GetTypeInfo().Assembly;

            Stream stream = asm.GetManifestResourceStream($"MHArmory.HelpText.{helpCategory}.html");
            using (var sr = new StreamReader(stream, Encoding.UTF8))
            {
                Title = sr.ReadLine();
                Content = sr.ReadToEnd().TrimStart();
            }
        }
    }

    public class HelpRootViewModel : ViewModelBase
    {
        public IList<HelpViewModel> Categories { get; }

        private HelpViewModel selected;
        public HelpViewModel Selected
        {
            get { return selected; }
            set { SetValue(ref selected, value); }
        }

        public HelpRootViewModel()
        {
            Categories = Enum.GetValues(typeof(HelpCategory))
                .Cast<HelpCategory>()
                .Select(x => new HelpViewModel(x))
                .ToArray();
        }

        public bool SelectCategory(HelpCategory category)
        {
            if (Categories == null || Categories.Count <= 0)
                return false;

            HelpViewModel helpViewModel = Categories.FirstOrDefault(x => x.HelpCategory == category);
            if (helpViewModel == null)
                return false;

            Selected = helpViewModel;

            return true;
        }
    }
}
