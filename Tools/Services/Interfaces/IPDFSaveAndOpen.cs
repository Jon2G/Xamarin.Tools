using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Tools.Services.Interfaces
{
    public interface IPDFSaveAndOpen
    {
        Task SaveAndView(string fileName, MemoryStream stream, PDFOpenContext context = PDFOpenContext.InApp, string contentType = "application / pdf");
    }

    /// <summary>
    /// Where should the PDF file open. In the app or out of the app.
    /// </summary>
    public enum PDFOpenContext
    {
        InApp,
        ChooseApp
    }
}
