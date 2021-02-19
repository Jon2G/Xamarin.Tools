using Android.Content;
using Kit.Sql;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using Kit.Droid.Services;
using Kit.Services.Interfaces;
using Xamarin.Forms;
using ZXing.Mobile;
using Bitmap = Android.Graphics.Bitmap;
[assembly: Dependency(typeof(QRCode))]
namespace Kit.Droid.Services
{
    public class QRCode : IQRCode
    {
        public MemoryStream Generate(string Value, int Width = 350, int Height = 350, int Margin = 10)
        {
            try
            {
                BarcodeWriter barcodeWriter = new ZXing.Mobile.BarcodeWriter
                {
                    Format = ZXing.BarcodeFormat.QR_CODE,
                    Options = new ZXing.Common.EncodingOptions
                    {
                        Width = Width,
                        Height = Height,
                        Margin = Margin
                    }
                };

                barcodeWriter.Renderer = new ZXing.Mobile.BitmapRenderer();
                Bitmap bitmap = barcodeWriter.Write(Value);
                MemoryStream stream = new MemoryStream();
                bitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, stream);  // this is the diff between iOS and Android
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
