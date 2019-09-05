using ImageConverter;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using UglyToad.PdfPig.Geometry;
using UglyToad.PdfPig.Util;
using static UglyToad.PdfPig.Geometry.PdfPath;

namespace DocumentLayoutAnalysis
{
    class HOCR
    {
        //http://kba.cloud/hocr-spec/1.2
        //https://github.com/kba/hocrjs

        static decimal _scale;
        IPageSegmenter _pageSegmenter;
        IWordExtractor _wordExtractor;
        string _documentPath = "not_found.pdf";
        string _indent;

        int pageCount = 0;
        int areaCount = 0;
        int lineCount = 0;
        int wordCount = 0;

        public HOCR(IWordExtractor wordExtractor, IPageSegmenter pageSegmenter, double scale = 1.0, string indent = " ")
        {
            _wordExtractor = wordExtractor;
            _pageSegmenter = pageSegmenter;
            _scale = (decimal)scale;
            _indent = indent;
        }

        private string GetPageImagePath(string documentPath, int pageNumber)
        {
            string imageName = Path.ChangeExtension(Path.GetFileName(documentPath).Replace(" ", "_"), pageNumber + ".png");
            string path = Path.GetDirectoryName(documentPath);
            return Path.Combine(path, imageName);
        }

        public string GetCode(string documentPath)
        {
            _documentPath = documentPath;

            var pageNumber = 0;
            string hocr = "";
            using (var document = PdfDocument.Open(documentPath))
            {
                pageNumber = document.NumberOfPages;
                hocr = GetCode(document);
            }

            using (var converter = new PdfImageConverter(documentPath))
            {
                for (var i = 0; i < pageNumber; i++)
                {
                    using (var bitmap = converter.GetPage(i + 1, (float)_scale))
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        bitmap.Save(GetPageImagePath(documentPath, i + 1));
                    }
                }
            }
            return hocr;
        }

        public string GetCode(PdfDocument document)
        {
            string xmlHeader = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
            xmlHeader += "\n<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\n";

            string html = "<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\" lang=\"en\">\n";
            string head =
                _indent + "<head>" +
                "\n" + _indent + _indent + "<title></title>" +
                "\n" + _indent + _indent + "<meta http-equiv='Content-Type' content='text/html;charset=utf-8' />" +
                "\n" + _indent + _indent + "<meta name='ocr-system' content='" + _pageSegmenter.GetType().Name + "' />" +
                "\n" + _indent + _indent + "<meta name='ocr-capabilities' content='ocr_page ocr_carea ocr_par ocr_line ocrx_word' />" +
                "\n" + _indent + "</head>\n";

            string hocr = head + _indent + "<body>\n";

            for (var i = 0; i < document.NumberOfPages; i++)
            {
                var page = document.GetPage(i + 1);
                hocr += GetCode(page, GetPageImagePath(_documentPath, i + 1)) + "\n";
            }

            hocr = hocr + _indent + "<script src='https://unpkg.com/hocrjs'></script>\n" + _indent + "</body>";
            hocr = xmlHeader + html + hocr + "\n</html>";
            return hocr;
        }

        private string GetCode(Page page, string imageName)
        {
            pageCount++;
            imageName = Path.GetFileName(imageName);
            string hocr = _indent + @"<div class='ocr_page' id='page_" + page.Number.ToString() +
                "' title='image \"" + imageName + "\"; bbox 0 0 " +
                (int)Math.Round(page.Width * _scale) + " " + (int)Math.Round(page.Height * _scale) +
                "; ppageno " + (page.Number - 1) + "\'>";

            foreach (var path in page.ExperimentalAccess.Paths)
            {
                hocr += "\n" + GetCode(path, page.Height);
            }

            var words = page.GetWords(_wordExtractor);

            if (words.Count() > 0)
            {
                var blocks = _pageSegmenter.GetBlocks(words);
                foreach (var block in blocks)
                {
                    hocr += "\n" + GetCode(block, page.Height);
                }
            }

            hocr += "\n" + _indent + @"</div>";
            return hocr;
        }

        private string GetCode(PdfPath path, decimal pageHeight)
        {
            if (path == null) return string.Empty;
            var bbox = GetBoundingRectangle(path.Commands);
            if (bbox != null)
            {
                return _indent + _indent + @"<span class='ocr_linedrawing' id='drawing_" + pageCount + "_0' title='" + GetCode((PdfRectangle)bbox, pageHeight) + "'/ >";
            }
            return string.Empty;
        }

        internal static PdfRectangle? GetBoundingRectangle(IReadOnlyList<IPathCommand> commands)
        {
            if (commands.Count == 0)
            {
                return null;
            }

            var minX = decimal.MaxValue;
            var maxX = decimal.MinValue;

            var minY = decimal.MaxValue;
            var maxY = decimal.MinValue;

            foreach (var command in commands)
            {
                var rect = command.GetBoundingRectangle();
                if (rect == null)
                {
                    continue;
                }

                if (rect.Value.Left < minX)
                {
                    minX = rect.Value.Left;
                }

                if (rect.Value.Right > maxX)
                {
                    maxX = rect.Value.Right;
                }

                if (rect.Value.Bottom < minY)
                {
                    minY = rect.Value.Bottom;
                }

                if (rect.Value.Top > maxY)
                {
                    maxY = rect.Value.Top;
                }
            }

            return new PdfRectangle(minX, minY, maxX, maxY);
        }

        private string GetCode(TextBlock block, decimal pageHeight)
        {
            areaCount++;
            string hocr = _indent + _indent + @"<div class='ocr_carea' id='block_" + pageCount + "_" + areaCount + "' title='" + GetCode(block.BoundingBox, pageHeight) + "'>";
            foreach (var line in block.TextLines)
            {
                hocr += "\n" + GetCode(line, pageHeight);
            }
            hocr += "\n" + _indent + _indent + @"</div>";
            return hocr;
        }

        private string GetCode(TextLine line, decimal pageHeight)
        {
            /*foreach (var word in line.Words)
            {
                foreach (var letter in word.Letters)
                {
                    Console.WriteLine(letter.StartBaseLine.Y);
                }
            }

            Console.WriteLine();*/


            lineCount++;
            double angle = 0;

            // http://kba.cloud/hocr-spec/1.2/#propdef-baseline
            // below will be 0 as long as the word's bounding box bottom is the BaseLine and not 'Bottom'
            double baseLine = (double)line.Words[0].Letters[0].StartBaseLine.Y;
            baseLine = (double)line.BoundingBox.Bottom - baseLine;

            string hocr = _indent + _indent + _indent + @"<span class='ocr_line' id='line_" + pageCount + "_" + lineCount + "' title='" + GetCode(line.BoundingBox, pageHeight)
                + "; baseline " + angle + " 0'>"; //"; baseline 0.005 - 10; x_size 42.392159; x_descenders 5.3921571; x_ascenders 12' >";

            foreach (var word in line.Words)
            {
                hocr += "\n" + GetCode(word, pageHeight);
            }
            hocr += "\n" + _indent + _indent + _indent + @"</span>";
            return hocr;
        }

        private string GetCode(Word word, decimal pageHeight)
        {
            wordCount++;
            string hocr = _indent + _indent + _indent + _indent + @"<span class='ocrx_word' id='word_" + pageCount + "_" + wordCount + "' title='" + GetCode(word.BoundingBox, pageHeight) + "; x_wconf 100'>" + word.Text + "</span> ";
            return hocr;
        }

        private string GetCode(PdfRectangle rectangle, decimal pageHeight)
        {
            return @"bbox " + (int)Math.Round(rectangle.Left * _scale) + " "
                            + (int)Math.Round((pageHeight - rectangle.Top) * _scale) + " "
                            + (int)Math.Round(rectangle.Right * _scale) + " "
                            + (int)Math.Round((pageHeight - rectangle.Bottom) * _scale);
        }

        // x_bboxes // http://kba.cloud/hocr-spec/1.2/#propdef-x_bboxes
    }
}
