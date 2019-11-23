using System;
using UglyToad.PdfPig.Geometry;

namespace DocumentLayoutAnalysis.StraightSkeleton
{
    //public class Edge
    //{
    //    public 
    //}

    // http://www.dma.fi.upm.es/personal/mabellanas/tfcs/skeleton/html/documentacion/Straight%20Skeletons%20Implementation.pdf
    // Point in polygon?: https://en.wikipedia.org/wiki/Point_in_polygon
    public class Vertex
    {
        public PdfPoint Point { get; }
        public PdfPath.Line LeftEdge { get; }
        public PdfPath.Line RightEdge { get; }
        public bool IsValid { get; }
        public bool IsProcessed { get; private set; }

        public double AngleBisector { get; }
        public PdfPath.Line AngleBisectorRay { get; }
        public double AngleBisectorSlope { get; }
        public double AngleBisectorIntercept { get; }
        
        public Vertex(PdfPath.Line leftEdge, PdfPath.Line rightEdge)
        {
            AngleBisectorSlope = double.NaN;
            AngleBisectorIntercept = double.NaN;
            AngleBisector = double.NaN;

            if (leftEdge.To.Equals(rightEdge.From)) IsValid = true;

            LeftEdge = leftEdge;
            RightEdge = rightEdge;

            if (IsValid)
            {
                AngleBisector = Angle(LeftEdge, RightEdge) / 2.0;

                // Rotate
                var deltaX = (double)(LeftEdge.From.X - LeftEdge.To.X);
                var deltaY = (double)(LeftEdge.From.Y - LeftEdge.To.Y);
                var cos = Math.Cos(AngleBisector);
                var sin = Math.Sin(AngleBisector);

                var newX = cos * deltaX - sin * deltaY + (double)LeftEdge.To.X;
                var newY = sin * deltaX + cos * deltaY + (double)LeftEdge.To.Y;
                AngleBisectorRay = new PdfPath.Line(LeftEdge.To, new PdfPoint(newX, newY));

                AngleBisectorSlope = (double)((AngleBisectorRay.To.Y - AngleBisectorRay.From.Y) / (AngleBisectorRay.To.X - AngleBisectorRay.From.X));
                AngleBisectorIntercept = (double)AngleBisectorRay.To.Y - AngleBisectorSlope * (double)AngleBisectorRay.To.X;
            }
        }

        public void MarkAsProcessed()
        {
            IsProcessed = true;
        }

        public static double DistancePointToLine(PdfPath.Line line, PdfPoint point)
        {
            var denominator = Math.Sqrt((double)(line.To.Y - line.From.Y) * (double)(line.To.Y - line.From.Y) +
                                        (double)(line.To.X - line.From.X) * (double)(line.To.X - line.From.X));

            var numerator = (double)Math.Abs((line.To.Y - line.From.Y) * point.X - (line.To.X - line.From.X) * point.Y +
                                              line.To.X * line.From.Y - line.To.Y * line.From.X);
            return numerator / denominator;
        }

        /// <summary>
        /// Returns the point at which the both vertex angle bisectors intersect.
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public PdfPoint? AngleBisectorsInterscAt(Vertex vertex)
        {
            if (double.IsNaN(AngleBisectorSlope) || double.IsNaN(vertex.AngleBisectorSlope)) return null;
            if (AngleBisectorSlope == vertex.AngleBisectorSlope) return null; // both are parallel (but the can overlap...)

            var x = (vertex.AngleBisectorIntercept - AngleBisectorIntercept) / (AngleBisectorSlope - vertex.AngleBisectorSlope);
            var y = AngleBisectorSlope * x + AngleBisectorIntercept;
            var intersection = new PdfPoint(x, y);

            // check if the intersection point belongs to both rays (for the moment we only know it belongs to both lines)
            if (!PointBelongsToRay(this.AngleBisectorRay.From, this.AngleBisectorRay.To, intersection)) return null;
            if (!PointBelongsToRay(vertex.AngleBisectorRay.From, vertex.AngleBisectorRay.To, intersection)) return null;
            return intersection;
        }

        public override string ToString()
        {
            return (IsValid ? "[OK]" : "[NOK]") +
                //" " + LeftEdge.ToString() + " -> " + RightEdge.ToString() +
                ", angle G: " + AngleBisector.ToString("0.00");
        }

        private bool PointBelongsToRay(PdfPoint rayOriginPoint, PdfPoint raySecondPoint, PdfPoint pointCheck)
        {
            // https://math.stackexchange.com/questions/1766357/check-if-a-given-coordinate-lies-in-path-of-a-ray-coordinate-geometry
            var tx = (pointCheck.X - rayOriginPoint.X) / (raySecondPoint.X - rayOriginPoint.X);
            if (tx < 0) return false;
            var ty = (pointCheck.Y - rayOriginPoint.Y) / (raySecondPoint.Y - rayOriginPoint.Y);
            if (ty < 0) return false;
            return true;
        }

        private double Angle(PdfPath.Line line1, PdfPath.Line line2)
        {
            var angle = (Math.Atan2((double)(line1.From.Y - line1.To.Y), (double)(line1.From.X - line1.To.X))
                       - Math.Atan2((double)(line2.From.Y - line2.To.Y), (double)(line2.From.X - line2.To.X)));
            angle = Math.Sign(angle) * Math.PI - angle;
            if (angle < 0) angle += 2 * Math.PI;
            return angle;
        }
    }
}
