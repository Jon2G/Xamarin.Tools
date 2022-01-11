﻿using System;
using System.Collections.Generic;
using System.Text;
using Kit.Sql.Attributes;

namespace Kit.MAUI.Validations
{
    [Preserve]
    public interface INotifyScrollToProperty
    {
        event ScrollToPropertyHandler ScrollToProperty;
    }
    [Preserve]
    public delegate void ScrollToPropertyHandler(string PropertyName);
}
