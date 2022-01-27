using System;
using System.Collections.Generic;
using System.Text;

namespace Kit.Services.Interfaces
{
    public interface ICrossVisualElement
    {
        public bool IsVisible { get; set; }
        public object BindingContext { get; set; }
    }
}
