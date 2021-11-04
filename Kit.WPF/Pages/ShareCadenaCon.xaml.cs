using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using ZXing;

namespace Kit.WPF.Pages
{
    /// <summary>
    /// Interaction logic for ShareCadenaCon.xaml
    /// </summary>
    public partial class ShareCadenaCon : Window
    {
        public string Code { get; set; }
        public ShareCadenaCon(string Title, string Code)
        {
            this.Code = Code;
            InitializeComponent();
            this.TxtTitle.Text = Title;
            this.Img.Source =
            Kit.Tools.Instance.ImageExtensions.FromStream(() =>
                Kit.Tools.Instance.BarCodeBuilder.Generate(BarcodeFormat.QR_CODE, Code))?.Native as ImageSource;
        }

        private void Salvar(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Filter = "Image files (*.png, *.jpeg, *.jpe, *.jfif, *.jpg) | *.png; *.jpeg; *.jpe; *.jfif; *.jpg"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                ToImage(new FileInfo(saveFileDialog.FileName));
            }
        }

        public void ToImage(FileInfo file)
        {
            this.InvalidateVisual();
            
            int width = (int)Math.Round(this.Width);
            int height = (int)Math.Round(this.Height);

            this.Measure(new Size(this.Width, this.Height));
            this.Arrange(new Rect(new Size(this.Width, this.Height)));
            this.UpdateLayout();

            RenderTargetBitmap bmp =
                new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(this);
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));

            using (FileStream memory = new FileStream(file.FullName, FileMode.OpenOrCreate))
            {
                encoder.Save(memory);
            }
            Process.Start(file.FullName);
        }
    }
}
