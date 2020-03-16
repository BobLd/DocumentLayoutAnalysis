namespace DocumentLayoutAnalysis
{
    using System.Collections.Generic;
    using UglyToad.PdfPig.Core;
    
    /*
     * Sources:
     * https://www.codeproject.com/Articles/18936/A-C-Implementation-of-Douglas-Peucker-Line-Approxi
     * https://codereview.stackexchange.com/questions/29002/ramer-douglas-peucker-algorithm
     * Optimisations:
     *  - Do not use Sqrt function
     *  - Use unsafe code
     *  - Avoid duplicate computations in loop
     */
    public static class RamerDouglasPeucker
    {
        /// <summary>
        /// Uses the Ramer Douglas Peucker algorithm to reduce the number of points.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns></returns>
        public static PdfPoint[] Reduce(PdfPoint[] points, double tolerance)
        {
            if (points == null || points.Length < 3) return points;
            if (double.IsInfinity(tolerance) || double.IsNaN(tolerance)) return points;
            tolerance *= tolerance;
            if (tolerance <= float.Epsilon) return points;

            int firstIndex = 0;
            int lastIndex = points.Length - 1;
            List<int> indexesToKeep = new List<int>();

            indexesToKeep.Add(firstIndex);
            indexesToKeep.Add(lastIndex);

            while (points[firstIndex].Equals(points[lastIndex]))
            {
                lastIndex--;
            }

            Reduce(points, firstIndex, lastIndex, tolerance, ref indexesToKeep);

            int l = indexesToKeep.Count;
            PdfPoint[] returnPoints = new PdfPoint[l];
            indexesToKeep.Sort();

            unsafe
            {
                fixed (PdfPoint* ptr = points, result = returnPoints)
                {
                    PdfPoint* res = result;
                    for (int i = 0; i < l; ++i)
                        *(res + i) = *(ptr + indexesToKeep[i]);
                }
            }

            return returnPoints;
        }

        /// <summary>
        /// Douglases the peucker reduction.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="firstIndex">The first point index.</param>
        /// <param name="lastIndex">The last point index.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <param name="indexesToKeep">The points' index to keep.</param>
        private static void Reduce(PdfPoint[] points, int firstIndex, int lastIndex, double tolerance, ref List<int> indexesToKeep)
        {
            double maxDistance = 0;
            int indexFarthest = 0;

            unsafe
            {
                fixed (PdfPoint* samples = points)
                {
                    PdfPoint point1 = *(samples + firstIndex);
                    PdfPoint point2 = *(samples + lastIndex);
                    double distXY = (double)point1.X * (double)point2.Y - (double)point2.X * (double)point1.Y;
                    double distX = (double)point2.X - (double)point1.X;
                    double distY = (double)point1.Y - (double)point2.Y;
                    double bottom = distX * distX + distY * distY;

                    for (int index = firstIndex; index < lastIndex; index++)
                    {
                        PdfPoint point = *(samples + index);
                        double area = distXY + distX * (double)point.Y + distY * (double)point.X;
                        double distance = (area / bottom) * area;

                        if (distance > maxDistance)
                        {
                            maxDistance = distance;
                            indexFarthest = index;
                        }
                    }
                }
            }

            if (maxDistance > tolerance && indexFarthest != 0)
            {
                //Add the largest point that exceeds the tolerance
                indexesToKeep.Add(indexFarthest);
                Reduce(points, firstIndex, indexFarthest, tolerance, ref indexesToKeep);
                Reduce(points, indexFarthest, lastIndex, tolerance, ref indexesToKeep);
            }
        }
    }
}
