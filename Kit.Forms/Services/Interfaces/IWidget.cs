using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Kit.Forms.Services.Interfaces
{
    public abstract class IWidget
    {
        public abstract string AppWidgetProviderFullClassName { get; }
        public static void UpdateWidget(string AppWidgetProviderFullClassName)
        {
            IUpdateWidget iWidget = DependencyService.Get<IUpdateWidget>();
            iWidget?.UpdateWidget(AppWidgetProviderFullClassName);
        }

        public static void UpdateWidget(IWidget widget) => UpdateWidget(widget.AppWidgetProviderFullClassName);
    }
}
