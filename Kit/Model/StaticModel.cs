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

        public static event PropertyChangedEventHandler StaticPropertyChanged;

        protected static void OnGlobalPropertyChanged([CallerMemberName] string propertyName = null)
        {
            StaticPropertyChanged?.Invoke(
                null,
                new PropertyChangedEventArgs(propertyName));
        }

        #endregion GlobalPropertyChanged
    }
}