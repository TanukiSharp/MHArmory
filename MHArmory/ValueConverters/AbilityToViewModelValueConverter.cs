using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using MHArmory.Core.DataStructures;
using MHArmory.ViewModels;

namespace MHArmory.ValueConverters
{
    public class AbilityToViewModelValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IAbility ability)
                return new AbilityViewModel(ability, null);
            else if (value is IEnumerable<IAbility> abilities)
                return abilities.Select(x => new AbilityViewModel(x, null));

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
