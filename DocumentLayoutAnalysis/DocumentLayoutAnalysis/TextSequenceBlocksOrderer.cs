using System;
using System.Collections.Generic;
using System.Linq;
using UglyToad.PdfPig.Content;

namespace DocumentLayoutAnalysis
{
    /// <summary>
    /// 
    /// </summary>
    public class TextSequenceBlocksOrderer : IBlocksOrderer
    {
        /// <summary>
        /// Create an instance of text sequence number blocks orderer, <see cref="TextSequenceBlocksOrderer"/>.
        /// </summary>
        public static TextSequenceBlocksOrderer Instance { get; } = new TextSequenceBlocksOrderer();

        /// <summary>
        /// Uses the Average Text Sequence.
        /// </summary>
        /// <param name="pageBlocks"></param>
        /// <returns></returns>
        public IReadOnlyList<TextBlock> GetBlocks(IEnumerable<TextBlock> pageBlocks)
        {
            // need to handle case where average is equal
            return GetBlocks(pageBlocks, AvgTextSequence);
        }

        public IReadOnlyList<TextBlock> GetBlocks(IEnumerable<TextBlock> pageBlocks, Func<TextBlock, double> orderingFunc)
        {
            // need to handle case where outputs are equal
            return pageBlocks.OrderBy(b => orderingFunc(b)).ToList();
        }

        private double AvgTextSequence(TextBlock textBlock)
        {
            var letters = textBlock.TextLines.SelectMany(li => li.Words.SelectMany(w => w.Letters));
            return letters.Average(l => (double)l.FontSize); // TextSequence 
        }
    }
}
