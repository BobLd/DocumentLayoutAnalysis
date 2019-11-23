using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using UglyToad.PdfPig.Geometry;
using UglyToad.PdfPig.Graphics.Colors;

namespace DocumentLayoutAnalysis
{
    class PageXmlTest
    {
        public static void Run()
        {
            string pageFilePath = @"D:\MachineLearning\Document Layout Analysis\hocr\PAGE samples\aletheiaexamplepage_2019.xml"; // Glyph_Sample01_General.xml";

            PageXmlDocument pageXml = PageXmlDocument.Deserialize(pageFilePath);

            var xml = pageXml.Serialize();

            File.WriteAllText(Path.ChangeExtension(pageFilePath, "new.xml"), xml);
        }

        static string PointToString(PdfPoint point)
        {
            return point.X.ToString("0") + "," + point.Y.ToString("0");
        }

        static string ToPoints(IEnumerable<PdfPoint> points)
        {
            return string.Join(" ", points.Select(p => PointToString(p)));
        }

        static string ToPoints(PdfRectangle pdfRectangle)
        {
            return ToPoints(new[] { pdfRectangle.BottomLeft, pdfRectangle.TopLeft, pdfRectangle.TopRight, pdfRectangle.BottomRight });
        }

        static PageXmlCoords ToCoords(PdfRectangle pdfRectangle)
        {
            return new PageXmlCoords()
            {
                //Conf = 1,
                Points = ToPoints(pdfRectangle)
            };
        }

        /// <summary>
        /// PageXml Text colour in RGB encoded format
        /// <para>(red value) + (256 x green value) + (65536 x blue value).</para> 
        /// </summary>
        static string ToRgbEncoded(IColor color)
        {
            var rgb = color.ToRGBValues();
            int red = (int)Math.Round(255f * (float)rgb.r);
            int green = 256 * (int)Math.Round(255f * (float)rgb.g);
            int blue = 65536 * (int)Math.Round(255f * (float)rgb.b);
            int sum = red + green + blue;

            // as per below, red and blue order might be inverted...
            //var colorWin = System.Drawing.Color.FromArgb(sum);

            return sum.ToString();
        }

        static int glyphCount = 700;
        static int wordCount = 100;
        static int lineCount = 100;

        static PageXmlGlyph ToGlyph(Letter letter)
        {
            glyphCount++;
            return new PageXmlGlyph()
            {
                Coords = ToCoords(letter.GlyphRectangle),
                Ligature = false,
                Production = PageXmlProductionSimpleType.Printed,
                TextStyle = new PageXmlTextStyle()
                {
                    FontSize = (float)letter.FontSize,
                    FontFamily = letter.FontName,
                    TextColourRgb = ToRgbEncoded(letter.Color),
                },
                TextEquivs = new PageXmlTextEquiv[] { new PageXmlTextEquiv() { Unicode = letter.Value } },
                Id = "c" + glyphCount
            };
        }

        static PageXmlWord ToWord(Word word)
        {
            wordCount++;
            return new PageXmlWord()
            {
                Coords = ToCoords(word.BoundingBox),
                Glyphs = word.Letters.Select(l => ToGlyph(l)).ToArray(),
                TextEquivs = new PageXmlTextEquiv[] { new PageXmlTextEquiv() { Unicode = word.Text } },
                Id = "w" + wordCount
            };
        }

        static PageXmlTextLine ToTextLine(TextLine textLine)
        {
            lineCount++;
            return new PageXmlTextLine()
            {
                Coords = ToCoords(textLine.BoundingBox),
                //Baseline = new PageXmlBaseline() { },
                Production = PageXmlProductionSimpleType.Printed,
                //ReadingDirection = PageXmlReadingDirectionSimpleType.LeftToRight,
                Words = textLine.Words.Select(w => ToWord(w)).ToArray(),
                TextEquivs = new PageXmlTextEquiv[] { new PageXmlTextEquiv() { Unicode = textLine.Text } },
                Id = "l" + lineCount
            };
        }

        static PageXmlPage FromPdfPage(Page page)
        {
            PageXmlPage pageXmlPage = new PageXmlPage()
            {
                //Border = new PageXmlBorder()
                //{
                //    Coords = new PageXmlCoords()
                //    {
                //        Points = page.
                //    }
                //},
                ImageFilename = "unknown",
                ImageHeight = (int)page.Height,
                ImageWidth = (int)page.Width,
                //PrintSpace = new PageXmlPrintSpace()
                //{
                //    Coords = new PageXmlCoords()
                //    {
                        
                //    }
                //}
            };

            var words = page.GetWords(NearestNeighbourWordExtractor.Instance);

            var pageWordsH = words.Where(x => x.TextDirection == TextDirection.Horizontal || x.TextDirection == TextDirection.Rotate180).ToArray();
            var blocks = RecursiveXYCut.Instance.GetBlocks(pageWordsH);

            int regionCount = 0;

            List<PageXmlRegion> regions = new List<PageXmlRegion>();
            foreach (var currentBlock in blocks)
            {
                regionCount++;
                PageXmlRegion textRegion = new PageXmlTextRegion()
                {
                    Coords = ToCoords(currentBlock.BoundingBox),
                    TextLines = currentBlock.TextLines.Select(l => ToTextLine(l)).ToArray(),
                    TextEquivs = new PageXmlTextEquiv[] { new PageXmlTextEquiv() { Unicode = currentBlock.Text } },
                    Id = "r" + regionCount
                };

                regions.Add(textRegion);
            }
            pageXmlPage.Items = regions.ToArray();

            return pageXmlPage;
        }

        public static void Run(string path)
        {
            PageXmlDocument pageXmlDocument = new PageXmlDocument()
            {
                Metadata = new PageXmlMetadata()
                {
                    Created = DateTime.UtcNow,
                    LastChange = DateTime.UtcNow,
                    Creator = "PdfPig",
                    Comments = "", // algo used in here
                },
                Page = new PageXmlPage()
                {

                },
                PcGtsId = "pc-" + path.GetHashCode()
            };

            using (PdfDocument document = PdfDocument.Open(path))
            {
                //var testAlto = AltoDocument.FromPdfDocument(document);

                for (var i = 0; i < document.NumberOfPages; i++)
                {
                    Page pagePdf = document.GetPage(i + 1);
                    pageXmlDocument.Page = FromPdfPage(pagePdf);

                    //var words = pagePdf.GetWords(NearestNeighbourWordExtractor.Instance);

                    //var pageWordsH = words.Where(x => x.TextDirection == TextDirection.Horizontal || x.TextDirection == TextDirection.Rotate180).ToArray();
                    //var blocks = RecursiveXYCut.Instance.GetBlocks(pageWordsH);



                }
            }


            File.WriteAllText(Path.ChangeExtension(path, "pagexml.xml"), pageXmlDocument.Serialize());
        }
    }
}
