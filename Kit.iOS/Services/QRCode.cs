using Kit.Services.Interfaces;
using Kit.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UIKit;
using ZXing.Mobile;

namespace Kit.iOS.Services
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
                UIImage bitmap = barcodeWriter.Write(Value);
                MemoryStream stream = (MemoryStream)bitmap.AsPNG().AsStream();
                return stream;
            }
            catch (Exception ex)
            {
                Log.LogMe(ex, "Al generar un código QR");
                return null;
            }

        }
    }
}

