namespace DocumentLayoutAnalysis
{
    using ImageConverter;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using UglyToad.PdfPig;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.Export;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.ReadingOrderDetector;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;

    /// <summary>
    /// Export pdf document the PAGE XML format.
    /// </summary>
    public class PageExportExample
    {
        static readonly string pdfPath = @"../../Resources/Samples/rotated block2.pdf";
        static readonly int zoom = 5;
        static readonly int pageNo = 1;

        public static void Run()
        {
            var exporter = new PageXmlTextExporter(NearestNeighbourWordExtractor.Instance,
                                                   RecursiveXYCut.Instance,
                                                   UnsupervisedReadingOrderDetector.Instance, 
                                                   scale: zoom);

            using (var converter = new PdfImageConverter(pdfPath))
            using (PdfDocument document = PdfDocument.Open(pdfPath))
            {
                var page = document.GetPage(pageNo);

                var xml = exporter.Get(page);
                File.WriteAllText(Path.ChangeExtension(pdfPath, pageNo + ".xml"), xml);

                using (var bitmap = converter.GetPage(page.Number, zoom))
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    // save pdf page as image
                    bitmap.Save(Path.ChangeExtension(pdfPath, pageNo + "_raw.png"));

                    // save empty image for LayoutEvalGUI
                    Bitmap blackAndWhite = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format8bppIndexed);
                    blackAndWhite.Save(Path.ChangeExtension(pdfPath, pageNo + "_bw_raw.png"));
                }
            }
        }
    }
}
