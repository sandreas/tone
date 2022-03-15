using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATL;
using OperationResult;
using tone.Common.Extensions.Enumerable;
using static OperationResult.Helpers;

namespace tone.Metadata.Formats;

public class FfmetadataFormat : IMetadataFormat
{
    // https://ffmpeg.org/ffmpeg-formats.html#Description

    private const string FfmetadataHeader = ";FFMETADATA";
    public async Task<Result<IMetadata, string>> ReadAsync(Stream input)
    {
        var metadata = new MetadataTrack();
        using var sr = new StreamReader(input);

        if (sr.Peek() < 0)
        {
            return Error("Peek failed");
        }

        var headerResult = await ReadHeaderAsync(sr);
        if (!headerResult)
        {
            return Error(headerResult.Error);
        }

        var properties = new Dictionary<string, string>();
        var lineNumber = -1;
        while (sr.Peek() >= 0)
        {
            lineNumber++;
            var line = await sr.ReadLineAsync();
            if (line == null)
            {
                return Error($"Could not read line {lineNumber}");
            }
            var trimmedLine = line.Trim();
            if (line.StartsWith(";") || trimmedLine == "")
            {
                continue;
            }
            
            if(string.Equals(trimmedLine, "[CHAPTER]", StringComparison.CurrentCultureIgnoreCase))
            {
                var chapters = ReadChapters(sr);
                foreach (var chapter in chapters)
                {
                    metadata.Chapters.Add(chapter);                    
                }
            }
            else
            {
                var (propertyName, propertyValue) = ReadProperty(line, sr);
            }
        }

        return metadata;
    }

    private (string, string) ReadProperty(string line, StreamReader sr)
    {
        var propertyNameBuilder = new StringBuilder();
        var propertyValueBuilder = new StringBuilder();
        var activeBuilder = propertyNameBuilder;
        var escaped = false;
        
        foreach (var c in line)
        {
            if (!escaped && c == '\\')
            {
                escaped = true;
                continue;
            }

            escaped = false;

            if (c == '=')
            {
                activeBuilder = propertyValueBuilder;
                continue;
            }
            activeBuilder.Append(c);
        }
        /*
        while (propertyValue.TrimEnd().EndsWith("\\"))
        {
            propertyValue += sr.ReadLine();
        }
*/
        return (propertyNameBuilder.ToString(), propertyValueBuilder.ToString());

        
    }

    private static string Unescape(string escapedString)
    {
        // ‘=’, ‘;’, ‘#’, ‘\’ and a newline

        var builder = new StringBuilder();
        var escaped = false;
        foreach (var c in escapedString)
        {
            if (!escaped && c == '\\')
            {
                escaped = true;
                continue;
            }

            builder.Append(c);
            escaped = false;
        }

        return builder.ToString();
    }
    
    private static string Escape(string str)
    {
        var charsToEscape = new[] { '=', ';', '#', '\\', '\n' };
        var builder = new StringBuilder();
        
        foreach (var c in str)
        {
            if (charsToEscape.Contains(c))
            {
                builder.Append('\\');
            }
            builder.Append(c);
        }

        return builder.ToString();
    }
    private IEnumerable<ChapterInfo> ReadChapters(StreamReader sr)
    {
        return new List<ChapterInfo>();
    }


    private async Task<Status<string>> ReadHeaderAsync(StreamReader sr)
    {
        var headerLine = await sr.ReadLineAsync() ?? "";
        if (!headerLine.StartsWith(FfmetadataHeader))
        {
            return Error("Could not find required header " + FfmetadataHeader);
        }
        return Ok();
    }


    public Task<Status<string>> WriteAsync(IMetadata input, Stream output)
    {
        throw new System.NotImplementedException();
    }
}