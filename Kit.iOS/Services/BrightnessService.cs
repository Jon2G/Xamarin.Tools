using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using Kit.Services.Interfaces;
using UIKit;

namespace Kit.iOS.Services
{
    public class BrightnessService : IBrightnessService
    {
        public float GetBrightness()
        {
            return (float)UIScreen.MainScreen.Brightness;
        }

        public void SetBrightness(float factor)
        {
            UIScreen.MainScreen.Brightness = factor;

        }
    }
}