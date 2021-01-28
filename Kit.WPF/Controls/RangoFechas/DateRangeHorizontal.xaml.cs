using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Kit.WPF.Controls;
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
                Raise(()=>Rango);
            }
        }
        #endregion

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
