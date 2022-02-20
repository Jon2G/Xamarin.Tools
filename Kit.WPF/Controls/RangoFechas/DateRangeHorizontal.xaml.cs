using System.Windows;
using Kit.Controls.DateRange;

namespace Kit.WPF.Controls.RangoFechas
{
    /// <summary>
    /// Lógica de interacción para DateRangeHorizontal.xaml
    /// </summary>
    public partial class DateRangeHorizontal : ObservableUserControl
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

        public DateRangeHorizontal()
        {
            this.Rango = new ModeloRango();
            InitializeComponent();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //this.Fechas.TodasLasFechas = (bool)(sender as CheckBox).IsChecked;
        }
    }
}