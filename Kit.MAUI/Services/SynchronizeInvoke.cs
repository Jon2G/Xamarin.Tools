using System;
using System.Threading.Tasks;
using Kit.Services.Interfaces;

namespace Kit.MAUI.Services
{
    public class SynchronizeInvoke : ISynchronizeInvoke
    {
        public async Task InvokeOnMainThreadAsync(Action action)
        {
            await Device.InvokeOnMainThreadAsync(action);
        }

        public void BeginInvokeOnMainThread(Action action)
        {
            Device.BeginInvokeOnMainThread(action);
        }
    }
}
