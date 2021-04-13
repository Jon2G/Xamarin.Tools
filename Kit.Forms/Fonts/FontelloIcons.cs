using System;
using System.Collections.Generic;
using System.Text;
using Kit.Forms.Fonts;
using Xamarin.Forms;

[assembly: ExportFont("kiticons_1.ttf", Alias = FontelloIcons.Font)]
namespace Kit.Forms.Fonts
{
    public static class FontelloIcons
    {
        public const string Ok = "\ue803";
        public const string Cross = "\ue804";
        public const string Camera = "\ue800";
        public const string Font = "KitFontIcons";
    }
}
