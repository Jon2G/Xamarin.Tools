using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ImageProcessing.JPEGCodec;
using ImageProcessing.PNGCodec;
using ImageProcessing.TGACodec;
using MoyskleyTech.ImageProcessing.Image;
using ZXing;
using ZXing.Common;

namespace Kit.Services.BarCode
{
    public class BarcodeDecoding
    {
        public BarcodeDecoding()
        {

        }

        public async Task<Result> Decode(FileInfo file, BarcodeFormat format, KeyValuePair<DecodeHintType, object>[] aditionalHints = null)
        {
            MultiFormatReader r = GetReader(format, aditionalHints);

            BinaryBitmap image = await GetImage(file);

            Result result = r.decode(image);

            return result;
        }

        MultiFormatReader GetReader(BarcodeFormat? format, KeyValuePair<DecodeHintType, object>[] aditionalHints)
        {
            var reader = new MultiFormatReader();

            var hints = new Dictionary<DecodeHintType, object>();

            if (format.HasValue)
            {
                hints.Add(DecodeHintType.POSSIBLE_FORMATS, new List<BarcodeFormat>() { format.Value });
            }
            if (aditionalHints != null)
            {
                foreach (var ah in aditionalHints)
                {
                    hints.Add(ah.Key, ah.Value);
                }
            }

            reader.Hints = hints;

            return reader;
        }

        async Task<BinaryBitmap> GetImage(FileInfo fileName)
        {
            BinaryBitmap bBitmap = null;
            using (var stream = fileName.OpenRead())
            {
                Image<Pixel> result;
                using (var memory = new MemoryStream())
                {
                    await stream.CopyToAsync(memory);
                    memory.Position = 0;

                    BitmapFactory factory = new BitmapFactory();
                    factory.AddCodec(new BitmapCodec());
                    factory.AddCodec(new PngCodec());
                    factory.AddCodec(new JPEGCodec());
                    factory.AddCodec(new TGACodec());
                    result = factory.Decode(memory);
                    result = result.GetBitmap(0, 0, result.Width, result.Height);
                }

                //using (var memory = new MemoryStream())
                //{
                // result.ToStream(memory);



                byte[] rgbBytes = GetRgbBytes(result);
                var bin = new HybridBinarizer(new RGBLuminanceSource(rgbBytes, result.Width, result.Height));
                Log.Logger.Debug("Memory:" + rgbBytes.Length);
                Log.Logger.Debug("Size:" + (result.Width * result.Height) * 3);
                bBitmap = new BinaryBitmap(bin);



                //LuminanceSource luminanceSource = new RGBLuminanceSource(memory.ToArray(), result.Width, result.Height);
                //HybridBinarizer hb = new HybridBinarizer(luminanceSource);
                //Binarizer a = hb.createBinarizer(luminanceSource);
                //bBitmap = new BinaryBitmap(a);
                //}
            }
            return bBitmap;
        }
        private byte[] GetRgbBytes(Image<Pixel> image)
        {
            var rgbBytes = new List<byte>();
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    var c = image.Get(x, y);
                    rgbBytes.AddRange(new[] { c.R, c.G, c.B });
                }
            }
            return rgbBytes.ToArray();
        }
    }
}
