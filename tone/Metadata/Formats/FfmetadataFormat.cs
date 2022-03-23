using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATL;
using OperationResult;
using static OperationResult.Helpers;

namespace tone.Metadata.Formats;

enum SectionType
{
    Default,
    Chapter,
    Stream,
    EndOfStream
}
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

        var currentSectionType = SectionType.Default;
        while (currentSectionType != SectionType.EndOfStream)
        {
            var properties = ReadSectionProperties(sr, currentSectionType, out currentSectionType);
            switch (currentSectionType)
            {
                case SectionType.Chapter:
                    ParseChapterProperties(properties, metadata);
                    break;
                default:
                    ParseMetadataProperties(properties, metadata);
                    break;
                    
            }
        }

        return metadata;
    }

    private static void ParseMetadataProperties(Dictionary<string, string> properties, MetadataTrack metadata)
    {
    }

    private static void ParseChapterProperties(Dictionary<string, string> properties, MetadataTrack metadata)
    {
    }

    private Dictionary<string, string> ReadSectionProperties(StreamReader sr, SectionType currentSectionType, out SectionType nextSectionType)
    {
        var properties = new Dictionary<string, string>();
        nextSectionType = SectionType.Default;
        while (sr.Peek() != -1)
        {
            var line = sr.ReadLine();
            if (line == null)
            {
                nextSectionType = SectionType.EndOfStream;
                break;
            }
            if (line.StartsWith(";") || line.StartsWith("#"))
            {
                continue;
            }

            while (line.EndsWith("\\") && sr.Peek() != -1)
            {
                line += sr.ReadLine();
            }

            var trimmedLowerLine = line.Trim().ToLower();
            if (trimmedLowerLine == "[chapter]")
            {
                nextSectionType = SectionType.Chapter;
                break;
            }
            if (trimmedLowerLine == "[stream]")
            {
                nextSectionType = SectionType.Stream;
                break;
            }

            var (name, value) = ReadProperty(line);
            if (name != "")
            {
                properties[name] = value;
            }
        }
        
        return properties;
    }

    private (string, string) ReadProperty(string line)
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


    private static async Task<Status<string>> ReadHeaderAsync(StreamReader sr)
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