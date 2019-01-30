using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace MHArmory.Core.WPF
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
                NotifyPropertyChanged(propertyName);
                return true;
            }

            return false;
        }

        protected void NotifyPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
