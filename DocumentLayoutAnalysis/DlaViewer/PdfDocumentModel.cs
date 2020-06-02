namespace DlaViewer
{
    using OxyPlot;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using UglyToad.PdfPig;
    using UglyToad.PdfPig.Core;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
    using UglyToad.PdfPig.Util;

    public class PdfDocumentModel
    {
        private PdfDocument pdfDocument;

        public int NumberOfPages => pdfDocument.NumberOfPages;

        public PdfPageModel GetPage(int pageNo)
        {
            try
            {
                return new PdfPageModel(pdfDocument.GetPage(pageNo));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return null;
            }
        }

        public string PdfPigVersion { get; set; }

        public static PdfDocumentModel Open(string path, bool clipPaths)
        {
            if (!File.Exists(path))
            {
                throw new Exception();
            }

            var version = System.Reflection.Assembly.GetAssembly(typeof(PdfDocument)).GetName().Version.ToString();
            var fullName = System.Reflection.Assembly.GetAssembly(typeof(PdfDocument)).GetName().Name;

            return new PdfDocumentModel()
            {
                pdfDocument = PdfDocument.Open(path,
                                               new ParsingOptions()
                                               {
                                                   ClipPaths = clipPaths
                                               }),
                PdfPigVersion = $"{fullName} {version}".Trim()
            };
        }

        public static DataPoint ToDataPoint(PdfPoint pdfPoint)
        {
            return new DataPoint(pdfPoint.X, pdfPoint.Y);
        }

        public static IEnumerable<Type> GetWordExtractors()
        {
            // get all IPageSegmenter
            return GetAllFromType(typeof(IWordExtractor));
        }

        public static IEnumerable<Type> GetPageSegmenters()
        {
            // get all IPageSegmenter
            return GetAllFromType(typeof(IPageSegmenter));
        }

        private static IEnumerable<Type> GetAllFromType(Type type)
        {
            var seg = DefaultPageSegmenter.Instance; // make sure DLA lib is loaded

            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.Contains("UglyToad"))
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p))
                .Where(c => !c.IsAbstract);
        }
    }
}
