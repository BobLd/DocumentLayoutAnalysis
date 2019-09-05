using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig.Geometry;
using static UglyToad.PdfPig.Geometry.PdfPath;

namespace DocumentLayoutAnalysis
{
    public static class Extensions
    {
        public static PdfRectangle BoundingBox(this PdfPath pdfPath)
        {
            IReadOnlyList<IPathCommand> commands = pdfPath.Commands;

            var minX = decimal.MaxValue;
            var maxX = decimal.MinValue;

            var minY = decimal.MaxValue;
            var maxY = decimal.MinValue;

            foreach (var command in commands)
            {
                var rect = command.GetBoundingRectangle();
                if (rect == null)
                {
                    continue;
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

            return new PdfRectangle(minX, minY, maxX, maxY);
        }
    }
}
