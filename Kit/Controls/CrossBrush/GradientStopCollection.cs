using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Kit.Controls.CrossBrush
{
    public sealed class GradientStopCollection<C> : ObservableCollection<GradientStop<C>> where C : Color, new()
    {

    }
}
