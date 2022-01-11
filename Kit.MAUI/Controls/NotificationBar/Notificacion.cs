using System;
using System.Windows.Input;
using Kit.Model;
using Microsoft.Maui;using Microsoft.Maui.Controls;

namespace Kit.MAUI.Controls.NotificationBar
{
    public class Notificacion : ModelBase
    {
        public ImageSource _Imagen;

        [System.ComponentModel.TypeConverter(typeof(Converters.MyImageSourceConverter))]
        public ImageSource Imagen
        {
            get => _Imagen;
            set
            {
                _Imagen = value;
                Raise(() => Imagen);
            }
        }

        public string Color { get; private set; }
        private string _Texto;
        public string Texto { get => _Texto; set { _Texto = value; Raise(() => Texto); } }
        public ICommand Command { get; private set; }

        public event EventHandler OnTapped;

        public Notificacion(string Icono, string Color, string Texto)
        {
            this.Imagen = Icono;
            this.Color = Color;
            this.Texto = Texto;
            this.Command = new Command<object>(x => Click());
        }

        private bool Click()
        {
            OnTapped?.Invoke(this, EventArgs.Empty);
            return true;
        }
    }
}