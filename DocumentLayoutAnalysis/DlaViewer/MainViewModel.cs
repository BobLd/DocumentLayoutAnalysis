namespace DlaViewer
{
    using ImageConverter;
    using OxyPlot;
    using OxyPlot.Annotations;
    using OxyPlot.Axes;
    using OxyPlot.Series;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;

    public class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        public PlotController CustomController { get; }

        private PdfImageConverter _pdfImageConverter;
        private PdfDocumentModel _pdfDocumentModel;

        private PlotModel _pagePlotModel;
        private IList<Series> _currentSeries;
        private PlotModel _heightHistoPlotModel;
        private PlotModel _widthtHistoPlotModel;

        private int _numberOfPages;
        private int _currentPageNumber;
        private string _bboxLevel;

        /*
        public double _pageHeight;
        public double PageHeight
        {
            get
            {
                return _pageHeight;
            }

            set
            {
                _pageHeight = value;
                this.RaisePropertyChanged(nameof(PageHeight));
            }
        }

        public double _pageWidth;
        public double PageWidth
        {
            get
            {
                return _pageWidth;
            }

            set
            {
                _pageWidth = value;
                this.RaisePropertyChanged(nameof(PageWidth));
            }
        }
        */

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
                    // messagebox here
                    return;
                }

                _currentPageNumber = value;
                DisplayPage(_currentPageNumber);
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

        public string BboxLevel
        {
            get
            {
                return _bboxLevel;
            }

            set
            {
                if (value == _bboxLevel) return;
                _bboxLevel = value;
                DisplayPage(CurrentPageNumber);
                this.RaisePropertyChanged(nameof(BboxLevel));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel" /> class.
        /// </summary>
        public MainViewModel()
        {
            CustomController = new CustomPlotController();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string property)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public void OpenDocument(string path)
        {
            if (Path.GetExtension(path) != ".pdf")
            {
                return;
            }

            _pdfImageConverter = new PdfImageConverter(path);
            _pdfDocumentModel = PdfDocumentModel.Open(path);
            NumberOfPages = _pdfDocumentModel.NumberOfPages;
            CurrentPageNumber = 1;
        }

        private void DisplayPage(int pageNo)
        {
            if (_pdfDocumentModel == null) return;

            var page = _pdfDocumentModel.GetPage(pageNo);

            var pageInfoModel = page.GetPageInfo();

            // Plot height distrib
            HeightHistoPlotModel = pageInfoModel.HeightDistribution.GetPlotModel("Letters height distribution");
            WidthHistoPlotModel = pageInfoModel.WidthDistribution.GetPlotModel("Letters width distribution");

            // Plot page 
            var pagePlotModel = new PlotModel { IsLegendVisible = false };
            pagePlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Minimum = 0, Maximum = page.Height });
            pagePlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Minimum = 0, Maximum = page.Width });
            
            switch (BboxLevel)
            {
                case "Words":
                    foreach (var word in page.GetWords())
                    {
                        var series1 = new LineSeries { Title = GetShorterText(word.Text), LineStyle = LineStyle.Solid, Color = OxyColors.Red };
                        var bbox = word.BoundingBox;
                        series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.BottomLeft));
                        series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.BottomRight));
                        series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.TopRight));
                        series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.TopLeft));
                        series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.BottomLeft));
                        pagePlotModel.Series.Add(series1);
                    }
                    break;

                case "Lines":
                    foreach (var line in page.GetTextBlocks().SelectMany(b => b.TextLines))
                    {
                        var series1 = new LineSeries { Title = GetShorterText(line.Text), LineStyle = LineStyle.Solid, Color = OxyColors.Red };
                        var bbox = line.BoundingBox;
                        series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.BottomLeft));
                        series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.BottomRight));
                        series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.TopRight));
                        series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.TopLeft));
                        series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.BottomLeft));
                        pagePlotModel.Series.Add(series1);
                    }
                    break;

                case "Paragraphs":
                    foreach (var block in page.GetTextBlocks())
                    {
                        var series1 = new LineSeries { Title = GetShorterText(block.Text), LineStyle = LineStyle.Solid, Color = OxyColors.Red };
                        var bbox = block.BoundingBox;
                        series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.BottomLeft));
                        series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.BottomRight));
                        series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.TopRight));
                        series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.TopLeft));
                        series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.BottomLeft));
                        pagePlotModel.Series.Add(series1);
                    }
                    break;

                default:
                    foreach (var letter in page.GetLetters())
                    {
                        var series1 = new LineSeries { Title = letter.Value, LineStyle = LineStyle.Solid, Color = OxyColors.Red };
                        var bbox = letter.GlyphRectangle;
                        series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.BottomLeft));
                        series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.BottomRight));
                        series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.TopRight));
                        series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.TopLeft));
                        series1.Points.Add(PdfDocumentModel.ToDataPoint(bbox.BottomLeft));
                        pagePlotModel.Series.Add(series1);
                    }
                    break;
            }

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
                    X = new PlotLength(0, PlotLengthUnit.Data),
                    Y = new PlotLength(0, PlotLengthUnit.Data),
                    Width = new PlotLength(page.Width, PlotLengthUnit.Data),
                    Height = new PlotLength(page.Height, PlotLengthUnit.Data),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Bottom
                });
            }
            catch (Exception)
            {
                throw;
            }


            this.PagePlotModel = pagePlotModel;
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
