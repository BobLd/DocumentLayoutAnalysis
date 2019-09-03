using ImageConverter;
using System.Drawing;
using System.IO;
using System.Linq;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis;

namespace DocumentLayoutAnalysis
{
    class RXYCutTest
    {
        public static void Run(string path)
        {
            float zoom = 10;
            var pinkPen = new Pen(Color.HotPink, zoom * 0.4f);
            var greenPen = new Pen(Color.GreenYellow, zoom * 0.6f);
            var bluePen = new Pen(Color.Blue, zoom * 1.0f);

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
                        var blocks = RecursiveXYCut.Instance.GetBlocks(pageWordsH);

                        foreach (var block in blocks)
                        {
                            var rect = new Rectangle(
                                (int)(block.BoundingBox.Left * (decimal)zoom),
                                imageHeight - (int)(block.BoundingBox.Top * (decimal)zoom),
                                (int)(block.BoundingBox.Width * (decimal)zoom),
                                (int)(block.BoundingBox.Height * (decimal)zoom));

                            graphics.DrawRectangle(bluePen, rect);

                            foreach (var line in block.TextLines)
                            {
                                var rectL = new Rectangle(
                                    (int)(line.BoundingBox.Left * (decimal)zoom),
                                    imageHeight - (int)(line.BoundingBox.Top * (decimal)zoom),
                                    (int)(line.BoundingBox.Width * (decimal)zoom),
                                    (int)(line.BoundingBox.Height * (decimal)zoom));

                                graphics.DrawRectangle(greenPen, rectL);
                            }
                        }

                        foreach (var word in words)
                        {
                            var rect = new Rectangle(
                                (int)(word.BoundingBox.Left * (decimal)zoom),
                                imageHeight - (int)(word.BoundingBox.Top * (decimal)zoom),
                                (int)(word.BoundingBox.Width * (decimal)zoom),
                                (int)(word.BoundingBox.Height * (decimal)zoom));

                            graphics.DrawRectangle(pinkPen, rect);
                        }

                        bitmap.Save(Path.ChangeExtension(path, (i + 1) + "_RXYCutTest.png"));
                    }
                }
            }
        }
    }
}
