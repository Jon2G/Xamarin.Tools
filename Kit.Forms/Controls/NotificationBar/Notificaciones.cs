
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Kit.Forms.Controls.NotificationBar
{
    public class Notificaciones : ViewModelBase<Notificaciones>
    {
        public static Notificaciones Instance
        {
            get; set;
        }

        public static Notificaciones Init(string Color = "#37FFFFFF")
        {
            NotificationBar.Notificaciones.Instance= new NotificationBar.Notificaciones(Color);
            return Instance;
        }

        private string _Color;
        public string Color
        {
            get => _Color;
            set
            {
                _Color = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<Notificacion> Elementos { get; private set; }
        private readonly Dictionary<object, INotificaciones> NotificationsBars;
        public Notificaciones(string Color)
        {
            this.Color = Color;
            this.Elementos = new ObservableCollection<Notificacion>();
            this.NotificationsBars = new Dictionary<object, INotificaciones>();
        }
        ~Notificaciones()
        {

        }

        public void Unsuscribe(object owner)
        {
            if (NotificationsBars.ContainsKey(owner))
            {
                NotificationsBars.Remove(owner);
            }
        }
        public void Suscribe(object owner, INotificaciones customToolBar)
        {
            customToolBar.Refresh();
            if (NotificationsBars.ContainsKey(owner))
            {
                return;
            }
            NotificationsBars.Add(owner, customToolBar);
        }

        public void Refresh()
        {
            foreach (KeyValuePair<object, INotificaciones> bar in NotificationBar.Notificaciones.Instance.NotificationsBars)
            {
                bar.Value.Refresh();
            }
        }

        private async Task Notificar(Notificacion Notificacion)
        {
            await Device.InvokeOnMainThreadAsync(() =>
            {
                this.Elementos.Add(Notificacion);
            });
        }
        private async Task Remove(Notificacion notificacion)
        {
            await Device.InvokeOnMainThreadAsync(() =>
            {
                this.Elementos.Remove(notificacion);
            });
        }
    }
}
