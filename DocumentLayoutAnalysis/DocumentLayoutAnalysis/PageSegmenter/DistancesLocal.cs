using System;
using System.Collections.Generic;
using System.Linq;
using UglyToad.PdfPig.Geometry;

namespace DocumentLayoutAnalysis.PageSegmenter
{
    public static class DistancesLocal
    {
        /// <summary>
        /// Find the index of the nearest point.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="element">The reference point, for which to find the nearest neighbour.</param>
        /// <param name="candidates">The list of neighbours candidates.</param>
        /// <param name="candidatesPoint"></param>
        /// <param name="pivotPoint"></param>
        /// <param name="distanceMeasure">The distance measure to use.</param>
        /// <param name="distance">The distance between reference point, and its nearest neighbour.</param>
        /// <returns></returns>
        public static int FindIndexNearest<T>(this T element, IReadOnlyList<T> candidates,
            Func<T, PdfPoint> candidatesPoint, Func<T, PdfPoint> pivotPoint,
            Func<PdfPoint, PdfPoint, double> distanceMeasure, out double distance)
        {
            if (candidates == null || candidates.Count == 0)
            {
                throw new ArgumentException("Distances.FindIndexNearest(): The list of neighbours candidates is either null or empty.", "points");
            }

            if (distanceMeasure == null)
            {
                throw new ArgumentException("Distances.FindIndexNearest(): The distance measure must not be null.", "distanceMeasure");
            }

            distance = double.MaxValue;
            int closestPointIndex = -1;
            var candidatesPoints = candidates.Select(candidatesPoint).ToList();
            var pivot = pivotPoint(element);

            for (var i = 0; i < candidates.Count; i++)
            {
                double currentDistance = distanceMeasure(candidatesPoints[i], pivot);
                if (currentDistance < distance && !candidates[i].Equals(element))
                {
                    distance = currentDistance;
                    closestPointIndex = i;
                }
            }

            return closestPointIndex;
        }

        /// <summary>
        /// Find the index of the nearest line.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="element">The reference line, for which to find the nearest neighbour.</param>
        /// <param name="candidates">The list of neighbours candidates.</param>
        /// <param name="candidatesLine"></param>
        /// <param name="pivotLine"></param>
        /// <param name="distanceMeasure">The distance measure between two lines to use.</param>
        /// <param name="distance">The distance between reference line, and its nearest neighbour.</param>
        public static int FindIndexNearest<T>(this T element, IReadOnlyList<T> candidates,
            Func<T, PdfLine> candidatesLine, Func<T, PdfLine> pivotLine,
            Func<PdfLine, PdfLine, double> distanceMeasure, out double distance)
        {
            if (candidates == null || candidates.Count == 0)
            {
                throw new ArgumentException("Distances.FindIndexNearest(): The list of neighbours candidates is either null or empty.", "lines");
            }

            if (distanceMeasure == null)
            {
                throw new ArgumentException("Distances.FindIndexNearest(): The distance measure must not be null.", "distanceMeasure");
            }

            distance = double.MaxValue;
            int closestLineIndex = -1;
            var candidatesLines = candidates.Select(candidatesLine).ToList();
            var pivot = pivotLine(element);

            for (var i = 0; i < candidates.Count; i++)
            {
                double currentDistance = distanceMeasure(candidatesLines[i], pivot);
                if (currentDistance < distance && !candidates[i].Equals(element))
                {
                    distance = currentDistance;
                    closestLineIndex = i;
                }
            }

            return closestLineIndex;
        }
    }
}
