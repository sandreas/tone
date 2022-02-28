using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ATL;
using tone.Common.Extensions.String;

namespace tone.Metadata.Parsers;

public class ChptFmtNativeParser
{
    private const string MetadataItemKeyTotalDuration = "total-duration";
    
    private static readonly List<(string, string)> MetadataItemKeyMappings = new()
    {
        ("total-duration:", MetadataItemKeyTotalDuration), 
        ("total-length", MetadataItemKeyTotalDuration),
    };

    public IMetadata Parse(Stream stream)
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
            if (currentChapter != null)
            {
                track.Chapters ??= new List<ChapterInfo>();
                if (track.Chapters.Count > 0)
                {
                    track.Chapters.Last().EndTime = currentChapter.StartTime;
                }
                track.Chapters.Add(currentChapter);
            }
        }

        var chapters = track.Chapters;
        // due to the format spec, the last chapter does not contain a length, so it has to be added via metadata item (command tag)
        if (chapters is { Count: > 0 } && chapters.Last().EndTime == 0 && commentTags.Any(t => t.Item1 == MetadataItemKeyTotalDuration))
        {
            chapters.Last().EndTime = (uint)((TimeSpan)commentTags.First(t => t.Item1 == MetadataItemKeyTotalDuration).Item2).TotalMilliseconds;
        } 

        return track;
    }

    private ChapterInfo? ParseChapterLine(string line)
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

    private void ParseComment(string line, List<(string, object)> metadata)
    {
        var content = line.TrimStart().TrimStart('#').TrimStart();
        
        foreach (var (key, destinationKey) in MetadataItemKeyMappings)
        {
            if (!content.StartsWith(key))
            {
                continue;
            }

            if (destinationKey == "total-duration")
            {
                var durationValueString = content.TrimPrefix(key).Trim();
                if(TimeSpan.TryParse(durationValueString, out var duration))
                {
                    metadata.Add((destinationKey, duration));
                    break;
                }
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
}