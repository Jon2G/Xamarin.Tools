using System;
using System.Collections.Generic;
using System.Text;

namespace Kit.Services.Interfaces
{
    public interface IPDFConverter
    {
        void ConvertHTMLtoPDF(PDFToHtml _PDFToHtml);
    }
}
