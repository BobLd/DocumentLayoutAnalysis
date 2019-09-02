using ImageConverter;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Geometry;
using static UglyToad.PdfPig.Geometry.PdfPath;

namespace DocumentLayoutAnalysis
{
    public class ShapesTest
    {
        public static void Run(string path)
        {
            float zoom = 10;
            var pinkPen = new Pen(Color.HotPink, zoom * 0.4f);
            var greenPen = new Pen(Color.GreenYellow, zoom * 0.4f);
            var redPen = new Pen(Color.Red, zoom * 0.4f);
            var bluePen = new Pen(Color.Blue, zoom * 0.4f);

            using (var converter = new PdfImageConverter(path))
            using (PdfDocument document = PdfDocument.Open(path))
            {
                for (var i = 0; i < document.NumberOfPages; i++)
                {
                    var page = document.GetPage(i + 1);
                    var paths = page.ExperimentalAccess.Paths;

                    using (var bitmap = converter.GetPage(i + 1, zoom))
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        var imageHeight = bitmap.Height;

                        foreach (var letter in page.Letters)
                        {
                            var rect = new Rectangle(
                                (int)(letter.GlyphRectangle.Left * (decimal)zoom),
                                imageHeight - (int)(letter.GlyphRectangle.Top * (decimal)zoom),
                                (int)(letter.GlyphRectangle.Width * (decimal)zoom),
                                (int)(letter.GlyphRectangle.Height * (decimal)zoom));
                            graphics.DrawRectangle(pinkPen, rect);
                        }

                        foreach (var p in paths)
                        {
                            if (p == null) continue;
                            var commands = p.Commands;

                            //var bbox = GetBoundingRectangle(commands);
                            //if (bbox.HasValue)
                            //{
                            //    var rect = new Rectangle(
                            //        (int)(bbox.Value.Left * (decimal)zoom),
                            //        imageHeight - (int)(bbox.Value.Top * (decimal)zoom),
                            //        (int)(bbox.Value.Width == 0 ? 1 : bbox.Value.Width * (decimal)zoom),
                            //        (int)(bbox.Value.Height == 0 ? 1 : bbox.Value.Height * (decimal)zoom));
                            //    graphics.DrawRectangle(bluePen, rect);
                            //}

                            foreach (var command in commands)
                            {
                                if (command is PdfPath.Line line)
                                {
                                    var bbox = line.GetBoundingRectangle();
                                    if (bbox.HasValue)
                                    {
                                        var rect = new Rectangle(
                                            (int)(bbox.Value.Left * (decimal)zoom),
                                            imageHeight - (int)(bbox.Value.Top * (decimal)zoom),
                                            (int)(bbox.Value.Width == 0 ? 1 : bbox.Value.Width * (decimal)zoom),
                                            (int)(bbox.Value.Height == 0 ? 1 : bbox.Value.Height * (decimal)zoom));
                                        graphics.DrawRectangle(bluePen, rect);
                                    }
                                }
                                else if (command is PdfPath.BezierCurve curve)
                                {
                                    var bbox = curve.GetBoundingRectangle();
                                    if (bbox.HasValue)
                                    {
                                        var rect = new Rectangle(
                                            (int)(bbox.Value.Left * (decimal)zoom),
                                            imageHeight - (int)(bbox.Value.Top * (decimal)zoom),
                                            (int)(bbox.Value.Width == 0 ? 1 : bbox.Value.Width * (decimal)zoom),
                                            (int)(bbox.Value.Height == 0 ? 1 : bbox.Value.Height * (decimal)zoom));
                                        graphics.DrawRectangle(redPen, rect);
                                    }
                                }
                                else if (command is PdfPath.Close close)
                                {
                                    var bbox = close.GetBoundingRectangle();
                                    if (bbox.HasValue)
                                    {
                                        var rect = new Rectangle(
                                            (int)(bbox.Value.Left * (decimal)zoom),
                                            imageHeight - (int)(bbox.Value.Top * (decimal)zoom),
                                            (int)(bbox.Value.Width == 0 ? 1 : bbox.Value.Width * (decimal)zoom),
                                            (int)(bbox.Value.Height == 0 ? 1 : bbox.Value.Height * (decimal)zoom));
                                        graphics.DrawRectangle(greenPen, rect);
                                    }
                                }
                                else if (command is PdfPath.Move move)
                                {
                                    var bbox = move.GetBoundingRectangle();
                                    if (bbox.HasValue)
                                    {
                                        var rect = new Rectangle(
                                            (int)(bbox.Value.Left * (decimal)zoom),
                                            imageHeight - (int)(bbox.Value.Top * (decimal)zoom),
                                            (int)(bbox.Value.Width == 0 ? 1 : bbox.Value.Width * (decimal)zoom),
                                            (int)(bbox.Value.Height == 0 ? 1 : bbox.Value.Height * (decimal)zoom));
                                        graphics.DrawRectangle(greenPen, rect);
                                    }
                                }
                            }
                        }

                        bitmap.Save(Path.ChangeExtension(path, (i + 1) + ".png"));
                    }
                }
            }
        }

        internal static PdfRectangle? GetBoundingRectangle(IReadOnlyList<IPathCommand> commands)
        {
            if (commands.Count == 0)
            {
                return null;
            }

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
