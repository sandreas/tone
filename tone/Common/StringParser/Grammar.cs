using System.Collections.Generic;
namespace tone.Common.StringParser;

public delegate TToken TokenBuilder<in TScanner, in TExistingTokens, out TToken>(TScanner scanner, TExistingTokens arg2)
    where TScanner : Scanner
    where TExistingTokens : IEnumerable<TToken?>;

public class Grammar<TScanner, TExistingTokens, TToken> 
    where TScanner:Scanner 
    where TExistingTokens:IEnumerable<TToken?>
{
    private readonly TokenBuilder<TScanner,TExistingTokens, TToken>[] _tokenBuilders;

    public Grammar(TokenBuilder<TScanner, TExistingTokens, TToken>[] tokenBuilders)
    {
        _tokenBuilders = tokenBuilders;
    }
    
    public TToken? BuildNextToken(ref TScanner scanner, ref TExistingTokens existingTokens){
        foreach(var builder in _tokenBuilders )
        {
            var token = builder(scanner, existingTokens);
            if(token != null)
            {
                return token;
            }
        }
        return default;
    }
    // todo: Build default token?
    
}