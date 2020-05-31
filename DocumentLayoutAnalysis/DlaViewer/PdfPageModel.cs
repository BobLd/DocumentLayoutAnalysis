namespace DlaViewer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UglyToad.PdfPig.Content;
    using UglyToad.PdfPig.DocumentLayoutAnalysis;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
    using UglyToad.PdfPig.Graphics;
    using UglyToad.PdfPig.Util;

    public class PdfPageModel
    {
        private readonly Page page;
        private IWordExtractor wordExtractor;
        private IPageSegmenter pageSegmenter;

        public double Height => page.Height;
        public double Width => page.Width;

        internal PdfPageModel(Page page)
        {
            this.page = page;
            wordExtractor = DefaultWordExtractor.Instance;
            pageSegmenter = DocstrumBoundingBoxes.Instance;
        }

        public void SetWordExtractor(Type wordExtractor)
        {
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
            this.pageSegmenter = (IPageSegmenter)Activator.CreateInstance(pageSegmenter);
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

        public IEnumerable<PdfPath> GetPdfPaths()
        {
            return page.ExperimentalAccess.Paths;
        }

        public IEnumerable<IPdfImage> GetImages()
        {
            return page.GetImages();
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
