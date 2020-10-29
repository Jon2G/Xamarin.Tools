using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.WPF.Controls.RangoFechas
{
    public class Rango : ViewModelBase<Rango>
    {
        private DateTime? _Inicio;
        public DateTime? Inicio
        {
            get => _Inicio;
            set
            {
                _Inicio = value;
                OnPropertyChanged(nameof(Inicio));
            }
        }
        private DateTime? _Fin;
        public DateTime? Fin
        {
            get => _Fin;
            set
            {
                _Fin = value;
                OnPropertyChanged(nameof(Fin));
            }
        }
        private bool _TodasLasFechas;
        public bool TodasLasFechas
        {
            get => _TodasLasFechas;
            set
            {
                _TodasLasFechas = value;
                OnPropertyChanged(nameof(TodasLasFechas));
                if (TodasLasFechas)
                {
                    Fin =
                    Inicio = null;
                }
                else
                {
                    Inicio = DateTime.Now;
                    Fin = DateTime.Now;
                }
            }
        }
        public bool Cancelado { get; set; }
        public bool Excel { get; set; }

        public Rango()
        {
            Excel = false;
            Cancelado = false;
            Inicio = DateTime.Now;
            Fin = DateTime.Now;
        }
    }
}
