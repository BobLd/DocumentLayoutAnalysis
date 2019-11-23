using ImageConverter;
using System.Drawing;
using System.IO;
using System.Linq;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis;

namespace DocumentLayoutAnalysis
{
    class PdfATest
    {
        public static void Run(string path)
        {
            float zoom = 3;
            var pinkPen = new Pen(Color.HotPink, zoom * 0.4f);
            var greenPen = new Pen(Color.GreenYellow, zoom * 0.6f);
            var bluePen = new Pen(Color.Fuchsia, zoom * 2.0f);

            using (var converter = new PdfImageConverter(path))
            using (PdfDocument document = PdfDocument.Open(path))
            {
                for (var i = 0; i < document.NumberOfPages; i++)
                {
                    var page = document.GetPage(i + 1);

                    using (var bitmap = converter.GetPage(i + 1, zoom))
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        var imageHeight = bitmap.Height;

                        var words = page.GetWords(NearestNeighbourWordExtractor.Instance);
                        var pageWordsH = words.Where(x => x.TextDirection == TextDirection.Horizontal || x.TextDirection == TextDirection.Rotate180).ToArray();
                        var blocks = new RecursiveXYCutLocal(path, i + 1).GetBlocks(pageWordsH);

                        foreach (var block in blocks)
                        {
                            var rect = new Rectangle(
                                (int)(block.BoundingBox.Left * (decimal)zoom),
                                imageHeight - (int)(block.BoundingBox.Top * (decimal)zoom),
                                (int)(block.BoundingBox.Width * (decimal)zoom),
                                (int)(block.BoundingBox.Height * (decimal)zoom));

                            graphics.DrawRectangle(bluePen, rect);
                        }

                        bitmap.Save(Path.ChangeExtension(path, (i + 1) + "_final.png"));
                    }
                }
            }
        }
    }
}
