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
            string file = @"Resources/Samples/104-7-3.pdf";

            ImageTest.Run(file);
            ShapesTest.Run(file);
        }
    }
}
