using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Plugin.Xamarin.Tools.Shared.Classes
{
    public static class Extensiones
    {
        public static ImageSource ByteToImage(this byte[] ByteArray)
        {
            return ImageSource.FromStream(() => new MemoryStream(ByteArray));
        }

        public static byte[] ImageToByte(this ImageSource ImageSource)
        {
            StreamImageSource streamImageSource = (StreamImageSource)ImageSource;
            System.Threading.CancellationToken cancellationToken = System.Threading.CancellationToken.None;
            Task<Stream> task = streamImageSource.Stream(cancellationToken);
            MemoryStream stream = task.Result as MemoryStream;
            return stream.ToArray();
        }

        public static int FindIndexOf<T>(this ObservableCollection<T> modificadoresSeleccionados, Func<T, bool> p)
        {
            for (int i = 0; i < modificadoresSeleccionados.Count; i++)
            {
                T elemento = modificadoresSeleccionados[i];
                if (p.Invoke(elemento))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
