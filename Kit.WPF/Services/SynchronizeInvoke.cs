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
            var app = Application.Current;
            if (app is null)
            {
                action.Invoke();
                return;
            }
            app.Dispatcher.BeginInvoke(action);
        }

        public async Task InvokeOnMainThreadAsync(Action action)
        {
            var app = Application.Current;
            if (app is null)
            {
                await Task.Run(action);
                return;
            }
            await Application.Current.Dispatcher.InvokeAsync(action);
        }
    }
}
