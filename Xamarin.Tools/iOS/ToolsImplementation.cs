using Foundation;
using Plugin.Xamarin.Tools.Shared;
using Plugin.Xamarin.Tools.Shared.Pages;
using Plugin.Xamarin.Tools.Shared.Services.Interfaces;
using SQLHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Plugin.Xamarin.Tools.iOS
{
    public class ToolsImplementation : AbstractTools
    {

        public override ITools InitAll(string LogDirectory, bool AlertAfterCritical = false)
        {
           return InitLoggin(LogDirectory, AlertAfterCritical);
        }

        public override ITools InitLoggin(string LogDirectory, bool AlertAfterCritical = false)
        {
            Debugging = Debugger.IsAttached;
            if (AlertAfterCritical)
            {
                SQLHelper.Log.Init(LogDirectory, CriticalAlert);
            }
            else
            {
                SQLHelper.Log.Init(LogDirectory);
            }
            return this;
        }

        public override AbstractTools SetDebugging(bool Debugging)
        {
            this.Debugging = true;
            return this;
        }
        public override void CriticalAlert(object sender, EventArgs e)
        {
            Shared.Services.CustomMessageBox.Current.ShowOK(sender.ToString(), "Alerta", "Entiendo", Shared.Enums.CustomMessageBoxImage.Error);
        }
        public UIInterfaceOrientationMask GetSupportedInterfaceOrientations(Page mainPage)
        {
            if (mainPage.Navigation.NavigationStack.Last() is BasePage page)
            {
                if (page.LockedOrientation != DeviceOrientation.Other)
                {
                    page.Disappearing += Page_Disappearing;
                    switch (page.LockedOrientation)
                    {
                        case DeviceOrientation.Landscape:
                            return UIInterfaceOrientationMask.Landscape;
                        case DeviceOrientation.Portrait:
                            return UIInterfaceOrientationMask.Portrait;

                    }
                }
            }
            return UIInterfaceOrientationMask.All;
        }
        private void Page_Disappearing(object sender, EventArgs e)
        {
            (sender as BasePage).Disappearing -= Page_Disappearing;
            UIDevice.CurrentDevice.SetValueForKey(NSNumber.FromNInt((int)(UIInterfaceOrientation.Unknown)), new NSString("orientation"));
        }
    }
}
