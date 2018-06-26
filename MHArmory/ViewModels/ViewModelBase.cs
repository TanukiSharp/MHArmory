using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace MHArmory.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public Dispatcher Dispatcher { get; }

        protected ViewModelBase()
        {
            Dispatcher = Dispatcher.CurrentDispatcher;
        }

        protected bool SetValue<T>(ref T field, T value, [CallerMemberName]string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value) == false)
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
