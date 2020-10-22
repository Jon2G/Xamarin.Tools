using System;
using System.Collections.Generic;
using System.Text;
using Tools.Shared.Services;

namespace Tools.Services.Interfaces
{
    public interface IPDFConverter
    {
        void ConvertHTMLtoPDF(PDFToHtml _PDFToHtml);
    }
}
