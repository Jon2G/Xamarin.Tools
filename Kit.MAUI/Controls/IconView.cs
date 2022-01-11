using Kit.MAUI.Fonts;
using Microsoft.Maui;using Microsoft.Maui.Controls;

namespace Kit.MAUI.Views
{
    public class IconView : ContentView
    {
        public virtual string Icon => FontelloIcons.Ok;
        public virtual string Title => string.Empty;

        public static readonly BindableProperty ToolbarItemProperty =
            BindableProperty.Create(
            propertyName: nameof(ToolbarItem),
            returnType: typeof(ToolbarItem),
            declaringType: typeof(ToolbarItem),
            defaultValue: null);

        public ToolbarItem ToolbarItem
        {
            get => (ToolbarItem)GetValue(ToolbarItemProperty);
            set
            {
                SetValue(ToolbarItemProperty, value);
                OnPropertyChanged();
            }
        }

        public IconView() : base()
        {
        }
    }
}