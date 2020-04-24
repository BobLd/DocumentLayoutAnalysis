namespace DlaViewer
{
    using ImageConverter;
    using OxyPlot;
    using OxyPlot.Annotations;
    using OxyPlot.Axes;
    using OxyPlot.Series;
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;

    public class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        public PlotController CustomController { get; }

        PdfImageConverter _pdfImageConverter;
        private PdfDocumentModel _pdfDocumentModel;

        private PlotModel _plotModel;
        private int _numberOfPages;
        private int _currentPageNumber;
        private string _bboxLevel;

        /// <summary>
        /// Gets the plot model.
        /// </summary>
        public PlotModel PlotModel
        {
            get
            {
                return _plotModel;
            }

            private set
            {
                _plotModel = value;
                this.RaisePropertyChanged("PlotModel");
            }
        }

        public OxyImage Image { get; private set; }

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
                this.RaisePropertyChanged("CurrentPageNumber");
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
                this.RaisePropertyChanged("NumberOfPages");
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
                this.RaisePropertyChanged("BboxLevel");
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

            // Create the plot model
            var tmp = new PlotModel { IsLegendVisible = false };
            tmp.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Minimum = 0, Maximum = page.Height });
            tmp.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Minimum = 0, Maximum = page.Width });

   

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
                        tmp.Series.Add(series1);
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
                        tmp.Series.Add(series1);
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
                        tmp.Series.Add(series1);
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
                        tmp.Series.Add(series1);
                    }
                    break;
            }

            // Add background image
            try
            {
                using (var stream = _pdfImageConverter.GetPageStream(pageNo, 2))
                {
                    Image = new OxyImage(stream);
                }

                tmp.Annotations.Add(new ImageAnnotation
                {
                    ImageSource = Image,
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

            this.PlotModel = tmp;
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
