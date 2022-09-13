using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATL;
using OperationResult;
using Sandreas.AudioMetadata;
using tone.Metadata.Extensions;

namespace tone.Metadata.Formats;

public class ChptFmtNativeMetadataFormat : IMetadataFormat
{
    private const string MetadataItemKeyTotalDuration = "total-duration";

    private static readonly List<(string, string)> MetadataItemKeyMappings = new()
    {
        ("total-duration:", MetadataItemKeyTotalDuration),
        ("total-length", MetadataItemKeyTotalDuration)
    };

    public async Task<Result<IMetadata, string>> ReadAsync(Stream input)
    {
        var track = new MetadataTrack();

        using var sr = new StreamReader(input);
        var lines = ReadLines(sr).ToArray();
        var commentTags = new List<(string, object)>();

        foreach (var line in lines)
        {
            if (line == null)
            {
                continue;
            }

            var trimmedLine = line.TrimStart();
            if (trimmedLine.StartsWith("#"))
            {
                ParseComment(trimmedLine, commentTags);
                continue;
            }

            var currentChapter = ParseChapterLine(trimmedLine);
            if (currentChapter == null)
            {
                continue;
            }

            track.Chapters ??= new List<ChapterInfo>();
            if (track.Chapters.Count > 0)
            {
                track.Chapters.Last().EndTime = currentChapter.StartTime;
            }

            track.Chapters.Add(currentChapter);
        }

        var chapters = track.Chapters;
        // due to the format spec, the last chapter does not contain a length, so it has to be added via metadata item (command tag)
        if (chapters is { Count: > 0 } && chapters.Last().EndTime == 0 &&
            commentTags.Any(t => t.Item1 == MetadataItemKeyTotalDuration))
        {
            chapters.Last().EndTime =
                (uint)((TimeSpan)commentTags.First(t => t.Item1 == MetadataItemKeyTotalDuration).Item2)
                .TotalMilliseconds;
        }

        return await Task.FromResult(track);
    }


    private static ChapterInfo? ParseChapterLine(string line)
    {
        var parts = line.Split();
        if (parts.Length == 0)
        {
            return null;
        }

        var chapterStartString = parts.First();
        
        var chapterStart = ParseTimeSpan(chapterStartString);
        if(chapterStart == null)        {
            return null;
        }
        var start = (uint)chapterStart.Value.TotalMilliseconds;
        return new ChapterInfo(start, line[chapterStartString.Length..].Trim());
    }

    private static void ParseComment(string line, ICollection<(string, object)> metadata)
    {
        var content = line.TrimStart().TrimStart('#').TrimStart();

        foreach (var (key, destinationKey) in MetadataItemKeyMappings)
        {
            if (!content.StartsWith(key))
            {
                continue;
            }

            if (destinationKey == "total-duration" &&
                TimeSpan.TryParse(content.TrimPrefix(key).Trim(), out var totalDuration))
            {
                metadata.Add((destinationKey, totalDuration));
            }
        }
    }

    private static IEnumerable<string?> ReadLines(TextReader sr)
    {
        while (sr.Peek() >= 0)
        {
            yield return sr.ReadLine();
        }
    }

    public async Task<Status<string>> WriteAsync(IMetadata input, Stream output)
    {
        if (input.Chapters.Count == 0)
        {
            return Helpers.Error("metadata does not contain any chapters");
        }

        var totalDurationMs = input.TotalDuration.TotalMilliseconds;
        if (totalDurationMs == 0)
        {
            totalDurationMs = input.Chapters.Last().EndTime;
        }

        if (totalDurationMs > 0)
        {
            var totalDurationAsString = FormatTimeSpan(TimeSpan.FromMilliseconds(totalDurationMs));
            await output.WriteAsync(
                Encoding.UTF8.GetBytes($"## {MetadataItemKeyTotalDuration}: {totalDurationAsString}"));
        }

        foreach (var chapter in input.Chapters)
        {
            var startAsString = FormatTimeSpan(TimeSpan.FromMilliseconds(chapter.StartTime));
            var title = chapter.Title;
            await output.WriteAsync(Encoding.UTF8.GetBytes($"\n{startAsString} {title}"));
        }

        return await Task.FromResult(Helpers.Ok());
    }

    private static string FormatTimeSpan(TimeSpan timeSpan)
    {
        var totalHoursAsString = Math.Floor(timeSpan.TotalHours).ToString(CultureInfo.InvariantCulture)
            .PadLeft(2, '0');
        return totalHoursAsString + timeSpan.ToString(@"\:mm\:ss\.fff");
    }
    
    private static TimeSpan? ParseTimeSpan(string chapterStartString)
    {        
        var firstSeparatorPos = chapterStartString.IndexOf(":", StringComparison.Ordinal);
        var hourPart = chapterStartString[..firstSeparatorPos];
        if(hourPart.StartsWith("0"))
        {
            hourPart = hourPart.TrimStart('0');
            if(hourPart == "")
            {
                hourPart = "0";
            }
        }
        if(!int.TryParse(hourPart, out var hours)){
            return null;
        }

        var parseablePart = "00" + chapterStartString[firstSeparatorPos..];
        if (!TimeSpan.TryParse(parseablePart, out var chapterStart))
        {
            return null;
        }

        chapterStart = chapterStart.Add(TimeSpan.FromHours(hours));
        return chapterStart;
    }
}