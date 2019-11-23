using Accord.Controls;
using DocumentLayoutAnalysis.StraightSkeleton;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UglyToad.PdfPig.Geometry;
using static UglyToad.PdfPig.Geometry.PdfPath;

namespace DocumentLayoutAnalysis
{
    class VertexTest
    {
        public static void Run(string path)
        {
            ScatterplotView view = new ScatterplotView();
            view.Dock = System.Windows.Forms.DockStyle.Fill;
            view.LinesVisible = true;

            var outerLimit = new PdfPoint[]
            {
                new PdfPoint(30,   100),
                new PdfPoint(50,   200),
                new PdfPoint(220,  240),
                new PdfPoint(440,  240),
                new PdfPoint(430,  40),
                new PdfPoint(230,  30),
                new PdfPoint(85,   40)
            };

            List<Vertex> vertices = new List<Vertex>();
            List<PdfPoint> intersects = new List<PdfPoint>();

            for (int i = 0; i < outerLimit.Count() - 2; i++)
            {
                var vertex = new Vertex(new Line(outerLimit[i], outerLimit[i + 1]), new Line(outerLimit[i + 1], outerLimit[i + 2]));
                DrawVertex(vertex, view, i);
                vertices.Add(vertex);
                if (vertices.Count > 1)
                {
                    var intersection = vertex.AngleBisectorsInterscAt(vertices[i - 1]);
                    if (intersection != null) intersects.Add((PdfPoint)intersection);
                }

                if (i == outerLimit.Count() - 3)
                {
                    var vertexFinal = new Vertex(new Line(outerLimit[i + 1], outerLimit[i + 2]), new Line(outerLimit[i + 2], outerLimit.First()));
                    DrawVertex(vertexFinal, view, i + 1);
                    vertices.Add(vertexFinal);
                    var intersection = vertex.AngleBisectorsInterscAt(vertices[i - 1]);
                    if (intersection != null) intersects.Add((PdfPoint)intersection);

                    vertexFinal = new Vertex(new Line(outerLimit[i + 2], outerLimit.First()), new Line(outerLimit.First(), outerLimit[1]));
                    DrawVertex(vertexFinal, view, i + 2);
                    vertices.Add(vertexFinal);
                    var intersection2 = vertex.AngleBisectorsInterscAt(vertices[i - 1]);
                    if (intersection2 != null) intersects.Add((PdfPoint)intersection2);
                    var intersection3 = vertexFinal.AngleBisectorsInterscAt(vertices.First());
                    if (intersection3 != null) intersects.Add((PdfPoint)intersection3);
                }
            }

            List<Tuple<Vertex, PdfPoint, double>> queue = new List<Tuple<Vertex, PdfPoint, double>>();

            for (int v = 1; v < vertices.Count - 1; v++) // one is missing
            {
                var previous = vertices[v - 1];
                var current = vertices[v];
                var next = vertices[v + 1];

                var i1 = current.AngleBisectorsInterscAt(previous);
                if (i1.HasValue)
                {
                    var dist1 = Vertex.DistancePointToLine(current.LeftEdge, i1.Value);
                    queue.Add(new Tuple<Vertex, PdfPoint, double>(current, i1.Value, dist1));
                }

                var i2 = current.AngleBisectorsInterscAt(next);
                if (i2.HasValue)
                {
                    var dist2 = Vertex.DistancePointToLine(current.LeftEdge, i2.Value);
                    queue.Add(new Tuple<Vertex, PdfPoint, double>(current, i2.Value, dist2));
                }
            }

            queue = queue.OrderBy(t => t.Item3).ToList();

            foreach (var pop in queue)
            {
    
            }

            foreach (var point in intersects)
            {
                view.Graph.GraphPane.AddCurve("intersect",
                    new double[] { (double)point.X },
                    new double[] { (double)point.Y },
                    Color.DarkBlue,
                    ZedGraph.SymbolType.TriangleDown);
            }

            var hole = new PdfPoint[]
            {
                new PdfPoint(175,  85),
                new PdfPoint(245,  140),
                new PdfPoint(315,  90),
                new PdfPoint(385,  160),
                new PdfPoint(330,  200),
                new PdfPoint(165,  180)
            };

            for (int i = 0; i < hole.Count() - 2; i++)
            {
                var vertex = new Vertex(new Line(hole[i], hole[i + 1]), new Line(hole[i + 1], hole[i + 2]));
                DrawVertex(vertex, view, i);

                if (i == hole.Count() - 3)
                {
                    var vertexFinal = new Vertex(new Line(hole[i + 1], hole[i + 2]), new Line(hole[i + 2], hole.First()));
                    DrawVertex(vertexFinal, view, i + 1);

                    var vertexFinal1 = new Vertex(new Line(hole[i + 2], hole.First()), new Line(hole.First(), hole[1]));
                    DrawVertex(vertexFinal1, view, i + 2);

                    var cross = vertexFinal.AngleBisectorsInterscAt(vertexFinal1);
                }
            }
            
            view.Graph.GraphPane.AxisChange();
            var f1 = new System.Windows.Forms.Form();
            f1.Width = 600;
            f1.Height = 400;
            f1.Controls.Add(view);
            f1.ShowDialog();
        }

        static ZedGraph.SymbolType[] symbols = new ZedGraph.SymbolType[]
        {
             ZedGraph.SymbolType.Circle,
              ZedGraph.SymbolType.Diamond,
              ZedGraph.SymbolType.HDash,
              ZedGraph.SymbolType.Plus,
              ZedGraph.SymbolType.Square,
              ZedGraph.SymbolType.Star,
              ZedGraph.SymbolType.Triangle,
              ZedGraph.SymbolType.TriangleDown,
              ZedGraph.SymbolType.VDash,
              ZedGraph.SymbolType.XCross
        };

        static void DrawVertex(Vertex vertex, ScatterplotView view, int i)
        {
            view.Graph.GraphPane.AddCurve("left_" + i,
                new double[] { (double)vertex.LeftEdge.From.X, (double)vertex.LeftEdge.To.X },
                new double[] { (double)vertex.LeftEdge.From.Y, (double)vertex.LeftEdge.To.Y },
                Color.Blue,
                symbols[i]);
           
            view.Graph.GraphPane.AddCurve("rigth_" + i,
                new double[] { (double)vertex.RightEdge.From.X, (double)vertex.RightEdge.To.X },
                new double[] { (double)vertex.RightEdge.From.Y, (double)vertex.RightEdge.To.Y },
                Color.Green,
                symbols[i]);

            view.Graph.GraphPane.AddCurve("bisector_" + i,
                new double[] { (double)vertex.AngleBisectorRay.From.X, (double)vertex.AngleBisectorRay.To.X },
                new double[] { (double)vertex.AngleBisectorRay.From.Y, (double)vertex.AngleBisectorRay.To.Y },
                Color.Red,
                symbols[i]);
        }

        /// <summary>
        /// Returns a list of points from the curve: start point, end point and approximated intermediate points.
        /// </summary>
        /// <param name="curve"></param>
        /// <returns></returns>
        internal static List<PdfPath.Line> BezierCurveToPaths(BezierCurve curve)
        {
            // https://ocw.mit.edu/courses/electrical-engineering-and-computer-science/6-837-computer-graphics-fall-2012/lecture-notes/MIT6_837F12_Lec01.pdf
            // https://en.wikipedia.org/wiki/B%C3%A9zier_curve

            List<Line> lines = new List<Line>();

            Func<BezierCurve, double, PdfPoint> P_t = (bezierCurve, t) => // with 0 <= t <= 1
            {
                var x = (1 - t) * (1 - t) * (1 - t) * (double)bezierCurve.StartPoint.X +
                        3 * t * (1 - t) * (1 - t) * (double)bezierCurve.FirstControlPoint.X +
                        3 * t * t * (1 - t) * (double)bezierCurve.SecondControlPoint.X +
                        t * t * t * (double)bezierCurve.EndPoint.X;

                var y = (1 - t) * (1 - t) * (1 - t) * (double)bezierCurve.StartPoint.Y +
                        3 * t * (1 - t) * (1 - t) * (double)bezierCurve.FirstControlPoint.Y +
                        3 * t * t * (1 - t) * (double)bezierCurve.SecondControlPoint.Y +
                        t * t * t * (double)bezierCurve.EndPoint.Y;

                return new PdfPoint(x, y);
            };

            var point20 = P_t(curve, 0.20);
            var point40 = P_t(curve, 0.40);
            var point60 = P_t(curve, 0.60);
            var point80 = P_t(curve, 0.80);

            lines.Add(new PdfPath.Line(curve.StartPoint, point20));
            lines.Add(new PdfPath.Line(point20, point40));
            lines.Add(new PdfPath.Line(point40, point60));
            lines.Add(new PdfPath.Line(point60, point80));
            lines.Add(new PdfPath.Line(point80, curve.EndPoint));
            return lines;
        }
    }
}
