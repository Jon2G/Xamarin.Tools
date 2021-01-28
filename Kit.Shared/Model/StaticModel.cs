using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Kit.Model
{
    public class StaticModel<T> : ModelBase
    {
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
