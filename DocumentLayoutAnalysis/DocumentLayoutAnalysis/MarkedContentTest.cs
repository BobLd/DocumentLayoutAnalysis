namespace DocumentLayoutAnalysis
{
    using System;
    using UglyToad.PdfPig;

    /// <summary>
    /// Export pdf document the PAGE XML format.
    /// </summary>
    public static class MarkedContentTest
    {
        private const string pdfPath = "../../Resources/Samples/Random 2 Columns Lists Chart_PDF-A.pdf";

        public static void Run()
        {
            using (PdfDocument document = PdfDocument.Open(pdfPath))
            {
                for (int i = 0; i < document.NumberOfPages; i++)
                {
                    var page = document.GetPage(i + 1);

                    var mcs = page.GetMarkedContents();
                    foreach (var mc in mcs)
                    {
                        var letters = mc.Letters;
                        var paths = mc.Paths;
                        var images = mc.Images;

                        foreach (var letter in letters)
                        {
                            Console.Write(letter.Value);
                        }
                        Console.WriteLine();
                    }
                }
            }
        }
    }
}
