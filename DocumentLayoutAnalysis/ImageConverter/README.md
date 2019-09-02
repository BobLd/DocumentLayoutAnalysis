# Pdf file to image converter
Tool to convert a pdf page to an image. It relies on the [_mupdf_](https://github.com/sumatrapdfreader/sumatrapdf) library, available in the [sumatra pdf reader](https://github.com/sumatrapdfreader/sumatrapdf/tree/master/mupdf/include/mupdf). See the __Sources__ for original code.
Can be used with PdfPig.

## Sources 
 * https://www.codeproject.com/articles/498317/rendering-pdf-documents-with-mupdf-and-p-invoke-in
 * https://github.com/wmjordan/mupdf/blob/master/MupdfSharp/Program.cs
 * https://github.com/reliak/moonpdf/blob/master/src/MoonPdfLib/MuPdf/MuPdfWrapper.cs
 * lib: https://github.com/sumatrapdfreader/sumatrapdf/tree/master/mupdf/include/mupdf/fitz
 
 ## Usage example (using PdfPig)
 The below example prints all pages of _test.pdf_ file the with the letter bounding boxes.
 
            float zoom = 10;
            var greenPen = new Pen(Color.GreenYellow, zoom * 0.4f);
            var path = "test.pdf"
            
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
