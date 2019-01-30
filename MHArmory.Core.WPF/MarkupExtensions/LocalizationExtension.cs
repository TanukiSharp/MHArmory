using MHArmory.Core.WPF.ValueConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace MHArmory.Core.WPF.MarkupExtensions
{
    public class LocalizationExtension : MarkupExtension
    {
        private static readonly IValueConverter localizationConverter = new LocalizationValueConverter();

        private readonly string sourcePath;

        public LocalizationExtension(string sourcePath)
        {
            this.sourcePath = sourcePath;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var target = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;

            var dependencyObject = target.TargetObject as DependencyObject;

            if (dependencyObject == null)
                return this;

            var binding = new Binding(sourcePath)
            {
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit,
                Converter = localizationConverter
            };

            var bindingExpression = (BindingExpression)binding.ProvideValue(serviceProvider);

            Core.Localization.RegisterLanguageChanged(bindingExpression, expr =>
            {
                ((BindingExpression)expr).UpdateTarget();
            });

            return bindingExpression;
        }
    }
}
