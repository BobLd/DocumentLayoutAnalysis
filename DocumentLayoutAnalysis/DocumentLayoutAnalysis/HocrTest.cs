using ImageConverter;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis;

namespace DocumentLayoutAnalysis
{
    class HocrTest
    {
        public static void Run(string path)
        {
            HOCR hocr = new HOCR(NearestNeighbourWordExtractor.Instance, RecursiveXYCut.Instance);
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
