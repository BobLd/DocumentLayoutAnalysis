using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentLayoutAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = @"Resources/Samples/";
            string file = path + @"Random 2 Columns Lists Chart.pdf"; //"New Algorithm for Medial Axis Transform of Plane Domain.pdf"; // @"Random 2 Columns Lists Chart_PDF-A.pdf"; // @"Random 2 Columns Lists Chart.pdf"; // "104-7-3.pdf"; // @"excel_2.pdf";

            PageXmlTest.Run(file);
            //AltoTest.Run(file);
            HocrTest.Run(file);
            //ImageTest.Run(file);
            //VertexTest.Run(file);
            //PathsTest.Run(file);
            //DocstrumBBTest.Run(file);
            //RXYCutTest.Run(file);
            //RXYCutStepsTest.Run(file);
            //PdfATest.Run(file);
            //Console.ReadLine();
        }
    }
}
