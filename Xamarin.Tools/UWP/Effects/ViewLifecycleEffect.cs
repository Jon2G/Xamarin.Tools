using Plugin.Xamarin.Tools.UWP.Effects;
using System;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;


[assembly: ResolutionGroupName(Plugin.Xamarin.Tools.Shared.Effects.ViewLifecycleEffect.EffectGroupName)]
[assembly: ExportEffect(typeof(ViewLifecycleEffect), Plugin.Xamarin.Tools.Shared.Effects.ViewLifecycleEffect.EffectName)]
namespace Plugin.Xamarin.Tools.UWP.Effects
{
    public class ViewLifecycleEffect : PlatformEffect
    {
        private FrameworkElement _nativeView;
        private Plugin.Xamarin.Tools.Shared.Effects.ViewLifecycleEffect _viewLifecycleEffect;

        protected override void OnAttached()
        {
            _viewLifecycleEffect = Element.Effects.OfType<Plugin.Xamarin.Tools.Shared.Effects.ViewLifecycleEffect>().FirstOrDefault();
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