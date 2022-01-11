using Kit.Sql.Attributes;
using System;
using Microsoft.Maui;using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Kit.MAUI.Dictionaries
{
    [XamlCompilation(XamlCompilationOptions.Compile), Preserve()]
    public partial class SylesDictionary : ResourceDictionary
    {
        public SylesDictionary()
        {
            InitializeComponent();
        }
        public static void UpdateCustomToolbarColor()
        {
            if(Application.Current.Resources["ToolbarDynamicColor"] is null)
            {
                throw new MissingMemberException("¿Are you missing merged SylesDictionary on App?");
            }
            Application.Current.Resources["ToolbarDynamicColor"]
                 = Kit.Daemon.Daemon.OffLine ?
                 Application.Current.Resources["ToolbarDangerColor"] :
                 Application.Current.Resources["ToolbarRegularColor"];
        }
    }
}