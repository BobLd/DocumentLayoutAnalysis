namespace DlaViewer
{
    using ImageConverter;
    using OxyPlot;
    using OxyPlot.Annotations;
    using OxyPlot.Axes;
    using OxyPlot.Series;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using UglyToad.PdfPig.Core;
    using static UglyToad.PdfPig.Core.PdfSubpath;

    public class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        public PlotController CustomController { get; }

        public string PdfPigVersion { get; set; }

        private PdfImageConverter _pdfImageConverter;
        private PdfDocumentModel _pdfDocumentModel;

        private PlotModel _pagePlotModel;
        private IList<Series> _currentSeries;
        private PlotModel _heightHistoPlotModel;
        private PlotModel _widthtHistoPlotModel;

        private int _numberOfPages;
        private int _currentPageNumber;

        public ObservableCollection<Type> WordExtractorList { get; }
        public ObservableCollection<Type>  PageSegmenterList { get; }

        private PdfPageModel _pdfPageModel;

        private Type _wordExtractor;
        public Type WordExtractor
        {
            get
            {
                return _wordExtractor;
            }

            set
            {
                _wordExtractor = value;
                SetWordExtractor(value);
                this.RaisePropertyChanged(nameof(WordExtractor));
            }
        }

        public void SetWordExtractor(Type wordExtractor)
        {
            if (_pdfPageModel != null && wordExtractor != null)
            {
                _pdfPageModel.SetWordExtractor(wordExtractor);

                if (IsDisplayWords)
                {
                    DisplayWords();
                }

                if (IsDisplayTextLines)
                {
                    DisplayTextLines();
                }

                if (IsDisplayTextBlocks)
                {
                    DisplayTextBlocks();
                }

                if (IsDisplayWsCover)
                {
                    DisplayWsCover();
                }
            }
        }

        private Type _pageSegmenter;
        public Type PageSegmenter
        {
            get
            {
                return _pageSegmenter;
            }

            set
            {
                _pageSegmenter = value;
                SetPageSegmenter(value);
                this.RaisePropertyChanged(nameof(PageSegmenter));
            }
        }

        public void SetPageSegmenter(Type pageSegmenter)
        {
            if (_pdfPageModel != null && pageSegmenter != null)
            {
                _pdfPageModel.SetPageSegmenter(pageSegmenter);
                if (IsDisplayTextLines)
                {
                    DisplayTextLines();
                }

                if (IsDisplayTextBlocks)
                {
                    DisplayTextBlocks();
                }
            }
        }

        public void HidePagePlotModel()
        {
            _currentSeries = PagePlotModel?.Series.ToList();
            PagePlotModel?.Series.Clear();
        }

        public void ShowPagePlotModel()
        {
            if (_currentSeries != null)
            {
                _currentSeries.ToList().ForEach(s => PagePlotModel.Series.Add(s));
                PagePlotModel.InvalidatePlot(true);
                this.RaisePropertyChanged(nameof(PagePlotModel));
            }
        }

        public PlotModel PagePlotModel
        {
            get
            {
                return _pagePlotModel;
            }

            private set
            {
                _pagePlotModel = value;
                this.RaisePropertyChanged(nameof(PagePlotModel));
            }
        }

        public PlotModel HeightHistoPlotModel
        {
            get
            {
                return _heightHistoPlotModel;
            }

            private set
            {
                _heightHistoPlotModel = value;
                this.RaisePropertyChanged(nameof(HeightHistoPlotModel));
            }
        }

        public PlotModel WidthHistoPlotModel
        {
            get
            {
                return _widthtHistoPlotModel;
            }

            private set
            {
                _widthtHistoPlotModel = value;
                this.RaisePropertyChanged(nameof(WidthHistoPlotModel));
            }
        }

        public OxyImage PageImage { get; private set; }

        public int CurrentPageNumber
        {
            get
            {
                return _currentPageNumber;
            }

            set
            {
                if (value > NumberOfPages || value < 1)
                {
                    return;
                }

                _currentPageNumber = value;
                LoadPage(_currentPageNumber);
                this.RaisePropertyChanged(nameof(CurrentPageNumber));
            }
        }

        public int NumberOfPages
        {
            get
            {
                return _numberOfPages;
            }

            private set
            {
                _numberOfPages = value;
                this.RaisePropertyChanged(nameof(NumberOfPages));
            }
        }

        private bool _clipPaths;
        public bool ClipPaths
        {
            get
            {
                return _clipPaths;
            }

            set
            {
                if (value == _clipPaths) return;
                _clipPaths = value;
                if (!string.IsNullOrEmpty(_pdfPath))
                {
                    _pdfDocumentModel = PdfDocumentModel.Open(_pdfPath, _clipPaths);
                    LoadPage(CurrentPageNumber);
                }
                this.RaisePropertyChanged(nameof(ClipPaths));
            }
        }

        private bool _removeDuplicateLetters;
        public bool RemoveDuplicateLetters
        {
            get
            {
                return _removeDuplicateLetters;
            }

            set
            {
                if (value == _removeDuplicateLetters) return;
                _removeDuplicateLetters = value;

                if (_pdfPageModel != null)
                {
                    _pdfPageModel.SetRemoveDuplicateLetters(_removeDuplicateLetters);
                }

                if (IsDisplayLetters)
                {
                    DisplayLetters();
                }

                if (IsDisplayWords)
                {
                    DisplayWords();
                }

                if (IsDisplayTextLines)
                {
                    DisplayTextLines();
                }

                if (IsDisplayTextBlocks)
                {
                    DisplayTextBlocks();
                }

                if (IsDisplayWsCover)
                {
                    DisplayWsCover();
                }

                this.RaisePropertyChanged(nameof(RemoveDuplicateLetters));
            }
        }

        bool _isDisplayLetters;
        public bool IsDisplayLetters
        {
            get
            {
                return _isDisplayLetters;
            }

            set
            {
                if (value == _isDisplayLetters) return;
                _isDisplayLetters = value;

                if (_isDisplayLetters)
                {
                    DisplayLetters();
                }
                else
                {
                    HideLetters();
                }

                this.RaisePropertyChanged(nameof(IsDisplayLetters));
            }
        }


        bool _isDisplayWords;
        public bool IsDisplayWords
        {
            get
            {
                return _isDisplayWords;
            }

            set
            {
                if (value == _isDisplayWords) return;
                _isDisplayWords = value;

                if (_isDisplayWords)
                {
                    DisplayWords();
                }
                else
                {
                    HideWords();
                }

                this.RaisePropertyChanged(nameof(IsDisplayWords));
            }
        }

        bool _isDisplayTextLines;
        public bool IsDisplayTextLines
        {
            get
            {
                return _isDisplayTextLines;
            }

            set
            {
                if (value == _isDisplayTextLines) return;
                _isDisplayTextLines = value;

                if (_isDisplayTextLines)
                {
                    DisplayTextLines();
                }
                else
                {
                    HideTextLines();
                }

                this.RaisePropertyChanged(nameof(IsDisplayTextLines));
            }
        }

        bool _isDisplayTextBlocks;
        public bool IsDisplayTextBlocks
        {
            get
            {
                return _isDisplayTextBlocks;
            }

            set
            {
                if (value == _isDisplayTextBlocks) return;
                _isDisplayTextBlocks = value;

                if (_isDisplayTextBlocks)
                {
                    DisplayTextBlocks();
                }
                else
                {
                    HideTextBlocks();
                }

                this.RaisePropertyChanged(nameof(IsDisplayTextBlocks));
            }
        }

        bool _isDisplayPaths;
        public bool IsDisplayPaths
        {
            get
            {
                return _isDisplayPaths;
            }

            set
            {
                if (value == _isDisplayPaths) return;
                _isDisplayPaths = value;

                if (_isDisplayPaths)
                {
                    DisplayPaths();
                }
                else
                {
                    HidePaths();
                }

                this.RaisePropertyChanged(nameof(IsDisplayPaths));
            }
        }

        bool _isDisplayImages;
        public bool IsDisplayImages
        {
            get
            {
                return _isDisplayImages;
            }

            set
            {
                if (value == _isDisplayImages) return;
                _isDisplayImages = value;

                if (_isDisplayImages)
                {
                    DisplayImages();
                }
                else
                {
                    HideImages();
                }

                this.RaisePropertyChanged(nameof(IsDisplayImages));
            }
        }


        bool _isDisplayTables;
        public bool IsDisplayTables
        {
            get
            {
                return _isDisplayTables;
            }

            set
            {
                if (value == _isDisplayTables) return;
                _isDisplayTables = value;

                if (_isDisplayTables)
                {
                    DisplayTables();
                }
                else
                {
                    HideTables();
                }

                this.RaisePropertyChanged(nameof(IsDisplayTables));
            }
        }

        bool _isDisplayWsCover;
        public bool IsDisplayWsCover
        {
            get
            {
                return _isDisplayWsCover;
            }

            set
            {
                if (value == _isDisplayWsCover) return;
                _isDisplayWsCover = value;

                if (_isDisplayWsCover)
                {
                    DisplayWsCover();
                }
                else
                {
                    HideWsCover();
                }

                this.RaisePropertyChanged(nameof(IsDisplayWsCover));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel" /> class.
        /// </summary>
        public MainViewModel()
        {
            CustomController = new CustomPlotController();

            WordExtractorList = new ObservableCollection<Type>(PdfDocumentModel.GetWordExtractors());
            PageSegmenterList = new ObservableCollection<Type>(PdfDocumentModel.GetPageSegmenters());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        private string _pdfPath;

        public void OpenDocument(string path)
        {
            if (Path.GetExtension(path) != ".pdf")
            {
                return;
            }

            _pdfPath = path;
            _pdfImageConverter = new PdfImageConverter(_pdfPath);
            _pdfDocumentModel = PdfDocumentModel.Open(_pdfPath, ClipPaths);
            NumberOfPages = _pdfDocumentModel.NumberOfPages;
            PdfPigVersion = _pdfDocumentModel.PdfPigVersion;
            CurrentPageNumber = 1;
        }

        private bool LoadPage(int pageNo)
        {
            if (_pdfDocumentModel == null) return false;

            _pdfPageModel = _pdfDocumentModel.GetPage(pageNo);

            if (_pdfPageModel == null) return false;

            // set remove duplicate letters
            _pdfPageModel.SetRemoveDuplicateLetters(_removeDuplicateLetters);

            // set word extractor
            _pdfPageModel.SetWordExtractor(WordExtractor);

            // set page segmenter
            _pdfPageModel.SetPageSegmenter(PageSegmenter);

            var pageInfoModel = _pdfPageModel.GetPageInfo();

            // Plot height distrib
            HeightHistoPlotModel = pageInfoModel.HeightDistribution?.GetPlotModel("Letters height distribution");
            WidthHistoPlotModel = pageInfoModel.WidthDistribution?.GetPlotModel("Letters width distribution");

            // Plot page 
            var pagePlotModel = new PlotModel { IsLegendVisible = false };
            pagePlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Minimum = 0, Maximum = _pdfPageModel.Height });
            pagePlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Minimum = 0, Maximum = _pdfPageModel.Width });

            // Add background image
            try
            {
                using (var stream = _pdfImageConverter.GetPageStream(pageNo, 2))
                {
                    PageImage = new OxyImage(stream);
                }

                pagePlotModel.Annotations.Add(new ImageAnnotation
                {
                    ImageSource = PageImage,
                    Opacity = 0.5,
                    X = new PlotLength(_pdfPageModel.CropBox.Bounds.BottomLeft.X, PlotLengthUnit.Data),
                    Y = new PlotLength(_pdfPageModel.CropBox.Bounds.BottomLeft.Y, PlotLengthUnit.Data),
                    Width = new PlotLength(_pdfPageModel.CropBox.Bounds.Width, PlotLengthUnit.Data),
                    Height = new PlotLength(_pdfPageModel.CropBox.Bounds.Height, PlotLengthUnit.Data),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Bottom
                });
            }
            catch (Exception)
            {
                throw;
            }

            this.PagePlotModel = pagePlotModel;

            if (IsDisplayLetters)
            {
                DisplayLetters();
            }

            if (IsDisplayWords)
            {
                DisplayWords();
            }

            if (IsDisplayTextLines)
            {
                DisplayTextLines();
            }

            if (IsDisplayTextBlocks)
            {
                DisplayTextBlocks();
            }

            if (IsDisplayPaths)
            {
                DisplayPaths();
            }

            if (IsDisplayImages)
            {
                DisplayImages();
            }

            if (IsDisplayTables)
            {
                DisplayTables();
            }

            if (IsDisplayWsCover)
            {
                DisplayWsCover();
            }

            return true;
        }


        public void DisplayWsCover()
        {
            if (PagePlotModel == null) return;

            foreach (var s in PagePlotModel.Series.Where(s => (string)s.Tag == "ws_cover").ToList())
            {
                PagePlotModel.Series.Remove(s);
            }

            foreach (var bbox in _pdfPageModel.GetWhitespaceCover())
            {
                var series1 = new LineSeries { Tag = "ws_cover", Title = "cover", LineStyle = LineStyle.Solid, Color = OxyColors.CadetBlue };
                series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.BottomLeft));
                series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.BottomRight));
                series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.TopRight));
                series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.TopLeft));
                series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.BottomLeft));
                PagePlotModel.Series.Add(series1);
            }

            PagePlotModel.InvalidatePlot(true);
        }

        public void HideWsCover()
        {
            if (PagePlotModel == null) return;

            foreach (var s in PagePlotModel.Series.Where(s => (string)s.Tag == "ws_cover").ToList())
            {
                PagePlotModel.Series.Remove(s);
            }

            PagePlotModel.InvalidatePlot(true);
        }






        public void DisplayLetters()
        {
            if (PagePlotModel == null) return;

            foreach (var s in PagePlotModel.Series.Where(s => (string)s.Tag == "letter").ToList())
            {
                PagePlotModel.Series.Remove(s);
            }

            foreach (var letter in _pdfPageModel.GetLetters())
            {
                var series1 = new LineSeries { Tag = "letter", Title = GetShorterText(letter.Value), LineStyle = LineStyle.Solid, Color = OxyColors.Blue };
                var bbox = letter.GlyphRectangle;
                series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.BottomLeft));
                series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.BottomRight));
                series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.TopRight));
                series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.TopLeft));
                series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.BottomLeft));
                PagePlotModel.Series.Add(series1);
            }

            PagePlotModel.InvalidatePlot(true);
        }

        public void HideLetters()
        {
            if (PagePlotModel == null) return;

            foreach (var s in PagePlotModel.Series.Where(s => (string)s.Tag == "letter").ToList())
            {
                PagePlotModel.Series.Remove(s);
            }

            PagePlotModel.InvalidatePlot(true);
        }

        public void DisplayWords()
        {
            if (PagePlotModel == null) return;

            foreach (var s in PagePlotModel.Series.Where(s => (string)s.Tag == "word").ToList())
            {
                PagePlotModel.Series.Remove(s);
            }

            foreach (var word in _pdfPageModel.GetWords())
            {
                var series1 = new LineSeries { Tag = "word", Title = GetShorterText(word.Text), LineStyle = LineStyle.Solid, Color = OxyColors.Red };
                var bbox = word.BoundingBox;
                series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.BottomLeft));
                series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.BottomRight));
                series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.TopRight));
                series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.TopLeft));
                series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.BottomLeft));
                PagePlotModel.Series.Add(series1);
            }

            PagePlotModel.InvalidatePlot(true);
        }

        public void HideWords()
        {
            if (PagePlotModel == null) return;

            foreach (var s in PagePlotModel.Series.Where(s => (string)s.Tag == "word").ToList())
            {
                PagePlotModel.Series.Remove(s);
            }

            PagePlotModel.InvalidatePlot(true);
        }

        public void DisplayTextLines()
        {
            if (PagePlotModel == null) return;

            foreach (var s in PagePlotModel.Series.Where(s => (string)s.Tag == "textline").ToList())
            {
                PagePlotModel.Series.Remove(s);
            }

            foreach (var line in _pdfPageModel.GetTextBlocks().SelectMany(b => b.TextLines))
            {
                var series1 = new LineSeries { Tag = "textline", Title = GetShorterText(line.Text), LineStyle = LineStyle.Solid, Color = OxyColors.OrangeRed };
                var bbox = line.BoundingBox;
                series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.BottomLeft));
                series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.BottomRight));
                series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.TopRight));
                series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.TopLeft));
                series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.BottomLeft));
                PagePlotModel.Series.Add(series1);
            }

            PagePlotModel.InvalidatePlot(true);
        }

        public void HideTextLines()
        {
            if (PagePlotModel == null) return;

            foreach (var s in PagePlotModel.Series.Where(s => (string)s.Tag == "textline").ToList())
            {
                PagePlotModel.Series.Remove(s);
            }

            PagePlotModel.InvalidatePlot(true);
        }

        public void DisplayTextBlocks()
        {
            if (PagePlotModel == null) return;

            foreach (var s in PagePlotModel.Series.Where(s => (string)s.Tag == "textblock").ToList())
            {
                PagePlotModel.Series.Remove(s);
            }

            foreach (var block in _pdfPageModel.GetTextBlocks())
            {
                var series1 = new LineSeries { Tag = "textblock", Title = GetShorterText(block.Text), LineStyle = LineStyle.Solid, Color = OxyColors.Brown };
                var bbox = block.BoundingBox;
                series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.BottomLeft));
                series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.BottomRight));
                series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.TopRight));
                series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.TopLeft));
                series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.BottomLeft));
                PagePlotModel.Series.Add(series1);
            }

            PagePlotModel.InvalidatePlot(true);
        }

        public void HideTextBlocks()
        {
            if (PagePlotModel == null) return;

            foreach (var s in PagePlotModel.Series.Where(s => (string)s.Tag == "textblock").ToList())
            {
                PagePlotModel.Series.Remove(s);
            }

            PagePlotModel.InvalidatePlot(true);
        }

        public void DisplayPaths()
        {
            if (PagePlotModel == null) return;

            foreach (var s in PagePlotModel.Series.Where(s => (string)s.Tag == "pdfpath").ToList())
            {
                PagePlotModel.Series.Remove(s);
            }

            foreach (var path in _pdfPageModel.GetPdfPaths())
            {
                foreach (var sp in path)
                {
                    string title = ("path: " + (path.IsStroked ? "stroked " + (path.StrokeColor?.ToRGBValues()).ToString() + " " + path.LineWidth : "") +
                                               (path.IsFilled ? "filled " + (path.FillColor?.ToRGBValues()).ToString() : "") +
                                               (path.IsClipping ? "clipping" : "")
                                               ).Trim();
                    var series1 = new LineSeries { Tag = "pdfpath", Title = title, LineStyle = LineStyle.Solid, Color = OxyColors.Yellow };

                    PdfPoint first = PdfPoint.Origin;
                    foreach (var c in sp.Commands)
                    {
                        if (c is Move m)
                        {
                            first = m.Location;
                            series1.Points.Add(PdfDocumentModel.ToDataPoint(first));
                        }
                        else if (c is Line l)
                        {
                            series1.Points.Add(PdfDocumentModel.ToDataPoint(l.From));
                            series1.Points.Add(PdfDocumentModel.ToDataPoint(l.To));
                        }
                        else if (c is BezierCurve bc)
                        {
                            var lines = bc.ToLines(10).ToList();
                            for (int i = 0; i < lines.Count; i++)
                            {
                                series1.Points.Add(PdfDocumentModel.ToDataPoint(lines[i].From));
                                series1.Points.Add(PdfDocumentModel.ToDataPoint(lines[i].To));
                            }
                        }
                        else if (c is Close)
                        {
                            series1.Points.Add(PdfDocumentModel.ToDataPoint(first));
                        }
                        else
                        {
                            throw new ArgumentException();
                        }
                    }

                    PagePlotModel.Series.Add(series1);
                }
            }

            PagePlotModel.InvalidatePlot(true);
        }

        public void HidePaths()
        {
            if (PagePlotModel == null) return;

            foreach (var s in PagePlotModel.Series.Where(s => (string)s.Tag == "pdfpath").ToList())
            {
                PagePlotModel.Series.Remove(s);
            }

            PagePlotModel.InvalidatePlot(true);
        }

        public void DisplayImages()
        {
            if (PagePlotModel == null) return;

            foreach (var s in PagePlotModel.Series.Where(s => (string)s.Tag == "image").ToList())
            {
                PagePlotModel.Series.Remove(s);
            }

            foreach (var block in _pdfPageModel.GetImages())
            {
                var series1 = new LineSeries { Tag = "image", Title = "image", LineStyle = LineStyle.Solid, Color = OxyColors.YellowGreen };
                var bbox = block.Bounds;
                series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.BottomLeft));
                series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.BottomRight));
                series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.TopRight));
                series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.TopLeft));
                series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.BottomLeft));
                PagePlotModel.Series.Add(series1);
            }

            PagePlotModel.InvalidatePlot(true);
        }

        public void HideImages()
        {
            if (PagePlotModel == null) return;

            foreach (var s in PagePlotModel.Series.Where(s => (string)s.Tag == "image").ToList())
            {
                PagePlotModel.Series.Remove(s);
            }

            PagePlotModel.InvalidatePlot(true);
        }

        public void DisplayTables()
        {
            if (PagePlotModel == null) return;

            foreach (var s in PagePlotModel.Series.Where(s => (string)s.Tag == "table").ToList())
            {
                PagePlotModel.Series.Remove(s);
            }

            foreach (var s in PagePlotModel.Series.Where(s => (string)s.Tag == "cell").ToList())
            {
                PagePlotModel.Series.Remove(s);
            }

            foreach (var table in _pdfPageModel.GetTables())
            {
                var series1 = new LineSeries { Tag = "table", Title = "table", LineStyle = LineStyle.Solid, StrokeThickness = 5.0, Color = OxyColors.Purple };
                var bbox = table.BoundingBox;
                series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.BottomLeft));
                series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.BottomRight));
                series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.TopRight));
                series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.TopLeft));
                series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.BottomLeft));
                PagePlotModel.Series.Add(series1);

                for (int r = 0; r < table.RowCount; r++)
                {
                    for (int c = 0; c < table.ColumnCount; c++)
                    {
                        var cell = table[r, c];
                        var bboxCell = cell.BoundingBox;
                        var series2 = new LineSeries { Tag = "cell", Title = $"cell[{r},{c}] {cell.IsSpanning} ({cell.GetText()})", LineStyle = LineStyle.DashDot, Color = OxyColors.Orange };
                        series2.Points.Add(PdfDocumentModel.ToDataPoint(bboxCell.BottomLeft));
                        series2.Points.Add(PdfDocumentModel.ToDataPoint(bboxCell.BottomRight));
                        series2.Points.Add(PdfDocumentModel.ToDataPoint(bboxCell.TopRight));
                        series2.Points.Add(PdfDocumentModel.ToDataPoint(bboxCell.TopLeft));
                        series2.Points.Add(PdfDocumentModel.ToDataPoint(bboxCell.BottomLeft));
                        PagePlotModel.Series.Add(series2);
                    }
                }
            }

            PagePlotModel.InvalidatePlot(true);
        }

        public void HideTables()
        {
            if (PagePlotModel == null) return;

            foreach (var s in PagePlotModel.Series.Where(s => (string)s.Tag == "table").ToList())
            {
                PagePlotModel.Series.Remove(s);
            }

            foreach (var s in PagePlotModel.Series.Where(s => (string)s.Tag == "cell").ToList())
            {
                PagePlotModel.Series.Remove(s);
            }

            PagePlotModel.InvalidatePlot(true);
        }

        private string GetShorterText(string text)
        {
            if (text.Length <= 25) return text;
            return string.Join("", text.Take(22)) + "...";
        }

        public void Dispose()
        {
            _pdfImageConverter.Dispose();
            // other dispose
        }
    }
}
