using ImageConverter;
using System.Drawing;
using System.IO;
using UglyToad.PdfPig;

namespace DocumentLayoutAnalysis
{
    class ImageTest
    {
        public static void Run(string path)
        {
            float zoom = 10;
            var greenPen = new Pen(Color.GreenYellow, zoom * 0.4f);

            using (var converter = new PdfImageConverter(path))
            using (var document = PdfDocument.Open(path))
            {
                for (var i = 0; i < document.NumberOfPages; i++)
                {
                    var page = document.GetPage(i + 1);

                    using (var bitmap = converter.GetPage(i + 1, zoom))
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        var imageHeight = bitmap.Height;

                        foreach (var letter in page.Letters)
                        {
                            var rect = new Rectangle(
                                (int)(letter.GlyphRectangle.Left * (decimal)zoom),
                                imageHeight - (int)(letter.GlyphRectangle.Top * (decimal)zoom),
                                (int)(letter.GlyphRectangle.Width * (decimal)zoom),
                                (int)(letter.GlyphRectangle.Height * (decimal)zoom));
                            graphics.DrawRectangle(greenPen, rect);
                        }

                        bitmap.Save(Path.ChangeExtension(path, (i + 1) + "_imageTest.png"));
                    }
                }
            }
        }
    }
}
