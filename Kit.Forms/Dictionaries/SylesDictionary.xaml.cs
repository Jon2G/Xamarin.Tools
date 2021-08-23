using Kit.Sql.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Kit.Forms.Dictionaries
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