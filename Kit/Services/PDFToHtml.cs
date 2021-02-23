
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using Kit.Enums;
using Kit.Services;
using Kit.Services.Interfaces;


namespace Kit.Services
{
    public class PDFToHtml : ModelBase, IDisposable
    {

        public EventHandler OnCompleted;
        public EventHandler OnFailed;

        private bool ispdfloading;
        private PDFEnum pDFEnum;
        private readonly IPDFConverter PDFConverter;
        public PDFToHtml(string HTML, string FileName, IPDFConverter PDFConverter)
        {
            this.PDFConverter = PDFConverter;
            this.FileName = FileName;
            HTMLString = HTML;
        }

        public bool IsPDFGenerating
        {
            get => ispdfloading;
            set
            {
                ispdfloading = value;
                Raise(() => IsPDFGenerating);
            }
        }

        public PDFEnum Status
        {
            get => pDFEnum;
            set
            {
                pDFEnum = value;
                UpdatePDFStatus(value);
                Raise(() => pDFEnum);
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
                Status = PDFEnum.Started;
                FilePath = CreateTempPath(FileName);
                FileStream = File.Create(FilePath);
                this.PDFConverter.ConvertHTMLtoPDF(this);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "ERROR");
            }

        }


        public static string CreateTempPath(string fileName)
        {

            string tempPath = Path.Combine(Tools.Instance.LibraryPath, "cache");

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

        private void UpdatePDFStatus(PDFEnum newValue)
        {
            if (newValue == PDFEnum.Started)
                IsPDFGenerating = true;
            else if (newValue == PDFEnum.Failed)
            {
                OnFailed?.Invoke(this, EventArgs.Empty);
                IsPDFGenerating = false;
                // await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("Pdf no generado");
                Log.Logger.Error("PDFEnum.Failed");
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
                //    SQLHelper.Log.Logger.Error(ex, "ERROR");
                //    await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("Pdf no generado");
                //}
            }
        }

    }
}
