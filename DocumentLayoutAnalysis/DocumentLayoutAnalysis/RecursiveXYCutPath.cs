using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using UglyToad.PdfPig.Geometry;
using static UglyToad.PdfPig.Geometry.PdfPath;

namespace DocumentLayoutAnalysis
{
    /// <summary>
    /// The recursive X-Y cut is a top-down page segmentation technique that decomposes a document 
    /// recursively into a set of rectangular blocks. This implementation leverages bounding boxes.
    /// https://en.wikipedia.org/wiki/Recursive_X-Y_cut
    /// <para>See 'Recursive X-Y Cut using Bounding Boxes of Connected Components' by Jaekyu Ha, Robert M.Haralick and Ihsin T. Phillips</para>
    /// </summary>
    public class RecursiveXYCutPath
    {
        /// <summary>
        /// Create an instance of Recursive X-Y Cut page segmenter, <see cref="RecursiveXYCut"/>.
        /// </summary>
        public static RecursiveXYCutPath Instance { get; } = new RecursiveXYCutPath();

        /// <summary>
        /// Get the blocks.
        /// </summary>
        /// <param name="pagePaths">The words in the page.</param>
        /// <param name="minimumWidth">The minimum width for a block.</param>
        /// <param name="dominantFontWidthFunc">The function that determines the dominant font width.</param>
        /// <param name="dominantFontHeightFunc">The function that determines the dominant font height.</param>
        public IReadOnlyList<PdfRectangle> GetBlocks(IEnumerable<PdfPath> pagePaths, decimal minimumWidth,
            decimal dominantFontWidth, decimal dominantFontHeight)
        {
            pagePaths = pagePaths.Where(p => p != null && p.Commands.Count > 0); // clean paths
            XYLeafP root = new XYLeafP(pagePaths); // Create a root node.
            XYNodeP node = VerticalCut(root, minimumWidth, dominantFontWidth, dominantFontHeight);

            var leafs = node.GetLeafs();

            if (leafs.Count > 0)
            {
                return leafs.Select(l => l.BoundingBox).ToList(); // new TextBlock(l.GetLines())).ToList();
            }

            return new List<PdfRectangle>();
        }

        private XYNodeP VerticalCut(XYLeafP leaf, decimal minimumWidth,
            decimal dominantFontWidth, decimal dominantFontHeight, int level = 0)
        {
            if (leaf.CountWords() <= 1 || leaf.BoundingBox.Width <= minimumWidth)
            {
                // we stop cutting if 
                // - only one word remains
                // - width is too small
                return leaf;
            }

            // order words left to right
            var paths = leaf.Paths.OrderBy(w => w.BoundingBox().Left).ToArray();

            // determine dominantFontWidth and dominantFontHeight
            //decimal domFontWidth = dominantFontWidthFunc(paths.SelectMany(x => x.Letters)
            //    .Select(x => Math.Abs(x.GlyphRectangle.Width)));
            //decimal domFontHeight = dominantFontHeightFunc(paths.SelectMany(x => x.Letters)
            //    .Select(x => Math.Abs(x.GlyphRectangle.Height)));

            List<decimal[]> projectionProfile = new List<decimal[]>();
            decimal[] currentProj = new decimal[2] { paths[0].BoundingBox().Left, paths[0].BoundingBox().Right };
            int wordsCount = paths.Count();
            for (int i = 1; i < wordsCount; i++)
            {
                if ((paths[i].BoundingBox().Left >= currentProj[0] && paths[i].BoundingBox().Left <= currentProj[1])
                    || (paths[i].BoundingBox().Right >= currentProj[0] && paths[i].BoundingBox().Right <= currentProj[1]))
                {
                    // it is overlapping 
                    if (paths[i].BoundingBox().Left >= currentProj[0]
                        && paths[i].BoundingBox().Left <= currentProj[1]
                        && paths[i].BoundingBox().Right > currentProj[1])
                    {
                        // |____|
                        //    |____|
                        // |_______|    <- updated
                        currentProj[1] = paths[i].BoundingBox().Right;
                    }

                    // we ignore the following cases:
                    //    |____|
                    // |____|          (not possible because of OrderBy)
                    // 
                    //    |____|
                    //|___________|    (not possible because of OrderBy)
                    //
                    //  |____|
                    //   |_|
                }
                else
                {
                    // no overlap
                    if (paths[i].BoundingBox().Left - currentProj[1] <= dominantFontWidth)
                    {
                        // if gap too small -> don't cut
                        // |____| |____|
                        currentProj[1] = paths[i].BoundingBox().Right;
                    }
                    else if (currentProj[1] - currentProj[0] < minimumWidth)
                    {
                        // still too small
                        currentProj[1] = paths[i].BoundingBox().Right;
                    }
                    else
                    {
                        // if gap big enough -> cut!
                        // |____|   |   |____|
                        if (i != wordsCount - 1) // will always add the last one after
                        {
                            projectionProfile.Add(currentProj);
                            currentProj = new decimal[2] { paths[i].BoundingBox().Left, paths[i].BoundingBox().Right };
                        }
                    }
                }
                if (i == wordsCount - 1) projectionProfile.Add(currentProj);
            }

            var newLeafsEnums = projectionProfile
                .Select(p => leaf.Paths.Where(w => w.BoundingBox().Left >= p[0] && w.BoundingBox().Right <= p[1]));
            var newLeafs = newLeafsEnums.Where(e => e.Count() > 0).Select(e => new XYLeafP(e));

            var newNodes = newLeafs.Select(l => HorizontalCut(l, minimumWidth,
                dominantFontWidth, dominantFontHeight, level)).ToList();

            var lost = leaf.Paths.Except(newLeafsEnums.SelectMany(x => x)).ToList();
            if (lost.Count > 0)
            {
                newNodes.AddRange(lost.Select(w => new XYLeafP(w)));
            }

            return new XYNodeP(newNodes);
        }

        private XYNodeP HorizontalCut(XYLeafP leaf, decimal minimumWidth,
            decimal dominantFontWidth, decimal dominantFontHeight, int level = 0)
        {
            if (leaf.CountWords() <= 1)
            {
                // we stop cutting if 
                // - only one word remains
                return leaf;
            }

            var words = leaf.Paths.OrderBy(w => w.BoundingBox().Bottom).ToArray(); // order bottom to top

            // determine dominantFontWidth and dominantFontHeight
            //decimal dominantFontWidth = dominantFontWidthFunc(words.SelectMany(x => x.Letters)
            //    .Select(x => Math.Abs(x.GlyphRectangle.Width)));
            //decimal dominantFontHeight = dominantFontHeightFunc(words.SelectMany(x => x.Letters)
            //    .Select(x => Math.Abs(x.GlyphRectangle.Height)));

            List<decimal[]> projectionProfile = new List<decimal[]>();
            decimal[] currentProj = new decimal[2] { words[0].BoundingBox().Bottom, words[0].BoundingBox().Top };
            int wordsCount = words.Count();
            for (int i = 1; i < wordsCount; i++)
            {
                if ((words[i].BoundingBox().Bottom >= currentProj[0] && words[i].BoundingBox().Bottom <= currentProj[1])
                    || (words[i].BoundingBox().Top >= currentProj[0] && words[i].BoundingBox().Top <= currentProj[1]))
                {
                    // it is overlapping 
                    if (words[i].BoundingBox().Bottom >= currentProj[0]
                        && words[i].BoundingBox().Bottom <= currentProj[1]
                        && words[i].BoundingBox().Top > currentProj[1])
                    {
                        currentProj[1] = words[i].BoundingBox().Top;
                    }
                }
                else
                {
                    // no overlap
                    if (words[i].BoundingBox().Bottom - currentProj[1] <= dominantFontHeight)
                    {
                        // if gap too small -> don't cut
                        // |____| |____|
                        currentProj[1] = words[i].BoundingBox().Top;
                    }
                    else
                    {
                        // if gap big enough -> cut!
                        // |____|   |   |____|
                        if (i != wordsCount - 1) // will always add the last one after
                        {
                            projectionProfile.Add(currentProj);
                            currentProj = new decimal[2] { words[i].BoundingBox().Bottom, words[i].BoundingBox().Top };
                        }
                    }
                }
                if (i == wordsCount - 1) projectionProfile.Add(currentProj);
            }

            if (projectionProfile.Count == 1)
            {
                if (level >= 1)
                {
                    return leaf;
                }
                else
                {
                    level++;
                }
            }

            var newLeafsEnums = projectionProfile.Select(p =>
                leaf.Paths.Where(w => w.BoundingBox().Bottom >= p[0] && w.BoundingBox().Top <= p[1]));
            var newLeafs = newLeafsEnums.Where(e => e.Count() > 0).Select(e => new XYLeafP(e));
            var newNodes = newLeafs.Select(l => VerticalCut(l, minimumWidth,
                dominantFontWidth, dominantFontHeight, level)).ToList();

            var lost = leaf.Paths.Except(newLeafsEnums.SelectMany(x => x)).ToList();
            if (lost.Count > 0)
            {
                newNodes.AddRange(lost.Select(w => new XYLeafP(w)));
            }
            return new XYNodeP(newNodes);
        }
    }

    /// <summary>
    /// A Leaf node used in the <see cref="RecursiveXYCut"/> algorithm, i.e. a block.
    /// </summary>
    internal class XYLeafP : XYNodeP
    {
        /// <summary>
        /// Returns true if this node is a leaf, false otherwise.
        /// </summary>
        public override bool IsLeaf => true;

        /// <summary>
        /// The words in the leaf.
        /// </summary>
        public IReadOnlyList<PdfPath> Paths { get; }

        /// <summary>
        /// The number of words in the leaf.
        /// </summary>
        public override int CountWords() => Paths == null ? 0 : Paths.Count;

        /// <summary>
        /// Returns null as a leaf doesn't have leafs.
        /// </summary>
        public override List<XYLeafP> GetLeafs()
        {
            return null;
        }

        /// <summary>
        /// Create a new <see cref="XYLeafP"/>.
        /// </summary>
        /// <param name="paths">The words contained in the leaf.</param>
        public XYLeafP(params PdfPath[] paths) : this(paths == null ? null : paths.ToList())
        {

        }

        /// <summary>
        /// Create a new <see cref="XYLeafP"/>.
        /// </summary>
        /// <param name="paths">The words contained in the leaf.</param>
        public XYLeafP(IEnumerable<PdfPath> paths) : base(null)
        {
            if (paths == null)
            {
                throw new ArgumentException("XYLeaf(): The words contained in the leaf cannot be null.", "words");
            }

            decimal left = paths.Min(b => b.BoundingBox().Left);
            decimal right = paths.Max(b => b.BoundingBox().Right);

            decimal bottom = paths.Min(b => b.BoundingBox().Bottom);
            decimal top = paths.Max(b => b.BoundingBox().Top);

            BoundingBox = new PdfRectangle(left, bottom, right, top);
            Paths = paths.ToArray();
        }
    }

    /// <summary>
    /// A Node used in the <see cref="RecursiveXYCut"/> algorithm.
    /// </summary>
    internal class XYNodeP
    {
        /// <summary>
        /// Returns true if this node is a leaf, false otherwise.
        /// </summary>
        public virtual bool IsLeaf => false;

        /// <summary>
        /// The rectangle completely containing the node.
        /// </summary>
        public PdfRectangle BoundingBox { get; set; }

        /// <summary>
        /// The children of the node.
        /// </summary>
        public XYNodeP[] Children { get; set; }

        /// <summary>
        /// Create a new <see cref="XYNodeP"/>.
        /// </summary>
        /// <param name="children">The node's children.</param>
        public XYNodeP(params XYNodeP[] children)
            : this(children?.ToList())
        {

        }

        /// <summary>
        /// Create a new <see cref="XYNodeP"/>.
        /// </summary>
        /// <param name="children">The node's children.</param>
        public XYNodeP(IEnumerable<XYNodeP> children)
        {
            if (children != null && children.Count() != 0)
            {
                Children = children.ToArray();
                decimal left = children.Min(b => b.BoundingBox.Left);
                decimal right = children.Max(b => b.BoundingBox.Right);
                decimal bottom = children.Min(b => b.BoundingBox.Bottom);
                decimal top = children.Max(b => b.BoundingBox.Top);
                BoundingBox = new PdfRectangle(left, bottom, right, top);
            }
            else
            {
                Children = new XYNodeP[0];
            }
        }

        /// <summary>
        /// Recursively counts the words included in this node.
        /// </summary>
        public virtual int CountWords()
        {
            if (Children == null)
            {
                return 0;
            }

            int count = 0;
            RecursiveCount(Children, ref count);
            return count;
        }

        /// <summary>
        /// Recursively gets the leafs (last nodes) of this node.
        /// </summary>
        public virtual List<XYLeafP> GetLeafs()
        {
            List<XYLeafP> leafs = new List<XYLeafP>();
            if (Children == null || Children.Length == 0)
            {
                return leafs;
            }

            int level = 0;
            RecursiveGetLeafs(Children, ref leafs, level);
            return leafs;
        }

        private void RecursiveCount(IEnumerable<XYNodeP> children, ref int count)
        {
            if (children.Count() == 0) return;
            foreach (XYNodeP node in children.Where(x => x.IsLeaf))
            {
                count += node.CountWords();
            }

            foreach (XYNodeP node in children.Where(x => !x.IsLeaf))
            {
                RecursiveCount(node.Children, ref count);
            }
        }

        private void RecursiveGetLeafs(IEnumerable<XYNodeP> children, ref List<XYLeafP> leafs, int level)
        {
            if (children.Count() == 0) return;
            bool isVerticalCut = level % 2 == 0;

            foreach (XYLeafP node in children.Where(x => x.IsLeaf))
            {
                leafs.Add(node);
            }

            level++;

            IEnumerable<XYNodeP> notLeafs = children.Where(x => !x.IsLeaf);

            if (isVerticalCut)
            {
                notLeafs = notLeafs.OrderBy(x => x.BoundingBox.Left).ToList();
            }
            else
            {
                notLeafs = notLeafs.OrderByDescending(x => x.BoundingBox.Top).ToList();
            }

            foreach (XYNodeP node in notLeafs)
            {
                RecursiveGetLeafs(node.Children, ref leafs, level);
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return IsLeaf ? "Leaf" : "Node";
        }
    }
}
