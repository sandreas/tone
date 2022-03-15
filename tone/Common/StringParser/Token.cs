using System;
using System.Text;

namespace tone.Common.StringParser;

public struct Token<TTokenType>
    where TTokenType : struct, Enum
{
    public TTokenType Type { get; }
    public string Value { get; }
    
    public Token(TTokenType type, string value)
    {
        Type = type;
        Value = value;
    }
}