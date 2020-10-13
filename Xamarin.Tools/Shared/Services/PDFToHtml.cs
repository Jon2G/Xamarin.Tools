using Plugin.Xamarin.Tools.Shared.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using Tools;
using Xamarin.Forms;

namespace Plugin.Xamarin.Tools.Shared.Services
{
    public class PDFToHtml : ViewModelBase<PDFToHtml>, IDisposable
    {
        public enum PDFEnum
        {
            Started = 0,
            Failed = 1,
            Completed = 2
        }
        public EventHandler OnCompleted;
        public EventHandler OnFailed;

        private bool ispdfloading;
        private PDFEnum pDFEnum;

        public PDFToHtml(string HTML, string FileName)
        {
            this.FileName = FileName;
            this.HTMLString = HTML;
        }

        public bool IsPDFGenerating
        {
            get { return ispdfloading; }
            set
            {
                ispdfloading = value;
                OnPropertyChanged();
            }
        }

        public PDFEnum Status
        {
            get { return pDFEnum; }
            set
            {
                pDFEnum = value;
                this.UpdatePDFStatus(value);
                OnPropertyChanged();
            }
        }

        public string HTMLString { get; set; }

        public string FileName { get; set; }

        public double PageHeight { get; set; } = 1024;

        public double PageWidth { get; set; } = 512;

        public double PageDPI { get; set; } = 300;

        public string FilePath { get; set; }

        public FileStream FileStream { get; set; }

        public byte[] PDFStreamArray { get; set; }


        public void Dispose()
        {
            if (FileStream != null)
            {
                FileStream.Dispose();
                FileStream = null;
            }

            PDFStreamArray = null;
        }

        public void GeneratePDF()
        {
            try
            {
                this.Status = PDFEnum.Started;
                FilePath = CreateTempPath(FileName);
                FileStream = File.Create(FilePath);
                PDFConverter.Current.ConvertHTMLtoPDF(this);
            }
            catch (Exception ex)
            {
                SQLHelper.Log.LogMe(ex, "ERROR");
            }
        }

        public static string CreateTempPath(string fileName)
        {
            string tempPath = Path.Combine(Plugin.Xamarin.Tools.Shared.Tools.Instance.LibraryPath, "cache");

            if (!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);

            string path = Path.Combine(tempPath, fileName + ".pdf");
            while (File.Exists(path))
            {
                fileName = Path.GetFileNameWithoutExtension(fileName) + "_" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + Path.GetExtension(fileName);
                path = Path.Combine(tempPath, fileName + ".pdf");
            }

            return path;
        }

        private async void UpdatePDFStatus(PDFEnum newValue)
        {
            if (newValue == PDFEnum.Started)
                IsPDFGenerating = true;
            else if (newValue == PDFEnum.Failed)
            {
                OnFailed?.Invoke(this, EventArgs.Empty);
                IsPDFGenerating = false;
                await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("Pdf no generado");
                SQLHelper.Log.LogMe("PDFEnum.Failed");
            }
            else if (newValue == PDFEnum.Completed)
            {
                OnCompleted?.Invoke(this, EventArgs.Empty);
                //try
                //{
                //    PDFStreamArray = Device.RuntimePlatform == Device.iOS ? File.ReadAllBytes(FilePath + ".pdf") : new byte[FileStream.Length];

                //    if (Device.RuntimePlatform == Device.Android)
                //        FileStream.Read(PDFStreamArray, 0, (int)FileStream.Length);

                //    await FileStream.WriteAsync(PDFStreamArray, 0, PDFStreamArray.Length);
                //    FileStream.Close();
                //    IsPDFGenerating = false;
                //    //await App.Current.MainPage.Navigation.PushAsync(new PDFViewer() { Title = FileName, BindingContext = this });
                //}
                //catch (Exception ex)
                //{
                //    SQLHelper.Log.LogMe(ex, "ERROR");
                //    await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("Pdf no generado");
                //}
            }
        }

    }
}
