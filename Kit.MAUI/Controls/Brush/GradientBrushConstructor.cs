using Kit.Controls.CrossBrush;

namespace Kit.MAUI.Controls.Brush
{
    public static class GradientBrushConstructor
    {
        public static CustomBackground
            <Microsoft.Maui.Controls.Brush,
                Kit.MAUI.Controls.Brush.Brush,
                Kit.MAUI.Controls.Brush.Color>
            Get<T>
            (
                string MainBackround = "MainBackround",
                string DarkBackround = "DarkBackround")
            where T : CustomBackground<Microsoft.Maui.Controls.Brush,
                Kit.MAUI.Controls.Brush.Brush,
                Kit.MAUI.Controls.Brush.Color>
        {
            return CustomBackground<Microsoft.Maui.Controls.Brush,
                    Kit.MAUI.Controls.Brush.Brush, 
                    Kit.MAUI.Controls.Brush.Color>
                .Get<T>(MainBackround, DarkBackround);
        }
    }
}
