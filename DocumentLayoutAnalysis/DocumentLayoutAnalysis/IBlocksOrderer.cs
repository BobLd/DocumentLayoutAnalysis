using System.Collections.Generic;
using UglyToad.PdfPig.Content;

namespace DocumentLayoutAnalysis
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBlocksOrderer
    {
        /// <summary>
        /// Order the text blocks.
        /// </summary>
        /// <param name="pageBlocks">The  text blocks to be ordered.</param>
        /// <returns>A list of text blocks ordered from this approach.</returns>
        IReadOnlyList<TextBlock> GetBlocks(IEnumerable<TextBlock> pageBlocks);
    }
}
