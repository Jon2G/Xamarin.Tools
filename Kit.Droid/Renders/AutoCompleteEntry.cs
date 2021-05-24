using Android.Content;
using System;
using System.Collections.Generic;
using System.Text;
using Android.Views.InputMethods;
using Kit.Droid.Renders;
using Plugin.CurrentActivity;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
[assembly: ExportRenderer(typeof(Kit.Forms.Controls.AutoCompleteEntry), typeof(AutoCompleteEntry))]
namespace Kit.Droid.Renders
{
    public class AutoCompleteEntry : EntryRenderer
    {
        public AutoCompleteEntry(Context context) : base(context)
        {

        }
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
            if (Control != null)
            {
           
            }
        }
    
    }
}
