using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.Xamarin.Tools.Shared.Services.Interfaces
{
    public interface IPDFConverter
    {
        void ConvertHTMLtoPDF(PDFToHtml _PDFToHtml);
    }
}
