using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using Kit.Controls.CrossBrush;


namespace Kit.WPF.Controls.Brush
{
    public class Brush : CrossBrush<System.Windows.Media.Brush, Kit.WPF.Controls.Brush.Color>
    {
        public Brush(string ResourceName) : base(ResourceName)
        {

        }
        public Brush()
        {

        }
        public override System.Windows.Media.Brush ToNaviteBrush()
        {
            System.Windows.Media.Brush native = null;
            switch (this.BrushType)
            {
                case BrushType.Solid:
                    native = new SolidColorBrush();
                    if (this.Stops.Any())
                    {
                        ((SolidColorBrush)native).Color =
                            (System.Windows.Media.Color)this.Stops.First().Color.ToNativeColor();
                    }
                    break;
                case BrushType.Radial:
                    native = new RadialGradientBrush()
                    {
                        RadiusX = RadiusX,
                        RadiusY = RadiusY
                    };
                    CopyStops(native as GradientBrush, this.Stops);
                    break;
                case BrushType.Linear:
                    native = new LinearGradientBrush();
                    CopyStops(native as GradientBrush, this.Stops);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return native;
        }

        private void CopyStops(System.Windows.Media.GradientBrush native, GradientStopCollection<Color> stops)
        {
            foreach (var stop in stops)
            {
                var Color = (System.Windows.Media.Color)stop.Color.ToNativeColor();
                native.GradientStops.Add(new GradientStop(Color, stop.Offset));
            }
        }

        public override void Apply()
        {
            Application.Current.Resources[ResourceKey] = this.ToNaviteBrush();
        }

        public override void LoadFromResource()
        {
            this.Stops.Clear();
            if (Application.Current.Resources[ResourceKey] is System.Windows.Media.SolidColorBrush s)
            {
                var c = s.Color;
                string color = $"#{c.R:X2}{c.G:X2}{c.B:X2}";
                this.Stops.Add(new GradientStop<Color>(new Color().From(color), 0));
                this.BrushType = BrushType.Solid;
            }
            if (Application.Current.Resources[ResourceKey] is System.Windows.Media.GradientBrush g)
            {
                foreach (var stop in g.GradientStops)
                {
                    var c = stop.Color;
                    string color = $"#{c.R:X2}{c.G:X2}{c.B:X2}";
                    this.Stops.Add(new GradientStop<Color>(new Color().From(color), (float)stop.Offset));
                }

                switch (g)
                {
                    case LinearGradientBrush:
                        this.BrushType = BrushType.Linear;
                        break;
                    case RadialGradientBrush r:
                        this.RadiusX = r.RadiusX;
                        this.RadiusY = r.RadiusY;
                        this.BrushType = BrushType.Radial;
                        break;
                }

            }
            return;
        }
    }
}
