using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Kit.Services.BarCode;
using Kit.Services.Interfaces;
using ZXing;
using ZXing.Common;
using ZXing.Rendering;

namespace Kit.WPF.Services
{
    public class BarCodeBuilder : IBarCodeBuilder
    {
        public MemoryStream Generate(BarcodeFormat Formato, string Value, int Width = 350, int Height = 350, int Margin = 10,
            EncodingOptions Options = null)
        {
            try
            {
                Options ??= new ZXing.Common.EncodingOptions();
                Options.Width = Width;
                Options.Height = Height;
                Options.Margin = Margin;

                BarcodeWriter barcodeWriter = new BarcodeWriter
                {
                    Format = Formato,
                    Options = Options 
                };

                barcodeWriter.Renderer = new BitmapRenderer();
                Bitmap bitmap = barcodeWriter.Write(Value);
                MemoryStream stream = new MemoryStream();
                bitmap.Save(stream,ImageFormat.Png);// this is the diff between iOS,Android,WPF
                stream.Position = 0;
                return stream;
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Al generar un código QR");
                return null;
            }

        }
    }
}
