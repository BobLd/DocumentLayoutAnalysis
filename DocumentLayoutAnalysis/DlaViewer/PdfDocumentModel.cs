namespace DlaViewer
{
    using OxyPlot;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using UglyToad.PdfPig;
    using UglyToad.PdfPig.Content;
    using UglyToad.PdfPig.Core;
    using UglyToad.PdfPig.DocumentLayoutAnalysis;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;
    using UglyToad.PdfPig.Util;

    public class PdfDocumentModel
    {
        private PdfDocument pdfDocument;

        public int NumberOfPages => pdfDocument.NumberOfPages;

        public PdfPageModel GetPage(int pageNo)
        {
            return new PdfPageModel(pdfDocument.GetPage(pageNo));
        }

        public static PdfDocumentModel Open(string path)
        {
            if (!File.Exists(path))
            {
                throw new Exception();
            }

            return new PdfDocumentModel() { pdfDocument = PdfDocument.Open(path) };
        }

        public static DataPoint ToDataPoint(PdfPoint pdfPoint)
        {
            return new DataPoint(pdfPoint.X, pdfPoint.Y);
        }
    }


    public class PdfPageModel
    {
        private readonly Page page;
        private readonly IWordExtractor wordExtractor;
        private readonly IPageSegmenter pageSegmenter;

        public double Height => page.Height;
        public double Width => page.Width;

        internal PdfPageModel(Page page)
        {
            this.page = page;
            wordExtractor = NearestNeighbourWordExtractor.Instance;
            pageSegmenter = DocstrumBoundingBoxes.Instance;
        }

        public IEnumerable<Letter> GetLetters()
        {
            return page.Letters;
        }

        public IEnumerable<Word> GetWords()
        {
            return page.GetWords(wordExtractor);
        }

        public IEnumerable<TextBlock> GetTextBlocks()
        {
            return pageSegmenter.GetBlocks(GetWords());
        }
    }
}
