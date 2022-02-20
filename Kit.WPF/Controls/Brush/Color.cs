namespace Kit.WPF.Controls.Brush
{
    public class Color : Kit.Controls.CrossBrush.Color
    {
        private System.Windows.Media.Color NativeColor;
        public override double R
        {
            get => NativeColor.R;
            set => NativeColor = System.Windows.Media.Color.FromArgb((byte)A, (byte)value, (byte)G, (byte)B);
        }

        public override double G
        {
            get => NativeColor.G;
            set => NativeColor = System.Windows.Media.Color.FromArgb((byte)A, (byte)R, (byte)value, (byte)B);

        }
        public override double B
        {
            get => NativeColor.B;
            set => NativeColor = System.Windows.Media.Color.FromArgb((byte)A, (byte)R, (byte)G, (byte)value);

        }

        public override double A
        {
            get => NativeColor.A;
            set => NativeColor = System.Windows.Media.Color.FromArgb((byte)value, (byte)R, (byte)G, (byte)B);
        }

        public override Kit.Controls.CrossBrush.Color From(string v)
        {
            var color = System.Drawing.ColorTranslator.FromHtml(v);
            return new Color()
            {
                NativeColor = new System.Windows.Media.Color()
                {
                    A = color.A,
                    R = color.R,
                    G = color.G,
                    B = color.B
                }
            };

        }

        public override object ToNativeColor()
        {
            return NativeColor;
        }
    }
}
