using System;
using System.Collections.Generic;

namespace tone.Common.StringParser;

public class Tokenizer<TToken>
{
    public int MaximumRoundsWithoutResult { get; set; } = 3;
    
    private readonly Action<Scanner, IList<TToken>>[] _tokenBuilders;

    public Tokenizer(params Action<Scanner, IList<TToken>>[] tokenBuilders)
    {
        _tokenBuilders = tokenBuilders;
    }

    public IEnumerable<TToken> Tokenize(Scanner scanner)
    {
        var tokens = new List<TToken>();
        var resultLessRounds = 0;
        while (scanner.HasNextChar())
        {
            var scannerIndex = scanner.Index;
            
            foreach (var builder in _tokenBuilders)
            {
                var builderIndex = scanner.Index;
                var tokenCount = tokens.Count;
                
                builder(scanner, tokens);
                if (tokenCount < tokens.Count)
                {
                    break;
                }

                scanner.Index = builderIndex;
            }

            if (scanner.Index <= scannerIndex)
            {
                resultLessRounds++;
            }
            else
            {
                resultLessRounds = 0;
            }

            if (resultLessRounds > MaximumRoundsWithoutResult)
            {
                throw new Exception($"Exceeded {MaximumRoundsWithoutResult} of scanner not changing its index - tokenizer is not configured properly");
            }
        }

        return tokens;
    }
}