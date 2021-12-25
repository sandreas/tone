using System;
using System.Collections.Generic;
using System.Linq;

namespace tone.Common.StringParser;

public class Tokenizer<TScanner, TTokenList, TToken, TTokenType>
    where TScanner : Scanner
    where TTokenList : IEnumerable<TToken?>, new()
    where TToken : IToken<TTokenType>
    where TTokenType : struct, Enum
{
    private readonly Grammar<TScanner, TTokenList, TToken> _grammar;
    private readonly TToken _defaultToken;

    public int MaxFailedCount { get; set; } = 1;
    
    public Tokenizer(Grammar<TScanner, TTokenList, TToken> grammar, TToken defaultToken)
    {
        _grammar = grammar;
        _defaultToken = defaultToken;
    }
    
    public TTokenList Tokenize(TScanner scanner)
    {
        // todo: check IEnumerable and yield return?

        var tokens = new TTokenList();
        var fallbackToken = _defaultToken;
        var maxFailedCount = MaxFailedCount;
        while(scanner.HasNext()){
            var lastScannerPosition = scanner.Index;
            var token = _grammar.BuildNextToken(ref scanner, ref tokens);
            
            if(token == null)
            {
                fallbackToken.Append(token);
                continue;
            }   
            
            if(MaxFailedCount > 0 && scanner.Index <= lastScannerPosition && --maxFailedCount < 1)
            {
                throw new Exception(
                    $"Scanner as not moved forward since %s iterations (at position {scanner.Index}), so there seems to be something wrong with your grammar - to prevent endless loops, the tokenizer has been stopped");
            }  
            // EqualityComparer<T>.Default.Equals(
            // if token == null or scannerpositon rewind, throw exception?
            if(EqualityComparer<TTokenType>.Default.Equals(token.Type, fallbackToken.Type))
            {
                fallbackToken.Append(token.Value);
                continue;
            }

            AppendTokensWithValue(ref tokens, fallbackToken, token);

        }

        AppendTokensWithValue(ref tokens, fallbackToken);
        
        return tokens;
    }

    private void AppendTokensWithValue(ref TTokenList tokenList, params TToken[] tokens)
    {
        foreach (var token in tokens)
        {
            if (token.HasValue)
            {
                _ = tokenList.Append(token);
            }
        }
    }
}


