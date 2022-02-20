using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace Kit
{
    public static class Extensiones
    {
        public static System.Windows.Threading.DispatcherOperation AddRangeAsync<T>(this Collection<T> collection, IEnumerable<T> pendiente)
        {
            Action<IEnumerable<T>> addMethod = collection.AddRange;
            return Application.Current.Dispatcher.BeginInvoke(addMethod, pendiente);
        }

        public static System.Windows.Threading.DispatcherOperation InsertAsync<T>(this Collection<T> collection, T pendiente, int index = 0)
        {
            Action<int, T> addMethod = collection.Insert;
            return Application.Current.Dispatcher.BeginInvoke(addMethod, index, pendiente);
        }

        public static System.Windows.Threading.DispatcherOperation AddAsync<T>(this Collection<T> collection, T pendiente)
        {
            Action<T> addMethod = collection.Add;
            return Application.Current.Dispatcher.BeginInvoke(addMethod, pendiente);
        }

        public static System.Windows.Threading.DispatcherOperation ClearAsync<T>(this Collection<T> collection)
        {
            Action addMethod = collection.Clear;
            return Application.Current.Dispatcher.BeginInvoke(addMethod);
        }

        public static System.Windows.Threading.DispatcherOperation RemoveAsync<T>(this Collection<T> collection, T pendiente)
        {
            Func<T, bool> addMethod = collection.Remove;
            return Application.Current.Dispatcher.BeginInvoke(addMethod, pendiente);
        }
    }
}