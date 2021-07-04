using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Kit.Forms.Services.Interfaces
{
    public interface IUpdateWidget
    {
        public void UpdateWidget(string AssemblyName, string AppWidgetProviderFullClassName);
    }
}
