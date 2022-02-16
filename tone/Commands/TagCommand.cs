using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using ATL;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Sandreas.Files;
using tone.Services;
using Serilog;
using tone.Common.Extensions;

namespace tone.Commands;

[Command("tag")]
public class TagCommand : ICommand
{
    private readonly ILogger _logger;
    private readonly FileWalker _fileWalker;
    private readonly DirectoryLoaderService _dirLoader;

    [CommandOption("input", 'i')] public IReadOnlyList<string> Input { get; init; }

    [CommandOption("dump")] public bool Dump { get; init; } = false;
    [CommandOption("include-extensions")] public IReadOnlyList<string> IncludeExtensions { get; init; }


    /*
    private readonly ILogger<ICommand> _logger;
    private readonly FileWalker _fileWalker;
    private readonly DirectoryLoaderService _dirLoader;

    public TagCommand(ILogger<ICommand> logger, FileWalker fileWalker, DirectoryLoaderService dirLoader)
    {
        _logger = logger;
        _fileWalker = fileWalker;
        _dirLoader = dirLoader;
    }


    public async Task<int> ExecuteAsync(TagOptions options)
    {
        var audioExtensions = DirectoryLoaderService.ComposeAudioExtensions(options.IncludeExtensions);
        var inputFiles = _dirLoader.SeekFiles(options.Input, audioExtensions).ToList();

        foreach (var file in inputFiles)
        {
            try
            {
                var track = new Track(file.path);
                track.Album = options.Album ?? track.Album;
                track.Artist = options.Artist ?? track.Artist;
                if (!track.Save())
                {
                    _logger.LogWarning("Could not save tags for {FilePath}", file.path);
                };
            }
            catch (Exception e)
            {
                _logger.LogError("Could not save tag for {FilePath}, exception: {ExceptionMessage}", file.path, e.Message);
            }

        }
        return await Task.FromResult(0);
    }

    private void SetTagValue(string? optionsAlbum, out string trackAlbum)
    {
        throw new NotImplementedException();
    }
    */
    public TagCommand(ILogger logger, FileWalker fileWalker, DirectoryLoaderService dirLoader)
    {
        _logger = logger;
        _fileWalker = fileWalker;
        _dirLoader = dirLoader;
    }

    public async ValueTask ExecuteAsync(IConsole console)
    {
        var audioExtensions = DirectoryLoaderService.ComposeAudioExtensions(IncludeExtensions);
        var inputFiles = _dirLoader.FindFilesByExtension(Input, audioExtensions).ToImmutableArray();


        foreach (var file in inputFiles)
        {
            if (Dump)
            {
                await DumpTags(console, file);
                continue;
            }
        }

        // console.WriteErrorLine(inputFiles.Count().ToString());

        // await console.Output.WriteLineAsync("hello tag command");
        // _logger.Error("Error testing from tag command");
    }

    private async Task DumpTags(IConsole console, IFileInfo file)
    {
        var track = new Track(file.FullName);
        var properties = track.GetType().GetProperties();

        var skipProperties = new[] { "Year", "Duration" };
        // Special lists: Chapters, PictureTokens, MetadataFormats, EmbeddedPictures
        // Special props: Lyrics, AudioFormat, TechnicalInformation
        foreach (var prop in properties)
        {
            var name = prop.Name;
            if (skipProperties.Contains(name))
            {
                continue;
            }

            var propValue = prop.GetValue(track, null);

            if (name == "DurationMs")
            {
                name = "Duration";
                propValue =  TimeSpan.FromMilliseconds((double)(propValue ?? 0)).ToString(@"hh\:mm\:ss\.fff");
            }

            if (propValue is DateTime dateTime && dateTime == DateTime.MinValue)
            {
                propValue = "";
            }

            if (propValue is LyricsInfo lyrics)
            {
                propValue = lyrics.UnsynchronizedLyrics;
            }

            if (propValue is Format format)
            {
                propValue = format.Name + " - " + string.Join(", ", format.MimeList);
            }

            if (string.IsNullOrEmpty(propValue?.ToString()))
            {
                continue;
            }

            if (propValue is IList list)
            {
                propValue = $"<listing> (length: {list.Count})";
            }

            if (propValue is IDictionary<string, string> dict)
            {
                await console.Output.WriteLineAsync(name + ": ");
                foreach (var (key, value) in dict)
                {
                    await console.Output.WriteLineAsync(" - " + key + ": " + value);
                }

                continue;
            }

            await console.Output.WriteLineAsync(name + ": " + propValue);
        }
        /*
Title: Kapitel 1 - Tochter des Meeres - Der Ursprung der Elemente, Band 1
Artist: A. L. Knorr
Composer: 
Comment: 
Genre: 
Album: Tochter des Meeres - Der Ursprung der Elemente, Band 1 (Ungekürzt)
OriginalAlbum: 
OriginalArtist: 
Copyright: 
Description: 
Publisher: 
PublishingDate: 01/01/0001 00:00:00
AlbumArtist: A. L. Knorr
Conductor: 
Date: 01/01/2021 00:00:00
Year: 2021
TrackNumber: 1
TrackTotal: 157
DiscNumber: 1
DiscTotal: 1
Popularity: 0
PictureTokens: System.Collections.Generic.List`1[ATL.PictureInfo]
ChaptersTableDescription: 
Chapters: System.Collections.Generic.List`1[ATL.ChapterInfo]
Lyrics: ATL.LyricsInfo
AdditionalFields: System.Collections.Generic.Dictionary`2[System.String,System.String]
Bitrate: 128
SampleRate: 44100
IsVBR: False
CodecFamily: 0
AudioFormat: ATL.Format
MetadataFormats: System.Collections.Generic.List`1[ATL.Format]
Duration: 184
DurationMs: 184080
ChannelsArrangement: Joint Stereo
TechnicalInformation: ATL.TechnicalInfo
EmbeddedPictures: System.Collections.Generic.List`1[ATL.PictureInfo]
        */
    }
}