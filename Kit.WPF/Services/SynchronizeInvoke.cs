using System;
using System.Threading.Tasks;
using System.Windows;
using Kit.Services.Interfaces;

namespace Kit.WPF.Services
{
    public class SynchronizeInvoke : ISynchronizeInvoke
    {
        public void BeginInvokeOnMainThread(Action action)
        {
            Application.Current.Dispatcher.BeginInvoke(action);
        }

        public async Task InvokeOnMainThreadAsync(Action action)
        {
            await Application.Current.Dispatcher.InvokeAsync(action);
        }
    }
}
