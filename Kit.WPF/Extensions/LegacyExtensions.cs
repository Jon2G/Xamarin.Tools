using Kit.WPF.Controls;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Kit
{
    public static class LegacyExtensions
    {
        [Obsolete("Use CrearImagen para mejorar el redimiento con la llamada asyncronica")]
        public static BitmapImage CreateImage(string path)
        {
            if (File.Exists(path))
            {
                BitmapImage myBitmapImage = new BitmapImage();
                myBitmapImage.BeginInit();
                myBitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                myBitmapImage.UriSource = new Uri(path);
                myBitmapImage.DecodePixelWidth = 320;
                myBitmapImage.EndInit();
                myBitmapImage.Freeze(); // important
                return myBitmapImage;
            }
            Log.Logger.Error($"Imagen no encontrada:{path}");
            return new BitmapImage();
        }

        public static async Task<BitmapImage> CrearImagen(string path, int escala = 320)
        {
            return (await MyImage.Generate(path, escala)).Native as BitmapImage;
        }

        public static object DiccionarioFind(this FrameworkElement sender, string ResourceKey)
        {
            object r = Application.Current.Resources[ResourceKey];
            return r ?? sender.FindResource(ResourceKey);
        }

        public static byte[] ReadAllBytes(this FileInfo file)
        {
            return File.ReadAllBytes(file.FullName);
        }

        public static ImageSource ByteToImage(byte[] imageData)
        {
            if (imageData is null)
            {
                return null;
            }
            BitmapImage biImg = new BitmapImage();
            MemoryStream ms = new MemoryStream(imageData);
            biImg.BeginInit();
            biImg.StreamSource = ms;
            biImg.EndInit();

            ImageSource imgSrc = biImg as ImageSource;

            return imgSrc;
        }

        public static BitmapSource ToImageSource(this System.Drawing.Bitmap bitmap)
        {
            BitmapSource bitmapSource = null;
            try
            {
                System.Drawing.Imaging.BitmapData bitmapData = bitmap.LockBits(
                    new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

                bitmapSource = BitmapSource.Create(
                    bitmapData.Width, bitmapData.Height,
                    bitmap.HorizontalResolution, bitmap.VerticalResolution,
                    PixelFormats.Bgr24, null,
                    bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

                bitmap.UnlockBits(bitmapData);
            }
            catch (Exception) { }
            return bitmapSource;
        }

        public static byte[] ImageToByte(this System.Drawing.Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        public static byte[] ImageToByte(this BitmapImage img)
        {
            MemoryStream memStream = new MemoryStream();
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(img));
            encoder.Save(memStream);
            return memStream.ToArray();
        }

        public static void SetPercent(this System.Windows.Controls.ProgressBar progressBar, double percentage)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                DoubleAnimation animation = new DoubleAnimation(percentage, TimeSpan.FromSeconds(2)) { };
                progressBar.BeginAnimation(RangeBase.ValueProperty, animation);
            });
        }

        public static string GetOsName(this OperatingSystem os_info)
        {
            RegistryKey reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            string productName = (string)reg.GetValue("ProductName");
            if (productName.StartsWith("Windows 10"))
            {
                return productName;
            }
            string version =
                os_info.Version.Major.ToString() + "." +
                os_info.Version.Minor.ToString();
            switch (version)
            {
                case "10.0": return "10/Server 2016";
                case "6.3": return "8.1/Server 2012 R2";
                case "6.2": return "8/Server 2012";
                case "6.1": return "7/Server 2008 R2";
                case "6.0": return "Server 2008/Vista";
                case "5.2": return "Server 2003 R2/Server 2003/XP 64-Bit Edition";
                case "5.1": return "XP";
                case "5.0": return "2000";
            }
            return "Unknown";
        }

        public static void Append(this StringBuilder s, params string[] parametros)
        {
            for (int i = 0; i < parametros.Length; i++)
                s.Append(parametros[i]);
        }

        /// <summary>
        /// Mueve el foco a los controles establecidos por el tab index y expande los combos donde establece el foco
        /// </summary>
        /// <param name="window"></param>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void NextControl(this Window window, object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (System.Windows.Input.Keyboard.FocusedElement is Control keyboardFocus)
                {
                    keyboardFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    switch (Keyboard.FocusedElement)
                    {
                        case TextBox text:
                            text.CaretIndex = text.Text.Length;
                            if (text.TemplatedParent is ComboBox ct)
                            {
                                if ((ct.SelectedItem ?? ct.Text).ToString() == "Seleccionar")
                                {
                                    ct.Text = string.Empty;
                                    ct.SelectedItem = null;
                                }
                            }
                            break;

                        case ComboBox combo:
                            if (!combo.IsEditable)
                            {
                                combo.IsDropDownOpen = true;
                            }
                            break;

                        case UserControl btnDash:
                            btnDash.Focus();
                            break;

                        default:
                            if (FocusManager.GetFocusedElement(window) is TextBox txt
                                && txt.TemplatedParent is ComboBox comb && !comb.IsEditable)
                            {
                                comb.IsDropDownOpen = true;
                            }
                            else if (FocusManager.GetFocusedElement(window) is DatePicker picker && picker.IsEnabled)
                            {
                                picker.IsDropDownOpen = true;
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Mueve el foco a los controles establecidos por el tab index y expande los combos donde establece el foco
        /// </summary>
        /// <param name="window"></param>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void NextControl(this UserControl window, object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (Keyboard.FocusedElement is Control keyboardFocus)
                {
                    keyboardFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    switch (Keyboard.FocusedElement)
                    {
                        case TextBox text:
                            text.CaretIndex = text.Text.Length;
                            if (text.TemplatedParent is ComboBox ct)
                            {
                                if ((ct.SelectedItem ?? ct.Text).ToString() == "Seleccionar")
                                {
                                    ct.Text = string.Empty;
                                    ct.SelectedItem = null;
                                }
                            }
                            break;

                        case ComboBox combo:
                            if (!combo.IsEditable)
                            {
                                combo.IsDropDownOpen = true;
                            }
                            break;

                        case UserControl btnDash:
                            btnDash.Focus();
                            break;

                        default:
                            if (FocusManager.GetFocusedElement(window) is TextBox txt
                                && txt.TemplatedParent is ComboBox comb && !comb.IsEditable)
                            {
                                comb.IsDropDownOpen = true;
                            }
                            else if (FocusManager.GetFocusedElement(window) is DatePicker picker && picker.IsEnabled)
                            {
                                picker.IsDropDownOpen = true;
                            }
                            break;
                    }
                }
            }
        }

        public static bool ExisteEnsamblado(this AppDomain Dominio, Type type)
        {
            try
            {
                Assembly ensamblado = type.Assembly;
                return (from a in AppDomain.CurrentDomain.GetAssemblies()
                        where a.FullName == ensamblado.FullName
                        select a).Any();
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Al determinar si un ensamblado existe");
            }

            return false;
        }

        public static childItem FindVisualChild<childItem>(this DependencyObject obj)
            where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                {
                    return (childItem)child;
                }
                else
                {
                    childItem childOfChild = child.FindVisualChild<childItem>();
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindParent<T>(parentObject);
        }

        public static DependencyObject GetScrollViewer(DependencyObject o)
        {
            // Return the DependencyObject if it is a ScrollViewer
            if (o is ScrollViewer)
            { return o; }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
            {
                var child = VisualTreeHelper.GetChild(o, i);

                var result = GetScrollViewer(child);
                if (result == null)
                {
                    continue;
                }
                else
                {
                    return result;
                }
            }
            return null;
        }

        public static System.Windows.Media.Color Oscurecer(this System.Windows.Media.Color Color, double CorrectionFactor)
        {
            double red = (double)Color.R;
            double green = (double)Color.G;
            double blue = (double)Color.B;

            if (CorrectionFactor < 0)
            {
                CorrectionFactor = 1 + CorrectionFactor;
                red *= CorrectionFactor;
                green *= CorrectionFactor;
                blue *= CorrectionFactor;
            }
            else
            {
                red = (255 - red) * CorrectionFactor + red;
                green = (255 - green) * CorrectionFactor + green;
                blue = (255 - blue) * CorrectionFactor + blue;
            }
            return System.Windows.Media.Color.FromArgb(Color.A, (byte)red, (byte)green, (byte)blue);
        }

        //public static System.Windows.Media.Color ToMediaColor(Xamarin.Forms.Color XColor)
        //{
        //    if (XColor.A == -1)
        //    {
        //        return System.Windows.Media.Colors.Transparent;
        //    }
        //    return ToMediaColor(XColor.ToHex());
        //}
        public static System.Windows.Media.Color ToMediaColor(string HexColor)
        {
            if (string.IsNullOrEmpty(HexColor))
            {
                return System.Windows.Media.Colors.Transparent;
            }
            return (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(HexColor);
        }
    }
}