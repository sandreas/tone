using System;
using System.Collections.Generic;
using System.Linq;
using tone.Common.StringParser;

namespace tone.Tokenizers;

public class FfmetadataTokenizer : Tokenizer<Token<FfmetadataToken>>
{
    public FfmetadataTokenizer() : base(HeaderLine, CommentLine, ChapterMarker, StreamMarker, MetadataProperty)
    {
    }

    private static void HeaderLine(CharScanner scanner, IList<Token<FfmetadataToken>> tokenList)
    {
        if (tokenList.Any())
        {
            return;
        }

        var tokenValue = scanner.ReadLine();
        if (tokenValue.StartsWith(";FFMETADATA"))
        {
            tokenList.Add(new Token<FfmetadataToken>(FfmetadataToken.Header, tokenValue));
        }
    }

    private static void CommentLine(CharScanner scanner, IList<Token<FfmetadataToken>> tokenList)
    {
        var firstChar = scanner.Peek();
        var tokenValue = scanner.ReadLine();

        if (tokenValue == "" || firstChar is ';' or '#')
        {
            tokenList.Add(new Token<FfmetadataToken>(FfmetadataToken.Comment, tokenValue));
        }
    }

    private static void ChapterMarker(CharScanner scanner, IList<Token<FfmetadataToken>> tokenList)
    {
        var line = scanner.ReadLine();
        if (line.ToLower() != "[chapter]")
        {
            return;
        }

        tokenList.Add(new Token<FfmetadataToken>(FfmetadataToken.ChapterMarker, line));
    }

    private static void StreamMarker(CharScanner scanner, IList<Token<FfmetadataToken>> tokenList)
    {
        var line = scanner.ReadLine();
        if (line.ToLower() != "[stream]")
        {
            return;
        }

        tokenList.Add(new Token<FfmetadataToken>(FfmetadataToken.StreamMarker, line));
    }
    private static void MetadataProperty(CharScanner scanner, IList<Token<FfmetadataToken>> tokenList)
    {
        var buffer = "";
        var propertyName = "";
        var separatorChar = '=';
        var escaped = false;
        while (scanner.CanPeek())
        {
            var currentChar = scanner.Read();
            if (escaped)
            {
                buffer += currentChar;
                escaped = false;
                continue;
            }

            if (currentChar == '\\')
            {
                escaped = true;
                continue;
            }

            if (currentChar == separatorChar)
            {
                if (separatorChar == '=')
                {
                    propertyName = buffer;
                    buffer = "";
                    separatorChar = '\n';
                    continue;
                }

                break;
            }

            buffer += currentChar;
        }

        if (propertyName != "")
        {
            tokenList.Add(new Token<FfmetadataToken>(FfmetadataToken.PropertyName, propertyName));
            tokenList.Add(new Token<FfmetadataToken>(FfmetadataToken.PropertyValue, buffer));
        }
    }

}