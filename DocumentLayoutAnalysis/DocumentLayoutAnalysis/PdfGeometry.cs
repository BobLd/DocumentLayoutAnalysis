using System;
using System.Collections.Generic;
using System.Linq;
using UglyToad.PdfPig.Geometry;
using static UglyToad.PdfPig.Geometry.PdfPath;

namespace DocumentLayoutAnalysis
{
    public class PdfGeometry
    {
        private double _shoeLaceSum;

        public IReadOnlyList<IPathCommand> Commands { get; private set; }
        public List<PdfGeometry> SubGeometries { get; }
        public PdfPoint Centroid => ComputeCentroid(Commands);

        internal PdfGeometry(IReadOnlyList<IPathCommand> commands)
        {
            Commands = commands;
            _shoeLaceSum = double.NaN;
        }

        public PdfGeometry(PdfPath path) : this(path.Commands)
        {
            SubGeometries = new List<PdfGeometry>();
            List<IPathCommand> subCommands = new List<IPathCommand>();

            foreach (var command in Commands)
            {
                if (subCommands.Count == 0)
                {
                    subCommands.Add(command);
                    continue;
                }

                if (command is Close close)
                {
                    subCommands.Add(command);
                    SubGeometries.Add(new PdfGeometry(subCommands));
                    subCommands = new List<IPathCommand>();
                }
                else if (command is Move move)
                {
                    if (move.Location.Equals(GetEndPoint(subCommands.Last())))
                    {
                        subCommands.Add(command);
                    }
                    else
                    {
                        SubGeometries.Add(new PdfGeometry(subCommands));
                        subCommands = new List<IPathCommand>();
                        subCommands.Add(command);
                    }
                }
                else if (command is Line line)
                {
                    if (line.From.Equals(GetEndPoint(subCommands.Last())))
                    {
                        subCommands.Add(command);
                    }
                    else
                    {
                        SubGeometries.Add(new PdfGeometry(subCommands));
                        subCommands = new List<IPathCommand>();
                        subCommands.Add(command);
                    }
                }
                else if (command is BezierCurve curve)
                {
                    if (curve.StartPoint.Equals(GetEndPoint(subCommands.Last())))
                    {
                        subCommands.Add(command);
                    }
                    else
                    {
                        SubGeometries.Add(new PdfGeometry(subCommands));
                        subCommands = new List<IPathCommand>();
                        subCommands.Add(command);
                    }
                }
            }
            if (subCommands.Count > 0) SubGeometries.Add(new PdfGeometry(subCommands));

            foreach (var subGeo in SubGeometries)
            {
                if (subGeo.Commands.Any(c => c is Close))
                {
                    var filtered = subGeo.Commands.Where(c => c is Line || c is BezierCurve || c is Move);
                    // check if first element and last element are connected
                    if (!GetStartPoint(filtered.First()).Equals(GetEndPoint(filtered.Last())))
                    {
                        // if not, force close
                        var commands = subGeo.Commands.ToList();
                        commands.Add(new Line(GetEndPoint(filtered.Last()), GetStartPoint(filtered.First())));
                        subGeo.Commands = commands;
                    }
                }
            }

            this.Commands = this.SubGeometries.SelectMany(g => g.Commands).ToList();
        }

        public PdfRectangle? GetBoundingRectangle()
        {
            if (Commands.Count == 0) return null;

            var minX = decimal.MaxValue;
            var maxX = decimal.MinValue;

            var minY = decimal.MaxValue;
            var maxY = decimal.MinValue;

            foreach (var command in Commands)
            {
                var rect = command.GetBoundingRectangle();
                if (rect == null && command is Move move)
                {
                    rect = new PdfRectangle(move.Location.X, move.Location.Y,
                                            move.Location.X, move.Location.Y);
                }

                if (rect == null) continue;

                if (rect.Value.Width < 0 || rect.Value.Height < 0)
                {
                    Console.WriteLine();
                }

                if (rect.Value.Left < minX)
                {
                    minX = rect.Value.Left;
                }

                if (rect.Value.Right > maxX)
                {
                    maxX = rect.Value.Right;
                }

                if (rect.Value.Bottom < minY)
                {
                    minY = rect.Value.Bottom;
                }

                if (rect.Value.Top > maxY)
                {
                    maxY = rect.Value.Top;
                }
            }

            if (minX == decimal.MaxValue ||
                maxX == decimal.MinValue ||
                minY == decimal.MaxValue ||
                maxY == decimal.MinValue)
            {
                return null;
            }

            if (minX > maxX || minY > maxY)
            {
                Console.WriteLine();
            }

            return new PdfRectangle(minX, minY, maxX, maxY);
        }

        internal bool IsVerticalLine()
        {
            if (Commands.Count == 0) return false;
            if (Commands.Any(c => c is BezierCurve)) return false;

            var filtered = Commands.Where(c => c is Line).ToList();
            if (filtered.Count == 0) return false;
            foreach (var line in filtered)
            {
                if (GetStartPoint(line).X != GetEndPoint(line).X) return false;
            }

            return true;

            //if (filtered.Count > 1) return false;
            //var line = filtered[0] as Line;
            //if (line.From.X == line.To.X) return true;
            //return false;
        }

        internal bool IsHorizontalLine()
        {
            if (Commands.Count == 0) return false;
            if (Commands.Any(c => c is BezierCurve)) return false;

            var filtered = Commands.Where(c => c is Line).ToList();
            if (filtered.Count == 0) return false;

            foreach (var line in filtered)
            {
                if (GetStartPoint(line).Y != GetEndPoint(line).Y) return false;
            }

            return true;
            //if (filtered.Count > 1) return false;
            //var line = filtered[0] as Line;
            //if (line.From.Y == line.To.Y) return true;
            //return false;
        }

        public bool IsClosed
        {
            get
            {
                if (Commands.Any(c => c is Close)) return true;
                var filtered = Commands.Where(c => c is Line || c is BezierCurve).ToList();
                if (filtered.Count < 2) return false;

                if (!GetStartPoint(filtered.First()).Equals(GetEndPoint(filtered.Last()))) return false;

                PdfPoint previous = GetEndPoint(filtered[0]);

                for (int i = 1; i < filtered.Count; i++)
                {
                    if (!GetStartPoint(filtered[i]).Equals(previous)) return false;
                    previous = GetEndPoint(filtered[i]);
                }
                return true;
            }
        }

        public bool IsClockwise
        {
            get
            {
                if (!IsClosed) return false;
                if (double.IsNaN(_shoeLaceSum)) GetShoeLaceSum();
                return _shoeLaceSum > 0;
            }
        }

        public bool IsCounterClockwise
        {
            get
            {
                if (!IsClosed) return false;
                if (double.IsNaN(_shoeLaceSum)) GetShoeLaceSum();
                return _shoeLaceSum < 0;
            }
        }

        public void RevertOrder()
        {
            var moves = Commands.Where(c => c is Move).ToList();
            if (moves.Count > 1) throw new ArgumentException();

            var closes = Commands.Where(c => c is Close).ToList();
            if (closes.Count > 1) throw new ArgumentException();

            var others = Commands.Where(c => c is Line || c is BezierCurve).ToList();
            others.Reverse();

            for (int i = 0; i < others.Count; i++)
            {
                var command = others[i];
                if (command is Line line)
                {
                    others[i] = new Line(line.To, line.From);
                }
                else if (command is BezierCurve curve)
                {
                    others[i] = new BezierCurve(curve.EndPoint,
                                                 curve.SecondControlPoint,
                                                 curve.FirstControlPoint,
                                                 curve.StartPoint);
                }
            }

            if (moves.Count > 0 && others.Count > 0)
            {
                Move move = moves.First() as Move;
                var other = others.First();
                if (!GetEndPoint(move).Equals(GetStartPoint(other)))
                {
                    moves[0] = new Move(GetStartPoint(other));
                }
            }

            moves.AddRange(others);
            moves.AddRange(closes);

            Commands = moves;
        }

        public void OrderClockwise()
        {
            if (IsClockwise) return; // nothing to do
            RevertOrder();
        }

        public void OrderAntiClockwise()
        {
            if (IsCounterClockwise) return; // nothing to do
            RevertOrder();
        }

        public static PdfPoint ComputeCentroid(IReadOnlyList<IPathCommand> commands)
        {
            var filtered = commands.Where(c => c is Line || c is BezierCurve);
            if (filtered.Count() == 0) return new PdfPoint();
            var points = filtered.Select(c => GetStartPoint(c)).ToList();
            points.AddRange(filtered.Select(c => GetEndPoint(c)));
            return new PdfPoint(points.Average(p => p.X), points.Average(p => p.Y));
        }

        /// <summary>
        /// Returns a list of points from the curve: start point, end point and approximated intermediate points.
        /// </summary>
        /// <param name="curve"></param>
        /// <returns></returns>
        public static List<Line> BezierCurveToPaths(BezierCurve curve)
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

            lines.Add(new Line(curve.StartPoint, point20));
            lines.Add(new Line(point20, point40));
            lines.Add(new Line(point40, point60));
            lines.Add(new Line(point60, point80));
            lines.Add(new Line(point80, curve.EndPoint));
            return lines;
        }

        private void GetShoeLaceSum()
        {
            // https://stackoverflow.com/questions/1165647/how-to-determine-if-a-list-of-polygon-points-are-in-clockwise-order
            if (!IsClosed)
            {
                _shoeLaceSum = double.NaN;
                return;
            }

            double sum = 0;
            foreach (var command in Commands)
            {
                if (command is Line line)
                {
                    sum += (double)((line.To.X - line.From.X) * (line.To.Y + line.From.Y));
                }
                else if (command is BezierCurve curve)
                {
                    var lines = BezierCurveToPaths(curve);
                    foreach (var lineB in lines)
                    {
                        sum += (double)((lineB.To.X - lineB.From.X) * (lineB.To.Y + lineB.From.Y));
                    }
                }
            }
            _shoeLaceSum = sum;
        }

        private static PdfPoint GetStartPoint(IPathCommand command)
        {
            if (command is Line line)
            {
                return line.From;
            }
            else if (command is BezierCurve curve)
            {
                return curve.StartPoint;
            }
            else if (command is Move move)
            {
                return move.Location;
            }
            else
            {
                throw new ArgumentException();
            }
        }

        private static PdfPoint GetEndPoint(IPathCommand command)
        {
            if (command is Line line)
            {
                return line.To;
            }
            else if (command is BezierCurve curve)
            {
                return curve.EndPoint;
            }
            else if (command is Move move)
            {
                return move.Location;
            }
            else
            {
                throw new ArgumentException();
            }
        }
    }
}
