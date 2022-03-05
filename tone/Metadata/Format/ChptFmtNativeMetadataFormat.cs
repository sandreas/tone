using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATL;
using OperationResult;
using tone.Common.Extensions.String;
using static OperationResult.Helpers;
namespace tone.Metadata.Format;

public class ChptFmtNativeMetadataFormat: IMetadataFormat
{
    private const string MetadataItemKeyTotalDuration = "total-duration";
    
    private static readonly List<(string, string)> MetadataItemKeyMappings = new()
    {
        ("total-duration:", MetadataItemKeyTotalDuration), 
        ("total-length", MetadataItemKeyTotalDuration)
    };
    
    public async Task<Result<IMetadata, string>> ReadAsync(Stream stream)
    {
        var track = new MetadataTrack();
        
        using var sr = new StreamReader(stream);
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
            
            var currentChapter  = ParseChapterLine(trimmedLine);
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
        if (chapters is { Count: > 0 } && chapters.Last().EndTime == 0 && commentTags.Any(t => t.Item1 == MetadataItemKeyTotalDuration))
        {
            chapters.Last().EndTime = (uint)((TimeSpan)commentTags.First(t => t.Item1 == MetadataItemKeyTotalDuration).Item2).TotalMilliseconds;
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
        if (!TimeSpan.TryParse(chapterStartString, out var chapterStart))
        {
            return null;
        }

        var start = (uint)chapterStart.TotalMilliseconds;
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

            if (destinationKey == "total-duration" && TimeSpan.TryParse(content.TrimPrefix(key).Trim(), out var totalDuration))
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
        if (input.Chapters == null || input.Chapters.Count == 0)
        {
            return Error("metadata does not contain any chapters");
        }

        var totalDurationMs = input.TotalDuration.TotalMilliseconds;
        if (totalDurationMs == 0)
        {
            totalDurationMs = input.Chapters.Last().EndTime;
        }
        if (totalDurationMs > 0)
        {
            var totalDurationAsString = FormatTimeSpan(TimeSpan.FromMilliseconds(totalDurationMs));
            await output.WriteAsync(Encoding.UTF8.GetBytes($"## {MetadataItemKeyTotalDuration}: {totalDurationAsString}"));
        }

        foreach (var chapter in input.Chapters)
        {
            var startAsString = FormatTimeSpan(TimeSpan.FromMilliseconds(chapter.StartTime));
            var title = chapter.Title;
            await output.WriteAsync(Encoding.UTF8.GetBytes($"\n{startAsString} {title}"));
        }
        
        return await Task.FromResult(Ok());
    }

    private static string FormatTimeSpan(TimeSpan timeSpan)
    {
        var totalHoursAsString = Math.Floor(timeSpan.TotalHours).ToString(CultureInfo.InvariantCulture)
            .PadLeft(2, '0');
        return totalHoursAsString + timeSpan.ToString(@"\:mm\:ss\.fff");

    }
}