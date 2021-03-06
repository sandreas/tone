using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GrokNet;
using OperationResult;
using static OperationResult.Helpers;

using tone.Common.Extensions.String;

namespace tone.Services;

public class GrokPatternService
{
    public Result<IEnumerable<(string, Grok)>, string> Build(IEnumerable<string>? grokDefinitions,
        IEnumerable<string>? customPatterns = null)
    {
        customPatterns ??= new List<string>();
        var customPatternsArray = customPatterns.ToArray();
        try
        {
            var validationResult = ValidateCustomPatterns(customPatternsArray);
            if (!validationResult)
            {
                return Error(validationResult.Error);
            }
            return  Ok(ConvertStrings(grokDefinitions, customPatternsArray));
        }
        catch (Exception e)
        {
            return Error(e.Message);
        }
    }

    private static Status<string> ValidateCustomPatterns(IEnumerable<string> customPatterns)
    {
        foreach (var pattern in customPatterns)
        {
            var result = ValidateCustomPatternLine(pattern);
            if (!result)
            {
                return Error(result.Error);
            }
        }

        return Ok();
    }
    
    private static Status<string> ValidateCustomPatternLine(string line)
    {
        if (string.IsNullOrEmpty(line) || line.TrimStart().StartsWith("#"))
        {
            return Ok();
        }

        var strArray = line.Split(new[] { ' ' }, 2);
        if (strArray.Length != 2)
        {
            return Error("Custom pattern must be separated by space and consist of two parts");
        }
        
        try
        {
            _ = Regex.Match("", strArray[1]);
        }
        catch
        {
            return Error( $"Invalid regex pattern {strArray[1]}");
        }

        return Ok();
    }
    

    private static IEnumerable<(string, Grok)> ConvertStrings(IEnumerable<string>? grokDefinitions, IEnumerable<string>? customPatterns = null)
    {
        var patternsString = string.Join("\n", customPatterns ?? Array.Empty<string>());
        
        var grokDefArray = grokDefinitions?.ToList() ?? new List<string>();
        var groks = new List<(string, Grok)>();
        foreach (var pattern in grokDefArray)
        {
            try
            {
                // this has to be done IN the foreach to prevent reading closed streams
                using var patternsStream = patternsString.StringToStream();
                var preparedPattern = PreparePattern(pattern);
                groks.Add((preparedPattern, new Grok(preparedPattern, patternsStream)));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        return groks;
        // return grokDefinitions?.Select(pattern => new Grok(PreparePattern(pattern), patternsStream));
    }
    
    private static string PreparePattern(string pattern)
    {
        var buffer = new StringBuilder();
        var isPlaceholder = false;
        var placeHolders = new Dictionary<char, string>()
        {
            {'a', "{NOTDIRSEP:Artist}" }, // artist: ,
            {'A', "{NOTDIRSEP:SortArtist}" }, // sort_artist: ,
            {'c', "{NOTDIRSEP:Comment}" }, // comment: ,
            {'C', "{NOTDIRSEP:Copyright}" }, // copyright: ,
            {'d', "{NOTDIRSEP:Description}" }, // description: ,
            {'D', "{NOTDIRSEP:LongDescription}" }, // long_description: ,
            //{'e', "" }, // encoded_by: ,
            {'g', "{NOTDIRSEP:Genre}" }, // genre: ,
            {'m', "{NOTDIRSEP:Album}" }, // album: ,
            {'M', "{NOTDIRSEP:SortAlbum}" }, // sort_album: ,
            {'n', "{NOTDIRSEP:Title}" }, // title / name: 
            {'N', "{NOTDIRSEP:SortTitle}" }, // sort_name: 
            {'p', "{PARTNUMBER:Part}" }, // series_part: ,
            {'s', "{NOTDIRSEP:MovementName}" }, // series: ,
            {'t', "{NOTDIRSEP:AlbumArtist}" }, // album_artist: ,
            {'w', "{NOTDIRSEP:Composer}" }, // writer: ,
            {'y', "{NOTDIRSEP:ReleaseDate}" }, // year: ,
            {'z', "{NOTDIRSEP:IgnoreDummy}" }, // IgnoreDummy
            {'Z', "{PARTNUMBER:IgnoreDummy}" }, // IgnoreDummy
        };
        foreach (var c in pattern)
        {
            if (isPlaceholder && placeHolders.ContainsKey(c))
            {
                buffer.Append("%" + placeHolders[c]);
                isPlaceholder = false;
                continue;
            } 
            if (!isPlaceholder && c == '%')
            {
                isPlaceholder = true;
                continue;
            }

            buffer.Append(c);
            isPlaceholder = false;
        }
        return buffer.ToString().TrimDirectorySeparatorEnd();
    }

}