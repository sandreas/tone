namespace tone.Common.StringParser;

public class Scanner
{
    private readonly char[] _input;
    private readonly int _maxIndex;
    public int Index { get; set; }

    public Scanner(string input)
    {
        _input = input.ToCharArray();
        Index = 0;
        _maxIndex = _input.Length - 1;
    }

    public char? Peek()
    {
        return CharAtOffset(0);
    }

    public char? Poke()
    {
        var value = Peek();
        if (value != null)
        {
            Index++;
        }

        return value;
    }

    private char? CharAtOffset(int offset)
    {
        var i = Index + offset;
        return i >= 0 && i <= _maxIndex ? _input[i] : null;
    }

    public bool HasNextChar()
    {
        return Peek() != null;
    }


    public string ReadLine()
    {
        return ReadLineWithDelimiter().TrimEnd('\r', '\n');
    }
    
    private string ReadLineWithDelimiter()
    {
        var line = "";
        while (HasNextChar())
        {
            var currentChar = Poke();
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