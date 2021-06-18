using System;
using System.Collections.Generic;
using System.Text;
using Android.App;
using Android.Content;
using Java.Lang;

// ReSharper disable once CheckNamespace
namespace Kit.Droid
{
    public static class ContextExtensions
    {
        public static bool IsServiceRunning(this Context context, Type serviceClass)
        {
            var service_class = Java.Lang.Class.FromType(serviceClass);
            ActivityManager manager = (ActivityManager)context.GetSystemService(Context.ActivityService);
#pragma warning disable CS0618 // Type or member is obsolete
            foreach (var service in manager.GetRunningServices(Integer.MaxValue))
#pragma warning restore CS0618 // Type or member is obsolete
            {
                if (service_class.Name == service.Service.ClassName)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
