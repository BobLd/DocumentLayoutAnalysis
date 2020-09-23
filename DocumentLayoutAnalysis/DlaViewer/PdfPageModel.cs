namespace DlaViewer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Tabula;
    using Tabula.Extractors;
    using UglyToad.PdfPig.Content;
    using UglyToad.PdfPig.Core;
    using UglyToad.PdfPig.DocumentLayoutAnalysis;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
    using UglyToad.PdfPig.Graphics;
    using UglyToad.PdfPig.Util;

    public class PdfPageModel
    {
        private readonly Page page;
        private readonly PageArea tabulaPage;
        private IWordExtractor wordExtractor;
        private IPageSegmenter pageSegmenter;
        private bool removeDuplicateLetters;

        public double Height => page.Height;
        public double Width => page.Width;
        public CropBox CropBox => page.CropBox;

        internal PdfPageModel(Page page, PageArea tabulaPage)
        {
            this.page = page;
            this.tabulaPage = tabulaPage;
        }

        public void SetRemoveDuplicateLetters(bool remove)
        {
            removeDuplicateLetters = remove;
        }

        public void SetWordExtractor(Type wordExtractor)
        {
            if (wordExtractor == null) return;

            try
            {
                this.wordExtractor = (IWordExtractor)Activator.CreateInstance(wordExtractor);
            }
            catch (Exception)
            {
                this.wordExtractor = (IWordExtractor)wordExtractor.GetMethod("get_Instance").Invoke(null, null);
            }
        }

        public void SetPageSegmenter(Type pageSegmenter)
        {
            if (pageSegmenter == null) return;
            this.pageSegmenter = (IPageSegmenter)Activator.CreateInstance(pageSegmenter);
        }

        public IReadOnlyList<Letter> GetLetters()
        {
            if (removeDuplicateLetters)
            {
                return DuplicateOverlappingTextProcessor.Get(page.Letters);
            }

            return page.Letters;
        }

        public IEnumerable<Word> GetWords()
        {
            if (wordExtractor == null)
            {
                return new List<Word>();
            }

            return wordExtractor.GetWords(GetLetters());
        }

        public IEnumerable<TextBlock> GetTextBlocks()
        {
            if (pageSegmenter == null)
            {
                return new List<TextBlock>();
            }

            return pageSegmenter.GetBlocks(GetWords());
        }

        public IEnumerable<PdfPath> GetPdfPaths()
        {
            return page.ExperimentalAccess.Paths;
        }

        public IEnumerable<IPdfImage> GetImages()
        {
            return page.GetImages();
        }

        public IReadOnlyList<Table> GetTables()
        {
            IExtractionAlgorithm ea = new SpreadsheetExtractionAlgorithm();
            return ea.Extract(tabulaPage);
        }

        public IEnumerable<PdfRectangle> GetWhitespaceCover()
        {
            return WhitespaceCoverExtractor.GetWhitespaces(GetWords(), GetImages());
        }

        public PageInfoModel GetPageInfo()
        {
            return new PageInfoModel(this);
        }

        public class PageInfoModel
        {
            private readonly PdfPageModel pdfPageModel;

            public Distribution HeightDistribution { get; }

            public Distribution WidthDistribution { get; }

            public PageInfoModel(PdfPageModel pdfPageModel)
            {
                this.pdfPageModel = pdfPageModel;
                var filteredLetters = pdfPageModel.GetLetters().Where(l => !string.IsNullOrEmpty(l.Value.Trim())).ToList();

                if (filteredLetters.Count > 0)
                {
                    HeightDistribution = new Distribution(filteredLetters.Select(l => l.GlyphRectangle.Height));
                    WidthDistribution = new Distribution(filteredLetters.Select(l => l.GlyphRectangle.Width));
                }
            }
        }
    }
}
