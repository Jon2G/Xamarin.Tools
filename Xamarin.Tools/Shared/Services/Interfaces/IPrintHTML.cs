using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Plugin.Xamarin.Tools.Shared.Services.Interfaces
{
    public interface IPrintHTML
    {
        public bool Print(string Html, string Printer);
    }
}
