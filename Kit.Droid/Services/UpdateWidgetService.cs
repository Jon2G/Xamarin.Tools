using Kit.Droid.Services;
using Kit.Forms.Services.Interfaces;
using Kit.Sql.Reflection;
using Plugin.CurrentActivity;
using Xamarin.Forms;

[assembly: Dependency(typeof(UpdateWidgetService))]
namespace Kit.Droid.Services
{
    public class UpdateWidgetService : IUpdateWidget
    {
        public void UpdateWidget(string AssemblyName, string AppWidgetProviderFullClassName)
        {
            using (ReflectionCaller caller = new ReflectionCaller(AssemblyName))
            {
                var context = CrossCurrentActivity.Current.AppContext;
                var type =caller.GetType(AppWidgetProviderFullClassName); 
                context.UpdateWidget(type);
            }
        }
    }
}
