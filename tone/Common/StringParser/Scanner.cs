namespace tone.Common.StringParser;

public class Scanner<T>
{
    private readonly T[] _input;
    private readonly int _lastValidIndex;
    public int Index { get; set; }

    public Scanner(T[] input)
    {
        _input = input;
        Index = 0;
        _lastValidIndex = _input.Length - 1;
    }

    public T? Peek()
    {
        return CharAtOffset(0);
    }

    public T? Read()
    {
        var value = Peek();
        if (value != null)
        {
            Index++;
        }

        return value;
    }

    
    public T? Next()
    {
        if (Index == _lastValidIndex)
        {
            return default;
        }
        Index++;
        return Peek();
    }
    
    private T? CharAtOffset(int offset)
    {
        var i = Index + offset;
        return i >= 0 && i <= _lastValidIndex ? _input[i] : default;
    }

    public bool CanPeek()
    {
        return Index <= _lastValidIndex;
    }
}