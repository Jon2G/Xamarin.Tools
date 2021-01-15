
using Kit.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace Kit.Forms.Controls.NotificationBar
{
    public class Notificacion : ViewModelBase<Notificacion>
    {
        public ImageSource _Imagen;
        [TypeConverter(typeof(Converters.MyImageSourceConverter))]
        public ImageSource Imagen
        {
            get => _Imagen;
            set
            {
                _Imagen = value;
                OnPropertyChanged();
            }
        }

        public string Color { get; private set; }
        private string _Texto;
        public string Texto { get => _Texto; set { _Texto = value; OnPropertyChanged(); } }
        public ICommand Command { get; private set; }
        public event EventHandler OnTapped;
        public Notificacion(string Icono, string Color, string Texto)
        {
            this.Imagen = Icono;
            this.Color = Color;
            this.Texto = Texto;
            this.Command = new CommonCommand(x => Click());
        }

        private bool Click()
        {
            OnTapped?.Invoke(this, EventArgs.Empty);
            return true;
        }
    }
}
