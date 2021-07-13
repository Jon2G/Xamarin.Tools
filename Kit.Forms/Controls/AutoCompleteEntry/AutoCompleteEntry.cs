using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Kit.Forms.Services.Interfaces;
using Xamarin.Forms;

namespace Kit.Forms.Controls
{
    public class AutoCompleteEntry : Entry
    {
        public static readonly BindableProperty TextChangedCommandProperty =
            BindableProperty.Create(nameof(AutoCompleteEntry.TextChangedCommand), typeof(ICommand),
                typeof(AutoCompleteEntry), null);

        public ICommand TextChangedCommand
        {
            get { return (ICommand)GetValue(TextChangedCommandProperty); }
            set { SetValue(TextChangedCommandProperty, value); }
        }

        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable<string>),
                typeof(AutoCompleteEntry), null
                , propertyChanged: ItemsSourceChanged);

        private static void ItemsSourceChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            if (bindable is AutoCompleteEntry auto)
            {
                auto.SuggestionsView.ItemsSource = newvalue as IEnumerable<string>;
            }
        }

        public IEnumerable<string> ItemsSource
        {
            get => (IEnumerable<string>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        private readonly IKeyboardService KeyboardService;
        private readonly SuggestionsView SuggestionsView;

        public AutoCompleteEntry()
        {
            this.KeyboardService = DependencyService.Get<IKeyboardService>();
            SuggestionsView = new SuggestionsView();
            this.TextChanged += AutoCompleteEntry_TextChanged;
            this.Focused += AutoCompleteEntry_Focused; ;
            this.Unfocused += AutoCompleteEntry_Unfocused;
        }

        private void AutoCompleteEntry_Focused(object sender, FocusEventArgs e)
        {
            AutoCompleteEntry_TextChanged(sender, null);
        }

        private async void AutoCompleteEntry_Unfocused(object sender, FocusEventArgs e)
        {
            //if (SuggestionsView.IsDisplayed&&SuggestionsView.Selected!=this.Text)
            //    await SuggestionsView.Hide();
        }

        private async void AutoCompleteEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextChangedCommand is null)
            {
                return;
            }

            if (!SuggestionsView.IsDisplayed)
            {
                await SuggestionsView.Show(this);
            }
            TextChangedCommand?.Execute(this.Text);
            this.Focus();
            //this.KeyboardService.Show();
        }
    }
}