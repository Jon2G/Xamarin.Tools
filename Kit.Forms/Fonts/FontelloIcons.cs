using Kit.Forms.Fonts;
using Xamarin.Forms;

[assembly: ExportFont(FontelloIcons.FontResourceName, Alias = FontelloIcons.Font,EmbeddedFontResourceId = "Kit.Forms.Fonts.FontelloIcons.font.kiticons4.ttf")]
namespace Kit.Forms.Fonts
{
    public static class FontelloIcons
    {
        public const string Ok = "\uE803";
        public const string Cross = "\uE804";
        public const string Camera = "\uE800";
        public const string RightArrow = "\uF105";
        public const string ThreeDots = "\uF0C9";
        public const string Font = "KitFontIcons";
        public const string FontResourceName = "kiticons4.ttf";
    }
}
