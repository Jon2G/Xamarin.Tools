﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.Xamarin.Tools.Shared.Pages.PinView
{
    public class PinChangedEventArg
    {
        public PinChangedEventArg(object sender, string pin)
        {
            Source = sender;
            Pin = pin;
        }
        public object Source { get; private set; }
        public string Pin { get; private set; }
    }
}
