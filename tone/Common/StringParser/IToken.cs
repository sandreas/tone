using System;

namespace tone.Common.StringParser;

public interface IToken<TTokenType> where TTokenType : struct, Enum

{
    public TTokenType Type { get; }
    public string Value { get; }
    public bool HasValue { get; }

    public void Append<TAppend>(TAppend value);
}