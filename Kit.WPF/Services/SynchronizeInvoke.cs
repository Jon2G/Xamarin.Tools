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
        public T BeginInvokeOnMainThread<T>(Func<T> action)
        {
            var app = Application.Current;
            if (app is null)
            {
                return Task.Run(action).GetAwaiter().GetResult();
            }
            return Application.Current.Dispatcher.InvokeAsync(action).GetAwaiter().GetResult();
        }

        public Task<T> InvokeOnMainThreadAsync<T>(Func<T> action)
        {
            var app = Application.Current;
            if (app is null)
            {
                return Task.Run(action);
            }
            return Application.Current.Dispatcher.InvokeAsync(action).Task;
        }
    }
}
