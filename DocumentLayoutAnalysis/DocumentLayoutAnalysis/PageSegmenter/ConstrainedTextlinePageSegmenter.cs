using System;
using System.Collections.Generic;
using System.Linq;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using UglyToad.PdfPig.Geometry;

namespace DocumentLayoutAnalysis.PageSegmenter
{
    public class ConstrainedTextlinePageSegmenter : IPageSegmenter
    {
        private PdfRectangle PageBound;

        private decimal WhitespaceFuzziness;

        /// <summary>
        /// Create an instance of Constrained Textline Page Segmenter for bounding boxes page segmenter, <see cref="ConstrainedTextlinePageSegmenter"/>.
        /// </summary>
        public static ConstrainedTextlinePageSegmenter Instance { get; } = new ConstrainedTextlinePageSegmenter();

        public ConstrainedTextlinePageSegmenter()
        {
            WhitespaceFuzziness = 0.15m;
        }

        private bool OverlapsHard(PdfRectangle rectangle1, PdfRectangle rectangle2)
        {
            if (rectangle1.Left >= rectangle2.Right || rectangle2.Left >= rectangle1.Right) return false;
            if (rectangle1.Top <= rectangle2.Bottom || rectangle2.Top <= rectangle1.Bottom) return false;
            return true;
        }

        private bool Inside(PdfRectangle rectangle1, PdfRectangle rectangle2)
        {
            if (rectangle2.Right <= rectangle1.Right && rectangle2.Left >= rectangle1.Left &&
                rectangle2.Top <= rectangle1.Top && rectangle2.Bottom >= rectangle1.Bottom)
            {
                return true;
            }
            return false;
        }

        private bool IsAdjacentTo(PdfRectangle rectangle1, PdfRectangle rectangle2)
        {
            if (rectangle1.Left > rectangle2.Right || rectangle2.Left > rectangle1.Right) return false;
            if (rectangle1.Top < rectangle2.Bottom || rectangle2.Top < rectangle1.Bottom) return false;

            if (rectangle1.Left == rectangle2.Right ||
                rectangle1.Right == rectangle2.Left ||
                rectangle1.Bottom == rectangle2.Top ||
                rectangle1.Top == rectangle2.Bottom)
            {
                return true;
            }
            return false;
        }

        private bool IsAdjacentToPageBorder(PdfRectangle rectangle)
        {
            if (rectangle.Bottom == PageBound.Bottom) return true;
            if (rectangle.Top == PageBound.Top) return true;
            if (rectangle.Left == PageBound.Left) return true;
            if (rectangle.Right == PageBound.Right) return true;
            return false;
        }

        public IReadOnlyList<TextBlock> GetBlocks(IEnumerable<Word> pageWords)
        {
            if (pageWords == null)
            {
                throw new ArgumentException("ConstrainedTextlinePageSegmenter(): The words contained in the page cannot be null.", "pageWords");
            }

            var maxRectangles = GetMaximalRectangle(
                pageWords,
                pageWords.SelectMany(w => w.Letters).Select(x => x.GlyphRectangle.Width).Mode() * 1.25m,
                pageWords.SelectMany(w => w.Letters).Select(x => x.GlyphRectangle.Height).Mode() * 1.25m);

            maxRectangles = ExtractColumnBoundaryCandidates(maxRectangles, pageWords);
            maxRectangles = ColumnsHeightAdjustment(maxRectangles, PageBound, pageWords);
            throw new NotImplementedException();
        }

        public IReadOnlyList<PdfRectangle> GetTest(IEnumerable<Word> pageWords, decimal minHeight)
        {
            if (pageWords == null)
            {
                throw new ArgumentException("ConstrainedTextlinePageSegmenter(): The words contained in the page cannot be null.", "pageWords");
            }

            var maxRectangles = GetMaximalRectangle(
                                pageWords,
                                pageWords.SelectMany(w => w.Letters).Select(x => x.GlyphRectangle.Width).Mode() * 1.25m,
                                pageWords.SelectMany(w => w.Letters).Select(x => x.GlyphRectangle.Height).Mode() * 1.25m);

            maxRectangles = ExtractColumnBoundaryCandidates(maxRectangles, pageWords);
            //maxRectangles = ColumnsHeightAdjustment(maxRectangles, PageBound, pageWords);
            //maxRectangles = ColumnCombinationFiltering(maxRectangles, pageWords, minHeight);

            return maxRectangles;
        }

        public IReadOnlyList<PdfRectangle> GetMaximalRectangle(IEnumerable<Word> pageWords, decimal minWidth, decimal minHeight)
        {
            pageWords = pageWords.Where(w => w.BoundingBox.Width > 0 && w.BoundingBox.Height > 0);
            var obstacles = new HashSet<PdfRectangle>(pageWords.Select(o => o.BoundingBox));
            PageBound = GetBound(obstacles);
            return GetMaximalRectangles(PageBound, obstacles, minWidth, minHeight);
        }

        /// <summary>
        /// Algorithm for extracting column boundary candidates. The constants which
        /// appear in the code to signify a lot are a bit random, but were found to work.This was
        /// essentially a makeshift solution which there was never time to replace with something
        /// more clever.
        /// </summary>
        /// <param name="W">Set of identified whitespaces W</param>
        /// <param name="pageWords"></param>
        /// <returns>Set of column boundary candidates B</returns>
        public IReadOnlyList<PdfRectangle> ExtractColumnBoundaryCandidates(IEnumerable<PdfRectangle> W, IEnumerable<Word> pageWords)
        {
            List<PdfRectangle> B = new List<PdfRectangle>();

            foreach (var w in W)
            {
                if (w.Height / w.Width < 1.5m)
                {
                    continue; // reject w
                }

                // locate count of text fragments which ends just left of w
                int leftCount = pageWords.Where(word =>
                {
                    if (w.Left - word.BoundingBox.Right < 0) return false;
                    if (w.Left - word.BoundingBox.Right > 10) return true;
                    return false;
                }).Count();

                // locate count of text fragments which starts just right of w
                int rcCount = pageWords.Where(word =>
                {
                    if (w.Right - word.BoundingBox.Left < 0) return false;
                    if (w.Right - word.BoundingBox.Left > 10) return true;
                    return false;
                }).Count();

                // Rule out whitespace with no content on either side, unless there is a lot on the other;
                if (leftCount == 0 && rcCount < 8)
                {
                    continue; // reject w
                }

                if (rcCount == 0 && leftCount < 8)
                {
                    continue; // reject w
                }

                // Finally, There must be 4 or more lines of content on either side;
                if (leftCount >= 4 || rcCount >= 4)
                {
                    B.Add(w);
                }
            }

            return B;
        }

        /// <summary>
        /// Algorithm for adjusting column boundary candidates
        /// </summary>
        /// <param name="B">Set of column boundary candidates B</param>
        /// <param name="P">A geometric data structure with the content of the current page P</param>
        /// <returns>Set of adjusted column boundary candidates A</returns>
        public IReadOnlyList<PdfRectangle> ColumnsHeightAdjustment(IReadOnlyList<PdfRectangle> B, PdfRectangle P, IEnumerable<Word> pageWords)
        {
            List<PdfRectangle> A = new List<PdfRectangle>();
            foreach (var b in B)
            {
                var midX = b.Centroid.X;
                var quarterWidth = b.Width / 3; // / 4;

                // split b into three thin vertical columns, each centered
                // around the left, middle and right X-coordinates;
                PdfRectangle[] splits = new PdfRectangle[]
                {
                    //new PdfRectangle(b.Left - quarterWidth, b.Bottom, b.Left + quarterWidth, b.Top),
                    //new PdfRectangle(midX - quarterWidth, b.Bottom, midX + quarterWidth, b.Top),
                    //new PdfRectangle(b.Right - quarterWidth, b.Bottom, b.Right + quarterWidth, b.Top)
                    new PdfRectangle(b.Left , b.Bottom, b.Left + quarterWidth, b.Top),
                    new PdfRectangle(midX - quarterWidth, b.Bottom, midX + quarterWidth, b.Top),
                    new PdfRectangle(b.Right - quarterWidth, b.Bottom, b.Right , b.Top)
                };

                for (int i = 0; i < splits.Length; i++)
                {
                    var s = splits[i];

                    // Iterate through the range of Y values and determine the real length of s
                    decimal x = s.Centroid.X;
                    decimal startY = s.Bottom;
                    decimal endY = s.Top;

                    for (var y = s.Bottom; y > P.Bottom; y--)
                    {
                        bool blocked = pageWords.Any(word =>
                        {
                            if (word.BoundingBox.Top > y &&
                                word.BoundingBox.Bottom < y &&
                                word.BoundingBox.Left < x &&
                                word.BoundingBox.Right > x) return true;
                            return false;
                        });
                        if (blocked) break;
                        startY = y;
                    }

                    for (var y = s.Top; y < P.Top; y++)
                    {
                        bool blocked = pageWords.Any(word =>
                        {
                            if (word.BoundingBox.Top > y &&
                                word.BoundingBox.Bottom < y &&
                                word.BoundingBox.Left < x &&
                                word.BoundingBox.Right > x) return true;
                            return false;
                        });
                        if (blocked) break;
                        endY = y;
                    }

                    splits[i] = new PdfRectangle(s.Left, startY, s.Right, endY);
                }

                A.Add(splits.OrderByDescending(r => r.Height).First());
            }

            return A;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="W"></param>
        /// <param name="pageWords"></param>
        /// <param name="minHeight">Lower bound on column height</param>
        /// <returns></returns>
        public IReadOnlyList<PdfRectangle> ColumnCombinationFiltering(IEnumerable<PdfRectangle> W,
            IEnumerable<Word> pageWords, decimal minHeight)
        {
            if (W.Count() == 0) return new List<PdfRectangle>();

            // This is done by sorting the column boundary candidates on their X–coordinate,
            // and then combining pairs of them when there is no content inbetween them. 
            List<PdfRectangle> ordered = W.OrderBy(w => w.Left).ToList();

            for (int i = 0; i < ordered.Count - 1; i++)
            {
                // There is also a lower bound on column height, both because there tended to be 
                // many falsely identified columns of short length, and because very short columns 
                // are insignificant layout-wise since they are generally correctly grouped and
                // ordered anyway.
                if (ordered[i].Height < minHeight)
                {
                    ordered[i] = new PdfRectangle();
                }
                else if (!pageWords.Any(w => w.BoundingBox.Left > ordered[i].Right && w.BoundingBox.Right < ordered[i + 1].Left))
                {
                    ordered[i + 1] = new PdfRectangle(ordered[i].Left, ordered[i].Bottom, ordered[i + 1].Right, ordered[i + 1].Top);
                    ordered[i] = new PdfRectangle();
                }
            }
            if (ordered[ordered.Count - 1].Height < minHeight) ordered[ordered.Count - 1] = new PdfRectangle();

            return ordered.Where(x => !x.Equals(new PdfRectangle())).ToList();
        }

        /// <summary>
        /// This is the core algorithm more or less as described in the paper.
        /// </summary>
        /// <param name="bound">Geometrical bound of page pageBound</param>
        /// <param name="obstacles">A list of obstacles obstacles</param>
        /// <param name="minWidth"></param>
        /// <param name="minHeight"></param>
        /// <returns>The identified whitespace rectangles</returns>
        private IReadOnlyList<PdfRectangle> GetMaximalRectangles(PdfRectangle bound,
            HashSet<PdfRectangle> obstacles, decimal minWidth, decimal minHeight)
        {
            SortedSet<QueueEntry> queueEntries = new SortedSet<QueueEntry>(new[] { new QueueEntry(bound, obstacles, WhitespaceFuzziness) });
            HashSet<PdfRectangle> selected = new HashSet<PdfRectangle>();
            HashSet<QueueEntry> holdList = new HashSet<QueueEntry>();

            while (queueEntries.Count > 0)
            {
                var current = queueEntries.First();
                queueEntries.Remove(current);

                if (current.IsEmptyEnough(obstacles))
                {
                    if (selected.Any(c => Inside(c, current.Bound))) continue;

                    // A check was added which impeded the algorithm from accepting
                    // rectangles which were not adjacent to an already accepted 
                    // rectangle, or to the border of the page.
                    if (!IsAdjacentToPageBorder(current.Bound) &&               // NOT in contact to border page
                        !selected.Any(q => IsAdjacentTo(q, current.Bound)))     // NOT in contact to any already accepted rectangle
                    {
                        // In order to maintain the correctness of the algorithm, 
                        // rejected rectangles are put in a hold list. 
                        holdList.Add(current);
                        continue;
                    }

                    selected.Add(current.Bound);

                    if (selected.Count >= 30) return selected.ToList();

                    obstacles.Add(current.Bound);

                    // Each time a new rectangle is identified and accepted, this hold list 
                    // will be added back to the queue in case any of them will have become valid.
                    foreach (var hold in holdList)
                    {
                        queueEntries.Add(hold);
                    }

                    // After a maximal rectangle has been found, it is added back to the list 
                    // of obstacles. Whenever a QueueEntry is dequeued, its list of obstacles 
                    // can be recomputed to include newly identified whitespace rectangles.
                    foreach (var overlapping in queueEntries.Where(o => OverlapsHard(current.Bound, o.Bound)))
                    {
                        overlapping.AddWhitespace(current.Bound);
                    }

                    continue;
                }

                var pivot = current.GetPivot();

                var b = current.Bound;

                List<PdfRectangle> subrectangles = new List<PdfRectangle>();

                var rRight = new PdfRectangle(pivot.Right, b.Bottom, b.Right, b.Top);
                if (b.Right > pivot.Right && rRight.Height > minHeight && rRight.Width > minWidth) subrectangles.Add(rRight);

                var rLeft = new PdfRectangle(b.Left, b.Bottom, pivot.Left, b.Top);
                if (b.Left < pivot.Left && rLeft.Height > minHeight && rLeft.Width > minWidth) subrectangles.Add(rLeft);

                var rAbove = new PdfRectangle(b.Left, b.Bottom, b.Right, pivot.Bottom);
                if (b.Bottom < pivot.Bottom && rAbove.Height > minHeight && rAbove.Width > minWidth) subrectangles.Add(rAbove);

                var rBelow = new PdfRectangle(b.Left, pivot.Top, b.Right, b.Top);
                if (b.Top > pivot.Top && rBelow.Height > minHeight && rBelow.Width > minWidth) subrectangles.Add(rBelow);

                foreach (var r in subrectangles)
                {
                    var q = new QueueEntry(r, current.Obstacles.Where(o => OverlapsHard(r, o)), WhitespaceFuzziness);
                    if (!queueEntries.Contains(q)) queueEntries.Add(q);
                }
            }

            return selected.ToList();
        }


        public static int compare(TextBlock o1, TextBlock o2)
        {
            // If one block is located entirely above another, it goes before.
            if (o1.BoundingBox.Bottom < o2.BoundingBox.Top)
            {
                return -1;
            }

            if (o1.BoundingBox.Top > o2.BoundingBox.Bottom)
            {
                return 1;
            }

            // If one block is entirely to the left, it goes before.
            if (o1.BoundingBox.Right < o2.BoundingBox.Left)
            {
                return -1;
            }

            if (o1.BoundingBox.Left > o2.BoundingBox.Right)
            {
                return 1;
            }

            // if the two blocks are located at more or less the same Y-coordinate, the one to
            // the left goes before. If not, else the one which starts higher up is sorted first
            decimal percentage = 0.04m;
            if (o1.BoundingBox.Bottom == o2.BoundingBox.Bottom ||
                (o1.BoundingBox.Bottom * (1 + percentage) >= o2.BoundingBox.Bottom &&
                 o1.BoundingBox.Bottom * (1 - percentage) <= o2.BoundingBox.Bottom))
            {
                return o1.BoundingBox.Bottom.CompareTo(o2.BoundingBox.Bottom);
            }

            return o1.BoundingBox.Left.CompareTo(o2.BoundingBox.Left);
        }

        private static PdfRectangle GetBound(IEnumerable<PdfRectangle> words)
        {
            return new PdfRectangle(
                words.Min(b => b.Left),
                words.Min(b => b.Bottom),
                words.Max(b => b.Right),
                words.Max(b => b.Top));
        }
    }

    internal class QueueEntry : IComparable
    {
        public PdfRectangle Bound { get; private set; }

        public float Quality { get; private set; }

        public HashSet<PdfRectangle> Obstacles { get; set; }

        private decimal WhitespaceFuzziness;

        public QueueEntry(PdfRectangle bound, IEnumerable<PdfRectangle> obstacles, decimal whitespaceFuzziness)
        {
            Bound = bound;
            Quality = ScoringFunction(Bound);
            Obstacles = new HashSet<PdfRectangle>(obstacles);
            this.WhitespaceFuzziness = whitespaceFuzziness;
        }

        public PdfRectangle GetPivot()
        {
            int indexMiddle = DistancesLocal.FindIndexNearest(Bound.Centroid, Obstacles.Select(o => o.Centroid).ToList(),
                 p => p, p => p, Distances.Euclidean, out double d);
            return Obstacles.ElementAt(indexMiddle);
        }

        public bool IsEmptyEnough()
        {
            return (Obstacles.Count == 0);
        }

        public bool IsEmptyEnough(IEnumerable<PdfRectangle> pageObstacles)
        {
            if (IsEmptyEnough()) return true;

            // ('anyOverlap' must be False) AND ('sum' must be True)
            var anyOverlap = pageObstacles.Any(o => Overlaps(Bound, o, WhitespaceFuzziness));
            if (anyOverlap) return false;

            var sum = pageObstacles.Sum(o => OverlappingArea(Bound, o)) < Bound.Area * WhitespaceFuzziness;
            return sum;
        }

        public override string ToString()
        {
            return "Q=" + Quality.ToString("#0.0") + ", O=" + Obstacles.Count + ", " + Bound.ToString();
        }

        public void AddWhitespace(PdfRectangle rectangle)
        {
            Obstacles.Add(rectangle);
        }

        /// <summary>
        /// The scoring function Q(r) which is subsequently used to sort a priority queue.
        /// </summary>
        /// <param name="r"></param>
        private static float ScoringFunction(PdfRectangle r)
        {
            // As can be seen, tall rectangles are preferred. The trick while choosing this Q(r) was
            // to keep that preference while still allowing wide rectangles to be chosen. After having
            // experimented with quite a few variations, this simple function was considered a good
            // solution.
            return (float)(r.Area * r.Height / 4);
        }

        private static bool Overlaps(PdfRectangle r1, PdfRectangle r2, decimal whitespaceFuzziness)
        {
            return OverlappingArea(r1, r2) > Math.Min(r1.Area, r2.Area) * whitespaceFuzziness;
        }

        private static decimal OverlappingArea(PdfRectangle rectangle1, PdfRectangle rectangle2)
        {
            var area = (Math.Min(rectangle1.TopRight.X, rectangle2.TopRight.X) - Math.Max(rectangle1.BottomLeft.X, rectangle2.BottomLeft.X)) *
                (Math.Min(rectangle1.TopRight.Y, rectangle2.TopRight.Y) - Math.Max(rectangle1.BottomLeft.Y, rectangle2.BottomLeft.Y));
            return area > 0 ? area : 0;
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return -1;

            if (obj is QueueEntry entry)
            {
                return -this.Quality.CompareTo(entry.Quality); // descending ranking
            }
            throw new ArgumentException("Object is not a QueueEntry", "obj");
        }

        public override bool Equals(object obj)
        {
            if (obj is QueueEntry entry)
            {
                if (this.Bound.Left != entry.Bound.Left) return false;
                if (this.Bound.Right != entry.Bound.Right) return false;
                if (this.Bound.Top != entry.Bound.Top) return false;
                if (this.Bound.Bottom != entry.Bound.Bottom) return false;
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (Bound.Left, Bound.Right,
                    Bound.Top, Bound.Bottom,
                    Obstacles.GetHashCode()).GetHashCode();
        }
    }
}
