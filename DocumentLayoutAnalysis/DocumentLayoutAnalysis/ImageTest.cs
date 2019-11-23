using ImageConverter;
using System;
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
            var redPen = new Pen(Color.Red, zoom * 0.4f);

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
                            Console.WriteLine(letter.TextDirection);
                            var height = letter.GlyphRectangle.Height;

                            var rect = new Rectangle(
                                (int)(letter.GlyphRectangle.Left * (decimal)zoom),
                                imageHeight - (int)(letter.GlyphRectangle.Top * (decimal)zoom),
                                (int)((letter.GlyphRectangle.Width == 0 ? 1 : letter.GlyphRectangle.Width) * (decimal)zoom),
                                (int)((letter.GlyphRectangle.Height == 0 ? 1 : letter.GlyphRectangle.Height) * (decimal)zoom));
                            graphics.DrawRectangle(redPen, rect);
                        }

                        bitmap.Save(Path.ChangeExtension(path, (i + 1) + "_imageTest.png"));
                    }
                }
            }
        }
    }
}
