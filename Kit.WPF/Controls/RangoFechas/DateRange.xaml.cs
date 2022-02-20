using Kit.Controls.DateRange;
using System.Windows;
using System.Windows.Controls;

namespace Kit.WPF.Controls.RangoFechas
{
    /// <summary>
    /// Lógica de interacción para DateRange.xaml
    /// </summary>
    public partial class DateRange : ObservableUserControl
    {
        #region Rango

        private ModeloRango _Rango;

        public ModeloRango Rango
        {
            get => _Rango;
            set
            {
                _Rango = value;
                Raise(() => Rango);
            }
        }

        #endregion Rango

        public Rango Fechas { get => (Rango)Rango; }

        public DateRange()
        {
            this.Rango = new ModeloRango();
            this.DataContext = this;
            InitializeComponent();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            this.Fechas.TodasLasFechas = (bool)(sender as CheckBox).IsChecked;
        }
    }
}