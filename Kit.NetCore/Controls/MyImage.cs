
using SQLHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Kit.Extensions;
using Kit.NetCore.Extensions;

namespace Kit.NetCore.Controls
{
    public class MyImage : System.Windows.Controls.Image
    {
        #region MySource
        public static readonly DependencyProperty MySourceProperty =
            DependencyProperty.Register(
                "MySource", typeof(string), typeof(MyImage),
                new FrameworkPropertyMetadata(
                    string.Empty,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (o, e) => ((MyImage)o).MySource = (string)e.NewValue));

        public void CambiarImagen(string filepath)
        {
            SetValue(MySourceProperty, filepath);
            Dispatcher.Invoke(() =>
            {
                UpdateImage();
            });
        }
        public string MySource
        {
            get => (string)GetValue(MySourceProperty);
            set
            {
                //if (AppData.Argumentos.IsInDesingMode)
                //{
                //    return;
                //}
                if (!string.IsNullOrEmpty(value))
                {
                    if (value[0] == '\\')
                    {
                        value = string.Concat(Tools.Instance.LibraryPath, value);
                    }
                }
                if (value == MySource) return;
                SetValue(MySourceProperty, value);
                UpdateImage();
            }
        }
        private ImageSource _currentImage;

        public ImageSource CurrentImage
        {
            get { return _currentImage; }
            set
            {
                if (Equals(value, _currentImage)) return;
                _currentImage = value;
                ImagenCambio();
            }
        }

        public byte[] Bytes
        {
            get
            {
                byte[] bytes = null;
                var bitmapSource = Source as BitmapSource;
                var encoder = new PngBitmapEncoder();
                if (bitmapSource != null)
                {
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                    using (var stream = new MemoryStream())
                    {
                        encoder.Save(stream);
                        bytes = stream.ToArray();
                    }
                }

                return bytes;
            }
        }

        private async void UpdateImage()
        {
            string file = MySource;
            // this is asynchronous and won't block UI
            // first generate rough preview
            if (Tools.Instance.IsInDesingMode)
            {
                if (!File.Exists(file))
                {
                    //MessageBox.Show(file);
                    return;
                }
                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        CurrentImage = new BitmapImage(new Uri(file));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                });

                return;
            }
            CurrentImage = await Generate(file, 320);
            // then generate quality preview
            //this.CurrentImage = await Generate(file, 1920);
        }
        public static Task<BitmapImage> Generate(string file, int scale)
        {
            return Task.Run(() =>
            {
                try
                {
                    if (Tools.Instance.IsInDesingMode)
                    {
                        return null;
                    }
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    //image.CacheOption = BitmapCacheOption.Default;
                    image.UriSource = new Uri(file);
                    image.DecodePixelWidth = scale;
                    image.EndInit();
                    image.Freeze(); // important
                    return image;
                }
                catch (Exception ex)
                {
                    Log.LogMe(ex, "Al cargar una imagen de MyImage");
                    return new BitmapImage();
                }
            });
        }
        private void ImagenCambio()
        {
            try
            {
                Source = CurrentImage;
            }
            catch (Exception ex)
            {
                Log.LogMe(ex, "En MyImage cambio");
            }
        }
        #endregion
        #region XSource
        public static readonly DependencyProperty XSourceProperty =
            DependencyProperty.Register(
                nameof(XSource), typeof(Xamarin.Forms.ImageSource), typeof(MyImage),
                new FrameworkPropertyMetadata(null,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            (o, e) => ((MyImage)o).XSource = (Xamarin.Forms.ImageSource)e.NewValue));
        private Xamarin.Forms.ImageSource _XSource;
        public Xamarin.Forms.ImageSource XSource
        {
            get => _XSource;
            set
            {
                _XSource = value;
                UpdateXImage();
            }

        }
        private async void UpdateXImage()
        {
            await Task.Yield();
            Source = Extensiones.ByteToImage(XSource.ImageToByte());
        }
        #endregion
        public MyImage() : base()
        {

        }

        ~MyImage()
        {
            _currentImage = null;
        }
    }
}
