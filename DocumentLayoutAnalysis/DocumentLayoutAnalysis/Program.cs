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
            string file = path + @"Random 2 Columns Lists Chart.pdf"; // 104-7-3.pdf"; // @"excel_2.pdf";

            //HocrTest.Run(file);
            //ImageTest.Run(file);
            //PathsTest.Run(file);
            //DocstrumBBTest.Run(file);
            //RXYCutTest.Run(file);
            RXYCutStepsTest.Run(file);
        }
    }
}
