using System.ComponentModel;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using View = Android.Views.View;

[assembly: ResolutionGroupName(Plugin.Xamarin.Tools.Shared.Effects.ViewLifecycleEffect.EffectGroupName)]
[assembly: ExportEffect(typeof(Plugin.Xamarin.Tools.Droid.Effects.ViewLifecycleEffect), Plugin.Xamarin.Tools.Shared.Effects.ViewLifecycleEffect.EffectName)]
namespace Plugin.Xamarin.Tools.Droid.Effects
{

    public class ViewLifecycleEffect : PlatformEffect
    {
        private View _nativeView;
        private Plugin.Xamarin.Tools.Shared.Effects.ViewLifecycleEffect _viewLifecycleEffect;

        protected override void OnAttached()
        {
            _viewLifecycleEffect = Element.Effects.OfType<Plugin.Xamarin.Tools.Shared.Effects.ViewLifecycleEffect>().FirstOrDefault();

            _nativeView = Control ?? Container;
            _nativeView.ViewAttachedToWindow += OnViewAttachedToWindow;
            _nativeView.ViewDetachedFromWindow += OnViewDetachedFromWindow;
        }

        protected override void OnDetached()
        {
            View view = Control ?? Container;
            _viewLifecycleEffect.RaiseUnloaded(Element);
            _nativeView.ViewAttachedToWindow -= OnViewAttachedToWindow;
            _nativeView.ViewDetachedFromWindow -= OnViewDetachedFromWindow;
        }

        private void OnViewAttachedToWindow(object sender, View.ViewAttachedToWindowEventArgs e) => _viewLifecycleEffect?.RaiseLoaded(Element);
        private void OnViewDetachedFromWindow(object sender, View.ViewDetachedFromWindowEventArgs e) => _viewLifecycleEffect?.RaiseUnloaded(Element);
    }
}

