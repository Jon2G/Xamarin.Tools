using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kit.Services.Interfaces
{
    public interface ISynchronizeInvoke
    {
        Task InvokeOnMainThreadAsync(Action action);
        void BeginInvokeOnMainThread(Action action);
    }
}
