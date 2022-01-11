using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui;using Microsoft.Maui.Controls;
using Kit.MAUI.Pages;
namespace Kit.MAUI
{
    public abstract class Application : Microsoft.Maui.Controls.Application
    {
        protected override void OnSleep()
        {
            OnSleep(Microsoft.Maui.Controls.Application.Current.MainPage);
        }
        private void OnSleep(Page page)
        {
            switch (page)
            {
                case Shell shell:
                    OnSleep(shell.CurrentPage);
                    return;
                case BasePage basePage:
                    basePage.OnSleep();
                    break;
            }
        }
    }
}
