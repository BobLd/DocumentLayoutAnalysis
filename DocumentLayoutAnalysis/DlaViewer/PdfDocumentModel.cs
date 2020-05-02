namespace DlaViewer
{
    using OxyPlot;
    using System;
    using System.IO;
    using UglyToad.PdfPig;
    using UglyToad.PdfPig.Core;

    public class PdfDocumentModel
    {
        private PdfDocument pdfDocument;

        public int NumberOfPages => pdfDocument.NumberOfPages;

        public PdfPageModel GetPage(int pageNo)
        {
            return new PdfPageModel(pdfDocument.GetPage(pageNo));
        }

        public static PdfDocumentModel Open(string path)
        {
            if (!File.Exists(path))
            {
                throw new Exception();
            }

            return new PdfDocumentModel() { pdfDocument = PdfDocument.Open(path) };
        }

        public static DataPoint ToDataPoint(PdfPoint pdfPoint)
        {
            return new DataPoint(pdfPoint.X, pdfPoint.Y);
        }
    }
}
