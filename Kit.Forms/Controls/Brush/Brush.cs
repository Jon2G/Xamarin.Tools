using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kit.Controls.CrossBrush;
using Xamarin.Forms;

namespace Kit.Forms.Controls.Brush
{
    public class Brush : CrossBrush<Xamarin.Forms.Brush, Color>
    {
        public override Xamarin.Forms.Brush ToNaviteBrush()
        {
            Xamarin.Forms.Brush native = null;
            switch (this.BrushType)
            {
                case BrushType.Solid:
                    native = new SolidColorBrush();
                    if (this.Stops.Any())
                    {
                        ((SolidColorBrush)native).Color =
                            (Xamarin.Forms.Color)this.Stops.First().Color.ToNativeColor();
                    }
                    break;
                case BrushType.Radial:
                    native = new RadialGradientBrush()
                    {
                        Radius = (new List<double>() { RadiusX, RadiusY }).Max()
                    };
                    break;
                case BrushType.Linear:
                    native = new LinearGradientBrush();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return native;
        }

        public override void Apply()
        {
            throw new NotImplementedException();
        }

        public override void LoadFromResource()
        {
            throw new NotImplementedException();
        }
    }
}
