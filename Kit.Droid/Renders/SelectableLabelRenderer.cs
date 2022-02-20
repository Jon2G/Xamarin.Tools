﻿using Android.Content;
using Android.Views;
using Kit.Droid.Renders;
using Kit.Forms.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(SelectableLabel), typeof(SelectableLabelRenderer))]

namespace Kit.Droid.Renders
{
    public class SelectableLabelRenderer : EditorRenderer
    {
        /*
         If you, like me, face a known issue that text selection stops working when there is more than one line of text,
        the solution is to inherit the FormsEditText and reset Enabled property in OnAttachedToWindow callback
        (you might want to write more sophisticated solution if you need full support of Enabled property).
         */
        //protected override FormsEditText CreateNativeControl() =>new CustomEditText(Context);
        //public class CustomEditText : FormsEditText
        //{
        //    public CustomEditText(Context context) : base(context)
        //    {
        //    }

        //    protected override void OnAttachedToWindow()
        //    {
        //        base.OnAttachedToWindow();

        //        Enabled = false;
        //        Enabled = true;
        //    }
        //}
        public SelectableLabelRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
                return;

            Control.Background = null;
            Control.SetPadding(0, 0, 0, 0);
            Control.ShowSoftInputOnFocus = false;
            Control.SetTextIsSelectable(true);
            Control.CustomSelectionActionModeCallback = new CustomSelectionActionModeCallback();
            Control.CustomInsertionActionModeCallback = new CustomInsertionActionModeCallback();
        }

        private class CustomInsertionActionModeCallback : Java.Lang.Object, ActionMode.ICallback
        {
            public bool OnCreateActionMode(ActionMode mode, IMenu menu) => false;

            public bool OnActionItemClicked(ActionMode m, IMenuItem i) => false;

            public bool OnPrepareActionMode(ActionMode mode, IMenu menu) => true;

            public void OnDestroyActionMode(ActionMode mode) { }
        }

        private class CustomSelectionActionModeCallback : Java.Lang.Object, ActionMode.ICallback
        {
            private const int CopyId = Android.Resource.Id.Copy;

            public bool OnActionItemClicked(ActionMode m, IMenuItem i) => false;

            public bool OnCreateActionMode(ActionMode mode, IMenu menu) => true;

            public void OnDestroyActionMode(ActionMode mode) { }

            public bool OnPrepareActionMode(ActionMode mode, IMenu menu)
            {
                try
                {
                    var copyItem = menu.FindItem(CopyId);
                    var title = copyItem.TitleFormatted;
                    menu.Clear();
                    menu.Add(0, CopyId, 0, title);
                }
                catch
                {
                    // ignored
                }

                return true;
            }
        }
    }
}
