using System;
using System.Collections.Generic;
using System.Text;
using Kit.Sql.Attributes;
using Xamarin.CommunityToolkit.Behaviors;
using Microsoft.Maui;using Microsoft.Maui.Controls;

namespace Kit.MAUI.Validations
{
    [Preserve]
    public static class ControlValidation
    {
        public static readonly BindableProperty BindablePropertyProperty =
            BindableProperty.CreateAttached(
                "BindableProperty",
                typeof(BindableProperty),
                typeof(ControlValidation),
                null,
                propertyChanged: OnAttachBehaviorChanged);


        public static BindableProperty GetBindableProperty(BindableObject view)
        {
            return (BindableProperty)view.GetValue(BindablePropertyProperty);
        }

        public static void SetBindableProperty(BindableObject view, bool value)
        {
            view.SetValue(BindablePropertyProperty, value);
        }

        static void OnAttachBehaviorChanged(BindableObject view, object oldValue, object newValue)
        {
            View AttachedView = view as View;
            if (AttachedView is null)
            {
                return;
            }
            BindableProperty bindableProperty = newValue as BindableProperty;
            if (bindableProperty is not null)
            {
                AttachedView.Behaviors.Add(new ControlValidationBehavior() { BindableProperty = bindableProperty, ShowErrorMessage = true });

            }
        }

    }
}
