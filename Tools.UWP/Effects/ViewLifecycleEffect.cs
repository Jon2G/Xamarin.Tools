using Tools.UWP.Effects;
using System;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;
using BaseViewLifecycleEffect = Tools.Effects.ViewLifecycleEffect;

[assembly: ResolutionGroupName(Tools.Effects.ViewLifecycleEffect.EffectGroupName)]
[assembly: ExportEffect(typeof(ViewLifecycleEffect), Tools.Effects.ViewLifecycleEffect.EffectName)]
namespace Tools.UWP.Effects
{
    public class ViewLifecycleEffect : PlatformEffect
    {
        private FrameworkElement _nativeView;
        private BaseViewLifecycleEffect _viewLifecycleEffect;

        protected override void OnAttached()
        {
            _viewLifecycleEffect = Element.Effects.OfType<BaseViewLifecycleEffect>().FirstOrDefault();
            _nativeView = Container;

            _nativeView.Loaded += NativeViewOnLoaded;
            _nativeView.Unloaded += NativeViewOnUnloaded;
        }

        protected override void OnDetached()
        {
            _viewLifecycleEffect?.RaiseUnloaded(Element);
            _nativeView.Loaded -= NativeViewOnLoaded;
            _nativeView.Unloaded -= NativeViewOnUnloaded;
        }

        private void NativeViewOnLoaded(object sender, RoutedEventArgs routedEventArgs) => _viewLifecycleEffect?.RaiseLoaded(Element);
        private void NativeViewOnUnloaded(object sender, RoutedEventArgs routedEventArgs) => _viewLifecycleEffect?.RaiseUnloaded(Element);
    }
}