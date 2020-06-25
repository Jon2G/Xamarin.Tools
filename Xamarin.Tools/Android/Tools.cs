using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Plugin.Xamarin.Tools.Shared;
using Plugin.Xamarin.Tools.Shared.Logging;

namespace Plugin.Xamarin.Tools.Android
{
    public partial class Tools
    {
        public static void Init(Func<Activity> topActivityFactory)
        {
            
            //Instance = new ToolsImp(topActivityFactory);
        }


        /// <summary>
        /// Initialize android user dialogs
        /// </summary>
        public static void Init(Application app)
        {
          //  ActivityLifecycleCallbacks.Register(app);
            //Init(() => ActivityLifecycleCallbacks.CurrentTopActivity);
       
        }


        /// <summary>
        /// Initialize android user dialogs
        /// </summary>
        public static ITools Init(Activity activity)
        {
            Acr.UserDialogs.UserDialogs.Init(activity);
            AppDomain.CurrentDomain.UnhandledException +=Log.CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += Log.TaskSchedulerOnUnobservedTaskException;

            return Tools.Instance;
            //ActivityLifecycleCallbacks.Register(activity);
            //Init(() => ActivityLifecycleCallbacks.CurrentTopActivity);
        }


        static ITools currentInstance;
        public static ITools Instance
        {
            get
            {
                if (currentInstance == null)
                    throw new ArgumentException("[Shared.Tools] In android, you must call UserDialogs.Init(Activity) from your first activity OR UserDialogs.Init(App) from your custom application OR provide a factory function to get the current top activity via UserDialogs.Init(() => supply top activity)");

                return currentInstance;
            }
            set => currentInstance = value;
        }
    }
}
