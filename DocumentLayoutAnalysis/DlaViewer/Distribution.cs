namespace DlaViewer
{
    using OxyPlot;
    using OxyPlot.Axes;
    using OxyPlot.Series;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UglyToad.PdfPig.DocumentLayoutAnalysis;

    public class Distribution
    {
        public IReadOnlyList<double> Values { get; }

        public double Mode { get; }

        public double Average { get; }

        public int BinLength { get; }

        public Dictionary<int, List<double>> Bins { get; set; }

        public Distribution(IEnumerable<double> values, int binLength = 1)
        {
            if (binLength <= 0)
            {
                throw new ArgumentException();
            }

            Values = values.Select(x => Math.Round(x, 5)).ToList();
            Average = Values.Average();
            Mode = Values.Mode();
            BinLength = binLength;

            var max = (int)Math.Ceiling(Values.Max());
            if (max == 0)
            {
                max = binLength;
            }
            else
            {
                binLength = binLength > max ? max : binLength;
            }

            var bins = Enumerable.Range(0, (int)Math.Ceiling(max / (double)binLength) + 1)
                .Select(x => x * binLength)
                .ToDictionary(x => x, _ => new List<double>());

            foreach (var value in Values)
            {
                var key = bins.Keys.ElementAt((int)Math.Floor(value / binLength));
                bins[key].Add(value);
            }

            Bins = bins;
        }

        public PlotModel GetPlotModel(string title = "", double titleFontSize = 12)
        {
            var distPlotModel = new PlotModel { Title = title, IsLegendVisible = true, TitleFontSize = titleFontSize };
            distPlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Minimum = 0.0 });

            var histoSeries = new HistogramSeries() { FillColor = OxyColors.Blue, StrokeThickness = 1, StrokeColor = OxyColors.Black, RenderInLegend = false };
            double peakDist = double.NegativeInfinity;
            foreach (var bin in Bins)
            {
                double pct = bin.Value.Count / (double)Values.Count;
                if (pct > peakDist) peakDist = pct;
                double start = bin.Key;
                double end = bin.Key + BinLength - 10e-7;
                histoSeries.Items.Add(new HistogramItem(start, end, pct, bin.Value.Count));
            }
            distPlotModel.Series.Add(histoSeries);

            // plot average
            ScatterSeries averageSeries = new ScatterSeries() { MarkerType = MarkerType.Diamond, MarkerFill = OxyColors.Red, MarkerSize = 5, Title = "Average" };
            averageSeries.Points.Add(new ScatterPoint(Average, peakDist / 2.0));
            distPlotModel.Series.Add(averageSeries);

            if (!double.IsNaN(Mode))
            {
                // plot mode
                ScatterSeries modeSeries = new ScatterSeries() { MarkerType = MarkerType.Circle, MarkerFill = OxyColors.Orange, MarkerSize = 5, Title = "Mode" };
                modeSeries.Points.Add(new ScatterPoint(Mode, peakDist / 2.0));
                distPlotModel.Series.Add(modeSeries);
            }

            return distPlotModel;
        }
    }

    /*
    public class DistributionBin
    {
        /// <summary>
        /// Included
        /// </summary>
        public int Start { get; }

        /// <summary>
        /// Excluded
        /// </summary>
        public int End { get; }

        public double Average;

        public double Mode;

        public DistributionBin(int start, int end, IEnumerable<double> values = null)
        {

        }
    }
    */
}
