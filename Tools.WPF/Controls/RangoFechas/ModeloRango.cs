using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.WPF.Controls.RangoFechas
{
    public class ModeloRango : Rango
    {
        private DateTime _MinDate;
        private DateTime _InicioMaxDate;
        public DateTime InicioMaxDate
        {
            set { _InicioMaxDate = value; OnPropertyChanged(nameof(InicioMaxDate)); }
            get
            {
                return _InicioMaxDate;
            }
        }
        private DateTime _FinMaxDate;
        public DateTime FinMaxDate
        {
            set { _FinMaxDate = value; OnPropertyChanged(nameof(FinMaxDate)); }
            get
            {
                return _FinMaxDate;
            }
        }
        public DateTime MinDate
        {
            set { _MinDate = value; OnPropertyChanged(nameof(MinDate)); }
            get
            {
                return _MinDate;
            }
        }
        public ModeloRango() : base()
        {
            FinMaxDate =
            InicioMaxDate = DateTime.Now;
            PropertyChanged += ModeloRango_PropertyChanged;
        }

        private void ModeloRango_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Inicio):
                    if (Inicio != null)
                    {
                        if (Fin < Inicio)
                        {
                            Fin = Inicio;
                        }
                        MinDate = (DateTime)Inicio;
                    }
                    break;
                case nameof(Fin):
                    if (Inicio != null)
                    {
                        if (Inicio > Fin)
                        {
                            Fin = Inicio;
                        }
                    }
                    break;
            }
        }
    }
}
