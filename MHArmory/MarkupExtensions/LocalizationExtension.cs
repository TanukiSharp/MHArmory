//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Markup;

//namespace MHArmory.MarkupExtensions
//{
//    public class LocalizationExtension : MarkupExtension
//    {
//        private readonly Binding binding;
//        private BindingExpression expression;

//        public LocalizationExtension(Binding binding)
//        {
//            this.binding = binding;
//        }

//        public override object ProvideValue(IServiceProvider serviceProvider)
//        {
//            if (expression != null)
//                return expression;
            
            
//            expression = binding.ProvideValue(serviceProvider) as BindingExpression;

//            if (expression == null)
//                return this;

//            var target = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;

//            if (target.TargetObject is FrameworkElement element)
//            {
//                element.Unloaded += Element_Unloaded;
//                Core.Localization.LanguageChanged += Localization_LanguageChanged;

//                return expression;
//            }

//            return this;
//        }

//        //public override object ProvideValue(IServiceProvider serviceProvider)
//        //{
//        //    var expression = binding.ProvideValue(serviceProvider) as BindingExpression;

//        //    if (expression == null)
//        //        return this;

//        //    if (binding != null)
//        //        return binding;

//        //    var target = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;

//        //    if (target.TargetObject is FrameworkElement element)
//        //    {
//        //        if (element.DataContext == null)
//        //            return null;

//        //        var nameDictionary = element.DataContext.GetType().GetProperty(name).GetValue(element.DataContext) as Dictionary<string, string>;

//        //        //return Core.Localization.Get(nameDictionary);

//        //        binding = new Binding
//        //        {
//        //            UpdateSourceTrigger = UpdateSourceTrigger.Explicit,
//        //            Path = new PropertyPath(name)
//        //        };

//        //        element.Unloaded += Element_Unloaded;
//        //        Core.Localization.LanguageChanged += Localization_LanguageChanged;

//        //        return binding;
//        //    }

//        //    return this;
//        //}

//        private void Localization_LanguageChanged(object sender, EventArgs e)
//        {
//            expression.UpdateTarget();
//        }

//        private void Element_Unloaded(object sender, RoutedEventArgs e)
//        {
//            ((FrameworkElement)sender).Unloaded += Element_Unloaded;
//            Core.Localization.LanguageChanged -= Localization_LanguageChanged;
//        }
//    }
//}
