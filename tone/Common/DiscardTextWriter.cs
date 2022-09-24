using System.IO;
using System.Text;

namespace tone.Common;

public class DiscardTextWriter: TextWriter
{
    public override void Write(char value)
    {
    }

    public override void Write(string? _)
    {
    }

    public override Encoding Encoding => Encoding.ASCII;
}