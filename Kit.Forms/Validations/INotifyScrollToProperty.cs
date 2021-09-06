using System;
using System.Collections.Generic;
using System.Text;

namespace Kit.Forms.Validations
{
    public interface INotifyScrollToProperty
    {
        event ScrollToPropertyHandler ScrollToProperty;
    }

    public delegate void ScrollToPropertyHandler(string PropertyName);
}
