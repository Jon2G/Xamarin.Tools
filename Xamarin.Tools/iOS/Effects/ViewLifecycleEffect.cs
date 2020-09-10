using System;
using System.ComponentModel;
using System.Linq;
using Foundation;
using Plugin.Xamarin.Tools.iOS.Effects;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;


[assembly: ResolutionGroupName(Plugin.Xamarin.Tools.Shared.Effects.ViewLifecycleEffect.EffectGroupName)]
[assembly: ExportEffect(typeof(Plugin.Xamarin.Tools.iOS.Effects.ViewLifecycleEffect), Plugin.Xamarin.Tools.Shared.Effects.ViewLifecycleEffect.EffectName)]

namespace Plugin.Xamarin.Tools.iOS.Effects
{
    public class ViewLifecycleEffect : PlatformEffect
    {
        private const NSKeyValueObservingOptions ObservingOptions =
            NSKeyValueObservingOptions.New|NSKeyValueObservingOptions.Old|
            NSKeyValueObservingOptions.Initial | NSKeyValueObservingOptions.OldNew | NSKeyValueObservingOptions.Prior;

        private Plugin.Xamarin.Tools.Shared.Effects.ViewLifecycleEffect _viewLifecycleEffect;
        private IDisposable _isLoadedObserverDisposable;

        protected override void OnAttached()
        {
            _viewLifecycleEffect = Element.Effects.OfType<Plugin.Xamarin.Tools.Shared.Effects.ViewLifecycleEffect>().FirstOrDefault();

            UIView nativeView = Control ?? Container;
            _isLoadedObserverDisposable = nativeView?.AddObserver("superview", ObservingOptions, IsViewLoadedObserver);

        }

        protected override void OnDetached()
        {
            _viewLifecycleEffect.RaiseUnloaded(Element);
            _isLoadedObserverDisposable.Dispose();
        }

        private void IsViewLoadedObserver(NSObservedChange nsObservedChange)
        {
            if (!nsObservedChange.NewValue.Equals(NSNull.Null))
            {
                _viewLifecycleEffect?.RaiseLoaded(Element);
            }
            else
            {
                if (nsObservedChange.OldValue != null && !nsObservedChange.OldValue.Equals(NSNull.Null))
                {
                    _viewLifecycleEffect?.RaiseUnloaded(Element);
                }
            }


        }
    }
}
