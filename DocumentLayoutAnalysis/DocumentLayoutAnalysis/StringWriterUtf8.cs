using System.IO;
using System.Text;

namespace DocumentLayoutAnalysis
{
    internal class StringWriterUtf8 : StringWriter
    {
        public StringWriterUtf8() : base()
        {
        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
}
