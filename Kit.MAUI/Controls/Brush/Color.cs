using Microsoft.Maui.Graphics;
namespace Kit.MAUI.Controls.Brush
{
    public class Color : Kit.Controls.CrossBrush.Color
    {
        private Microsoft.Maui.Graphics.Color NativeColor;
        public override double R
        {
            get => NativeColor.Red;
            set => NativeColor = Microsoft.Maui.Graphics.Color.FromRgba(value, G, B, A);
        }

        public override double G
        {
            get => NativeColor.Green;
            set => NativeColor = Microsoft.Maui.Graphics.Color.FromRgba(R, value, B, A);

        }
        public override double B
        {
            get => NativeColor.Blue;
            set => NativeColor = Microsoft.Maui.Graphics.Color.FromRgba(R, G, value, A);

        }
        public override double A
        {
            get => NativeColor.Alpha;
            set => NativeColor = Microsoft.Maui.Graphics.Color.FromRgba(R, G, B, value);
        }
        public override Kit.Controls.CrossBrush.Color From(string v)
        {
            return new Color()
            {
                NativeColor = Microsoft.Maui.Graphics.Color.FromArgb(v)
            };
        }

        public override object ToNativeColor()
        {
            return NativeColor;
        }
    }
}
