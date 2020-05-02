namespace DlaViewer
{
    using System.Collections.Generic;
    using System.Linq;
    using UglyToad.PdfPig.Content;
    using UglyToad.PdfPig.DocumentLayoutAnalysis;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;
    using UglyToad.PdfPig.Util;

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

        public PageInfoModel GetPageInfo()
        {
            return new PageInfoModel(this);
        }

        public class PageInfoModel
        {
            private PdfPageModel pdfPageModel;

            public Distribution HeightDistribution { get; }
            public Distribution WidthDistribution { get; }

            public PageInfoModel(PdfPageModel pdfPageModel)
            {
                this.pdfPageModel = pdfPageModel;
                var filteredLetters = pdfPageModel.GetLetters().Where(l => !string.IsNullOrEmpty(l.Value.Trim())).ToList();

                HeightDistribution = new Distribution(filteredLetters.Select(l => l.GlyphRectangle.Height));
                WidthDistribution = new Distribution(filteredLetters.Select(l => l.GlyphRectangle.Width));
            }
        }
    }
}
