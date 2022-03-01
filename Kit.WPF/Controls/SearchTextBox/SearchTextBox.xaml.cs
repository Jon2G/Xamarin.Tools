using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Key = System.Windows.Input.Key;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

// ReSharper disable once CheckNamespace
namespace Kit.WPF.Controls
{
    /// <summary>
    /// Interaction logic for SearchTextBox.xaml
    /// </summary>
    public partial class SearchTextBox : ObservableUserControl
    {
        public event EventHandler OnItemSelected;
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(SearchTextBox), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty ProviderProperty = DependencyProperty.Register(nameof(Provider), typeof(ISearchTextBoxProvider), typeof(SearchTextBox), new FrameworkPropertyMetadata(null));
        //public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(SearchTextBox), new FrameworkPropertyMetadata(string.Empty));
        public static readonly DependencyProperty BusquedaProperty = DependencyProperty.Register(nameof(Busqueda), typeof(string), typeof(SearchTextBox), new FrameworkPropertyMetadata(string.Empty));
        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);

            set => SetValue(SelectedItemProperty, value);
        }
        public ISearchTextBoxProvider Provider
        {
            get => (ISearchTextBoxProvider)GetValue(ProviderProperty);

            set => SetValue(ProviderProperty, value);
        }
        public string Busqueda
        {
            get => (string)GetValue(BusquedaProperty);

            set => SetValue(BusquedaProperty, value);
        }
        public bool TrueId
        {
            get; set;
        }
        public SearchTextBox()
        {
            InitializeComponent();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = Pressed(e.Key);
        }
        private bool Pressed(Key Key)
        {
            switch (Key)
            {
                case Key.Down:
                    if (Provider.IsOpen)
                    {
                        Provider.LoadSuggestions(TextBox.Text);
                        Pressed(Key.Enter);
                        return true;
                    }
                    Provider.IsOpen = true;
                    Provider.LoadSuggestions(TextBox.Text);
                    return true;
                case Key.Enter:
                    this.Datagrid.Focus();
                    Keyboard.Focus(this.Datagrid);
                    this.Datagrid.SelectedItem = this.Provider.Suggestions.FirstOrDefault();
                    this.Datagrid.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    return true;
                case Key.Escape:
                    Close_Click(null, null);
                    return true;
            }
            return false;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Provider.IsOpen = false;

        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Provider.IsOpen = false;
        }

        private void TextBox_GotMouseCapture(object sender, MouseEventArgs e)
        {
            this.TextBox.SelectAll();
        }

        private void DataGrid_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    Enter(e);
                    break;
            }
        }
        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Enter(e);
        }
        private void Enter(InputEventArgs e)
        {
            if (Datagrid.SelectedItem is ISearchTextBox entry)
            {
                this.Busqueda = entry.SelectedText;
            }
            else
            {
                this.Busqueda = Datagrid.SelectedItem.ToString();
            }
            if (this.OnItemSelected != null)
            {
                this.Busqueda = string.Empty;
                this.OnItemSelected.Invoke(this.Datagrid.SelectedItem, e);
            }
            this.TextBox.Text = this.Busqueda;
            Provider.IsOpen = false;
            e.Handled = true;

        }


        private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.TextBox.Text?.Trim()))
            {
                this.Busqueda = string.Empty;
            }
        }
    }
}
