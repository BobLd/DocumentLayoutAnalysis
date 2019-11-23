using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis;

/// Coordinates as being used in HPOS and VPOS are absolute coordinates referring to the upper-left corner of a page.
/// The upper left corner of the page is defined as coordinate (0/0). 
namespace DocumentLayoutAnalysis
{
    class AltoTest
    {
        public static void Run()
        {
            string altoFilePath = @"D:\MachineLearning\Document Layout Analysis\hocr\Glyph_Sample01_General.xml"; // Glyph_Sample01_General.xml";
            AltoDocument alto = AltoDocument.Deserialize(altoFilePath);


            var xml = alto.Serialize();

            File.WriteAllText(Path.ChangeExtension(altoFilePath, "new.xml"), xml);
        }

        public static AltoGlyph ToAltoGlyph(Letter letter, decimal height)
        {
            return new AltoGlyph()
            {
                VPos = (float)(height - letter.GlyphRectangle.Top),
                HPos = (float)letter.GlyphRectangle.Left,
                Height = (float)letter.GlyphRectangle.Height,
                Width = (float)letter.GlyphRectangle.Width,
                Gc = 1.0f,
                Content = letter.Value,
                Id = "NA"
            };
        }

        public static AltoString ToAltoString(UglyToad.PdfPig.Content.Word word, decimal height)
        {
            var glyphs = word.Letters.Select(l => ToAltoGlyph(l, height)).ToArray();
            AltoString stringType = new AltoString()
            {
                VPos = (float)(height - word.BoundingBox.Top),
                HPos = (float)word.BoundingBox.Left,
                Height = (float)word.BoundingBox.Height,
                Width = (float)word.BoundingBox.Width,
                Glyph = glyphs,
                Cc = string.Join("", glyphs.Select(g => 9f * (1f - g.Gc))), // from 0->1 to 9->0
                Content = word.Text,
                Cs = false,
                Lang = null,
                //Style = AltoFontStyles.Bold,
                StyleRefs = null,
                SubsContent = null,
                //SubsType = AltoSubsType.Abbreviation,
                TagRefs = null,
                Wc = float.NaN,
                Id = "NA"
            };
            return stringType;
        }

        public static AltoTextBlockTextLine ToAltoTextLine(TextLine textLine, decimal height)
        {
            var strings = textLine.Words.Select(w => ToAltoString(w, height)).ToArray();
            AltoTextBlockTextLine line = new AltoTextBlockTextLine()
            {
                VPos = (float)(height - textLine.BoundingBox.Top),
                HPos = (float)textLine.BoundingBox.Left,
                Height = (float)textLine.BoundingBox.Height,
                Width = (float)textLine.BoundingBox.Width,
                BaseLine = float.NaN, // TBD
                Hyp = new AltoTextBlockTextLineHyp() { }, // TBD
                Strings = strings,
                Lang = null,
                Sp = new AltoSP[0], // TBD
                StyleRefs = null,
                TagRefs = null,
                Id = "NA"
            };
            return line;
        }

        public static AltoTextBlock ToAltoTextBlock(TextBlock textBlock, decimal height)
        {
            AltoTextBlock block = new AltoTextBlock()
            {
                VPos = (float)(height - textBlock.BoundingBox.Top),
                HPos = (float)textBlock.BoundingBox.Left,
                Height = (float)textBlock.BoundingBox.Height,
                Width = (float)textBlock.BoundingBox.Width,
                Rotation = 0,  // check textBlock.TextDirection
                TextLines = textBlock.TextLines.Select(l => ToAltoTextLine(l, height)).ToArray(),
                Cs = false,
                StyleRefs = null,
                TagRefs = null,
                title = null,
                type = null,
                IdNext = "NA", // for reading order
                Id = "NA"
            };
            return block;
        }

        public static AltoDescription GetAltoDescription(string fileName)
        {
            AltoDescriptionProcessing processing = new AltoDescriptionProcessing()
            {
                ProcessingAgency = null,
                ProcessingCategory = AltoProcessingCategory.Other, // TBD
                ProcessingDateTime = DateTime.UtcNow.ToString(),
                ProcessingSoftware = new AltoProcessingSoftware()
                {
                    SoftwareName = "PdfPig",
                    SoftwareCreator = @"https://github.com/UglyToad/PdfPig",
                    ApplicationDescription = "Read and extract text and other content from PDFs in C# (port of PdfBox)",
                    SoftwareVersion = "x.x.xx"
                },
                ProcessingStepDescription = null,
                ProcessingStepSettings = null, // algo names here
                Id = "NA"
            };

            AltoDocumentIdentifier documentIdentifier = new AltoDocumentIdentifier()
            {
                DocumentIdentifierLocation = null,
                Value = null
            };

            AltoFileIdentifier fileIdentifier = new AltoFileIdentifier()
            {
                FileIdentifierLocation = null,
                Value = null
            };

            return new AltoDescription()
            {
                MeasurementUnit = AltoMeasurementUnit.Inch1200, // need to check that
                Processings = new[] { processing },
                SourceImageInformation = new AltoSourceImageInformation()
                {
                    DocumentIdentifiers = new AltoDocumentIdentifier[] { documentIdentifier },
                    FileIdentifiers = new AltoFileIdentifier[] { fileIdentifier },
                    FileName = fileName
                }
            };
        }

        public static void Run(string path)
        {
            float zoom = 10;
            var pinkPen = new Pen(Color.HotPink, zoom * 0.4f);
            var greenPen = new Pen(Color.GreenYellow, zoom * 0.6f);
            var bluePen = new Pen(Color.Blue, zoom * 1.0f);

            AltoDocument alto = new AltoDocument()
            {
                Layout = new AltoLayout()
                {
                    StyleRefs = null
                },
                Description = GetAltoDescription("filename"),
                SchemaVersion = "SCHEMAVERSION1",
                Styles = new AltoStyles()
                {

                },
                Tags = new AltoTags()
                {

                }
            };

            List<AltoPage> altoPages = new List<AltoPage>();

            using (PdfDocument document = PdfDocument.Open(path))
            {
                var testAlto = AltoDocument.FromPdfDocument(document);

                for (var i = 0; i < document.NumberOfPages; i++)
                {
                    var pagePdf = document.GetPage(i + 1);

                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    var words = pagePdf.GetWords(NearestNeighbourWordExtractor.Instance);
                    stopwatch.Stop();
                    Console.WriteLine("GetWords() - Time elapsed: {0}", stopwatch.Elapsed);

                    var pageWordsH = words.Where(x => x.TextDirection == TextDirection.Horizontal || x.TextDirection == TextDirection.Rotate180).ToArray();

                    stopwatch.Reset();
                    stopwatch.Start();
                    var blocks = RecursiveXYCut.Instance.GetBlocks(pageWordsH);
                    stopwatch.Stop();
                    Console.WriteLine("RecursiveXYCut() - Time elapsed: {0}", stopwatch.Elapsed);

                    altoPages.Add(new AltoPage()
                    {
                        Height = (float)pagePdf.Height,
                        Width = (float)pagePdf.Width,
                        Accuracy = float.NaN,
                        Quality = AltoQuality.OK,
                        QualityDetail = null,
                        BottomMargin = null,
                        LeftMargin = null,
                        RightMargin = null,
                        TopMargin = null,
                        Pc = float.NaN,
                        PhysicalImgNr = pagePdf.Number,
                        PrintedImgNr = null,
                        PageClass = null,
                        Position = AltoPosition.Cover,
                        Processing = null,
                        ProcessingRefs = null,
                        StyleRefs = null,
                        PrintSpace = new AltoPageSpace()
                        {
                            Height = (float)pagePdf.Height,                     // TBD
                            Width = (float)pagePdf.Width,                       // TBD
                            VPos = 0f,                                          // TBD
                            HPos = 0f,                                          // TBD
                            ComposedBlocks = null,                              // TBD
                            GraphicalElements = null,                           // TBD
                            Illustrations = null,                               // TBD
                            ProcessingRefs = null,                              // TBD
                            StyleRefs = null,                                   // TBD
                            TextBlock = blocks.Select(b => ToAltoTextBlock(b, pagePdf.Height)).ToArray(),
                            Id = "NA"
                        },
                        Id = "NA"
                    });
                }
            }

            alto.Layout.Pages = altoPages.ToArray();


            XmlSerializer xsSubmit = new XmlSerializer(typeof(AltoDocument));
            var xml = "";

            using (var sww = new StringWriter())
            {
                using (XmlTextWriter writer = new XmlTextWriter(sww) { Formatting = Formatting.Indented })
                {
                    xsSubmit.Serialize(writer, alto);
                    xml = sww.ToString(); // Your XML
                }
            }

            File.WriteAllText(Path.ChangeExtension(path, "xml"), xml);
        }
    }
}
