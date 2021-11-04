using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Xamarin.Forms;

namespace Kit.Forms.Services
{
    public static class FontCache
    {
        public static void DeleteFontCacheIfFontChanged(Type AppType)
        {
            try
            {
                var assembly = AppType.Assembly;
                var exportFontAttribute =
                    assembly.GetCustomAttributes(typeof(ExportFontAttribute), true).FirstOrDefault() as
                        ExportFontAttribute;

                if (exportFontAttribute == null) return;

                string? fontFilePath = null;
                if (Device.RuntimePlatform == Device.Android)
                {
                    fontFilePath = Path.Combine(Xamarin.Essentials.FileSystem.CacheDirectory,
                        exportFontAttribute.FontFileName);
                }
                else if (Device.RuntimePlatform == Device.UWP)
                {
                    fontFilePath = Path.Combine(Xamarin.Essentials.FileSystem.AppDataDirectory, "fonts",
                        exportFontAttribute.FontFileName);
                }
                else if (Device.RuntimePlatform == Device.iOS)
                {
                    fontFilePath = Path.Combine(Xamarin.Essentials.FileSystem.CacheDirectory,
                        exportFontAttribute.FontFileName);
                }

                if (fontFilePath == null || !File.Exists(fontFilePath)) return;

                var deleteFile = false;

                var asmName = assembly.GetName().Name;
                using (var embeddedStream =
                    assembly.GetManifestResourceStream(asmName + ".Resources.Fonts." +
                                                       exportFontAttribute.FontFileName))
                using (var fileStream = File.OpenRead(fontFilePath))
                {
                    var embeddedFontHash = GetHash(embeddedStream);
                    var cachedFontHash = GetHash(fileStream);

                    deleteFile = embeddedFontHash is null || !embeddedFontHash.SequenceEqual(cachedFontHash);
                }

                if (deleteFile)
                {
                    Debug.WriteLine($"deleting '{fontFilePath}'");
                    File.Delete(fontFilePath);
                }

                static byte[] GetHash(Stream stream)
                {
                    if (stream is null)
                    {
                        return null;
                    }
                    using (SHA256 hashAlgorithm = SHA256.Create())
                    {
                        byte[] data = hashAlgorithm.ComputeHash(stream);
                        return data;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "FontCache");
            }
        }
    }
}