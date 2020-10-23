using System.ComponentModel;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using View = Android.Views.View;
using ViewLifecycleEffectBase = Tools.Effects.ViewLifecycleEffect;
[assembly: ResolutionGroupName(Tools.Effects.ViewLifecycleEffect.EffectGroupName)]
[assembly: ExportEffect(typeof(Tools.Droid.Effects.ViewLifecycleEffect), Tools.Effects.ViewLifecycleEffect.EffectName)]
namespace Tools.Droid.Effects
{

    public class ViewLifecycleEffect : PlatformEffect
    {
        private View _nativeView;
        private ViewLifecycleEffectBase _viewLifecycleEffect;

        protected override void OnAttached()
        {
            _viewLifecycleEffect = Element.Effects.OfType<ViewLifecycleEffectBase>().FirstOrDefault();

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

