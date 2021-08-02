using Kit.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Kit;

using Kit;

namespace Kit.WPF.Controls
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
                        value = string.Concat(Kit.Tools.Instance.LibraryPath, value);
                    }
                }
                if (value == MySource) return;
                SetValue(MySourceProperty, value);
                UpdateImage();
            }
        }

        public static readonly DependencyProperty CurrentImageProperty =
            DependencyProperty.Register(
                "CurrentImage", typeof(Kit.Controls.CrossImage.CrossImage), typeof(MyImage),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (o, e) => ((MyImage)o).CurrentImage = (Kit.Controls.CrossImage.CrossImage)e.NewValue));

        private Kit.Controls.CrossImage.CrossImage _CurrentImage;

        public Kit.Controls.CrossImage.CrossImage CurrentImage
        {
            get { return _CurrentImage; }
            set
            {
                if (Equals(value, _CurrentImage)) return;
                _CurrentImage = value;
                ImagenCambio();
            }
        }

        public byte[] Bytes
        {
            get
            {
                byte[] bytes = null;
                BitmapSource bitmapSource = Source as BitmapSource;
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                if (bitmapSource != null)
                {
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                    using (MemoryStream stream = new MemoryStream())
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
            if (Kit.AbstractTools.IsInDesingMode)
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
                        CurrentImage = new Kit.WPF.Controls.CrossImage.CrossImage()
                        {
                            Native = new BitmapImage(new Uri(file))
                        };
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

        public static Task<CrossImage.CrossImage> Generate(string file, int scale)
        {
            return Task.Run(() =>
            {
                try
                {
                    if (AbstractTools.IsInDesingMode)
                    {
                        return null;
                    }
                    Controls.CrossImage.CrossImage cross = new Controls.CrossImage.CrossImage();
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    //image.CacheOption = BitmapCacheOption.Default;
                    image.UriSource = new Uri(file);
                    image.DecodePixelWidth = scale;
                    image.EndInit();
                    image.Freeze(); // important
                    cross.Native = image;
                    return cross;
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "Al cargar una imagen de MyImage ruta: {0}", file);
                    return new CrossImage.CrossImage()
                    {
                        Native = new BitmapImage()
                    };
                }
            });
        }

        private void ImagenCambio()
        {
            try
            {
                if (CurrentImage?.Native is null)
                {
                    Source = null;
                    return;
                }
                Source = (BitmapSource)CurrentImage?.Native;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "En MyImage cambio");
            }
        }

        #endregion MySource

        public MyImage() : base()
        {
        }

        ~MyImage()
        {
            _CurrentImage = null;
        }
    }
}