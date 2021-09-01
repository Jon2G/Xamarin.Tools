using Kit.Controls.CrossBrush;

namespace Kit.WPF.Controls.Brush
{
    public static class GradientBrushConstructor
    {
        public static CustomBackground
            <System.Windows.Media.Brush, Kit.WPF.Controls.Brush.Brush, Kit.WPF.Controls.Brush.Color> 
            Get<T>
            (
                string MainBackround = "MainBackround",
                string DarkBackround = "DarkBackround") 
            where T : CustomBackground<System.Windows.Media.Brush, Brush, Color>
        {
            return CustomBackground<System.Windows.Media.Brush, Kit.WPF.Controls.Brush.Brush, Kit.WPF.Controls.Brush.Color>
                .Get<T>(MainBackround, DarkBackround);
        }
    }
}
