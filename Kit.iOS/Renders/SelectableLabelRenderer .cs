using Foundation;
using Kit.Forms.Controls;
using Kit.iOS.Renders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
[assembly: ExportRenderer(typeof(SelectableLabel), typeof(SelectableLabelRenderer))]
namespace Kit.iOS.Renders
{


    public class SelectableLabelRenderer : EditorRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            if (Control == null) return;

            Control.Selectable = true;
            Control.Editable = false;
            Control.ScrollEnabled = false;
            Control.TextContainerInset = UIEdgeInsets.Zero;
            Control.TextContainer.LineFragmentPadding = 0;
        }
    }
}