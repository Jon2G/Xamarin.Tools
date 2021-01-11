using Kit.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Kit
{

    public abstract class ViewModelBase<T> : KitINotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, args);
        }
        #endregion

        #region GlobalPropertyChanged
        protected static event PropertyChangedEventHandler GlobalPropertyChanged = delegate { };
        protected static void OnGlobalPropertyChanged([CallerMemberName] string propertyName = null)
        {
            GlobalPropertyChanged(
                typeof(T),
                new PropertyChangedEventArgs(propertyName));
        }
        #endregion

    }
}
