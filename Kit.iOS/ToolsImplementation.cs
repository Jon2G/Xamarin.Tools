using Foundation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Kit;
using Kit.Services;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Kit.iOS
{
    public class ToolsImplementation : AbstractTools
    {
        public override ITools Init(string LogDirectory = "Logs", bool AlertAfterCritical = false)
        {
            this.CustomMessageBox = new Services.CustomMessageBoxService();
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
            Acr.UserDialogs.UserDialogs.Instance.Alert(sender.ToString(), "Alerta", "Entiendo");
        }
        public UIInterfaceOrientationMask GetSupportedInterfaceOrientations(Page mainPage)
        {
            if (mainPage.Navigation.NavigationStack.Any() && mainPage.Navigation.NavigationStack.Last() is Page page)
            {
                if (page.GetType().GetProperty("LockedOrientation") is System.Reflection.PropertyInfo LockedOrientationProperty)
                {
                    if (LockedOrientationProperty.GetValue(page) is DeviceOrientation LockedOrientation)
                    {
                        if (LockedOrientation != DeviceOrientation.Other)
                        {
                            page.Disappearing += Page_Disappearing;
                            switch (LockedOrientation)
                            {
                                case DeviceOrientation.Landscape:
                                    return UIInterfaceOrientationMask.Landscape;
                                case DeviceOrientation.Portrait:
                                    return UIInterfaceOrientationMask.Portrait;

                            }
                        }
                    }
                }
            }
            return UIInterfaceOrientationMask.All;
        }
        private void Page_Disappearing(object sender, EventArgs e)
        {
            (sender as Page).Disappearing -= Page_Disappearing;
            UIDevice.CurrentDevice.SetValueForKey(NSNumber.FromNInt((int)UIInterfaceOrientation.Unknown), new NSString("orientation"));
        }
    }
}
