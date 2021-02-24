using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Kit.Services.Interfaces;

namespace Kit.Forms.Services
{
    public class SynchronizeInvoke : ISynchronizeInvoke
    {
        public async Task InvokeOnMainThreadAsync(Action action)
        {
            await Xamarin.Forms.Device.InvokeOnMainThreadAsync(action);
        }

        public void BeginInvokeOnMainThread(Action action)
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(action);
        }
    }
}
