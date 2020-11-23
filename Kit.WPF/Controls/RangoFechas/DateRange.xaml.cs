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

namespace Kit.WPF.Controls.RangoFechas
{
    /// <summary>
    /// Lógica de interacción para DateRange.xaml
    /// </summary>
    public partial class DateRange : ObservableUserControl
    {
        #region Buscador
        private Rango _Rango;
        public Rango Rango
        {
            get => _Rango;
            set
            {
                _Rango = value;
                OnPropertyChanged();
            }
        }
        public static readonly DependencyProperty RangoProperty =
            DependencyProperty.Register(
                nameof(DateRange.Rango), typeof(Rango), typeof(DateRange),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        #endregion

        public Rango Fechas { get => (Rango)Rango; }
        public DateRange()
        {
            this.DataContext = this;
            this.Rango = new ModeloRango();
            InitializeComponent();

        }
    }
}
