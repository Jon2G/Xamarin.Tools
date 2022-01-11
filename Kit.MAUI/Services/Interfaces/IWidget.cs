using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui;using Microsoft.Maui.Controls;

namespace Kit.MAUI.Services.Interfaces
{
    public abstract class IWidget
    {
        public abstract string AppWidgetProviderFullClassName { get; }
        public static void UpdateWidget(string AppWidgetProviderFullClassName)
        {
            IUpdateWidget iWidget =TinyIoC.TinyIoCContainer.Current.Resolve<IUpdateWidget>();
            iWidget?.UpdateWidget(AppWidgetProviderFullClassName);
        }

        public static void UpdateWidget(IWidget widget) => UpdateWidget(widget.AppWidgetProviderFullClassName);
    }
}
