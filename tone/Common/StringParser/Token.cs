using System;
using System.Text;

namespace tone.Common.StringParser;
public struct Token<TTokenType>: IToken<TTokenType> 
    where TTokenType : struct, Enum
{
    public TTokenType Type { get; private set; }
    
    private readonly StringBuilder _value;
    public string Value => _value.ToString();

    public bool HasValue => _value != null && _value.Length != 0; 
    
    public Token(TTokenType type, string value)
    {
        Type = type;
        _value = new StringBuilder(value);
    }
    
    public void Append<TAppend>(TAppend value)
    {
        _value.Append(value);
    }
}
