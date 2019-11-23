using DocumentLayoutAnalysis.Export;
using System.IO;
using UglyToad.PdfPig.DocumentLayoutAnalysis;

namespace DocumentLayoutAnalysis
{
    class HocrTest
    {
        public static void Run(string path)
        {
            hOCR hocr = new hOCR(NearestNeighbourWordExtractor.Instance, RecursiveXYCut.Instance, 2);
            //using (PdfDocument document = PdfDocument.Open(path))
            //{
            //    string str = hocr.GetCode(document);
            //    File.WriteAllText(Path.ChangeExtension(path, "html"), str);
            //}

            string str = hocr.GetCode(path);
            File.WriteAllText(Path.ChangeExtension(path, "html"), str);
        }
    }
}
