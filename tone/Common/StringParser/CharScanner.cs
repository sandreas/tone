using System.Linq;

namespace tone.Common.StringParser;

public class CharScanner: Scanner<char>
{
    public CharScanner(string input): base(input.ToArray())
    {

    }
    
    public string ReadLine()
    {
        return ReadLineWithDelimiter().TrimEnd('\r', '\n');
    }
    
    private string ReadLineWithDelimiter()
    {
        var line = "";
        while (CanPeek())
        {
            var currentChar = Read();
            line += currentChar;
            if (currentChar == '\r' && Peek() != '\n')
            {
                break;
            }

            if (currentChar == '\n')
            {
                break;
            }
        }

        return line;
    }
}