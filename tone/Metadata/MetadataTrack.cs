using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Runtime.CompilerServices;
using ATL;
using ATL.AudioData;

namespace tone.Metadata;

public class MetadataTrack : Track, IMetadata
{
    public new string? Path => base.Path;

    public DateTime? RecordingDate
    {
        get => Date;
        set => Date = value;
    }

    private TimeSpan? _totalDuration;

    public TimeSpan TotalDuration
    {
        get => _totalDuration ?? TimeSpan.FromMilliseconds(DurationMs);
        set => _totalDuration = value;
    }

    private static
        Dictionary<string, (string ID3v23, string ID3v24, string MP4, string Matroska, string ASF_WindowsMedia, string
            RiffInfo)> TagMapping { get; set; } = new()
    {
        // REF_HEADLINE (SuggestedPropertyName): ["ID3v2.3","ID3v2.4","MP4","Matroska","ASF/Windows Media","RIFF INFO"]
        { nameof(Group), ("TIT1", "TIT1", "©grp", "T=30", "WM/ArtistSortOrder", "") },
        { nameof(SortAlbum), ("TSOA", "TSOA", "soal", "T=50 SORT_WITH", "WM/AlbumSortOrder", "") },
        { nameof(SortAlbumArtist), ("TSO2", "TSO2", "soaa", "T=30", "", "") },
        { nameof(SortArtist), ("TSOP", "TSOP", "soar", "T=30", "WM/ArtistSortOrder", "") },
        { nameof(EncodedBy), ("TENC", "TENC", "", "T=30 ENCODED_BY", "WM/EncodedBy", "") },
        { nameof(EncoderSettings), ("TSSE", "TSSE", "©enc", "T=30", "WM/EncodingSettings", "") },
        { nameof(MovementName), ("MVNM", "MVNM", "©mvn", "T=20 TITLE", "", "") },
        { nameof(Movement), ("MVIN", "MVIN", "©mvi", "T=20 PART_NUMBER", "", "") },
        // {nameof(MovementTotal), ("MVIN","MVIN","©mvc","T=30","","")}, // special case: MVIN has to be appended
        { nameof(LongDescription), ("TDES", "TDES", "ldes", "T=30", "", "") },
        { nameof(Subtitle), ("TIT3", "TIT3", "----:com.apple.iTunes:SUBTITLE" /*or ©st3?*/, "T=30", "WM/SubTitle", "") },
        { nameof(SortTitle), ("TSOT", "TSOT", "sonm", "T=30 SORT_WITH", "WM/TitleSortOrder", "") },
        { nameof(Narrator), ("", "", "©nrt", "T=30", "", "") },
        { nameof(PurchaseDate), ("", "", "purd", "", "", "") },
        { nameof(ItunesMediaType), ("", "", "stik", "", "", "") },
        { nameof(EncodingTool), ("", "", "©too", "", "", "") },
        { nameof(ItunesGapless), ("", "", "pgap", "", "", "") },
    };

    public string? Group
    {
        get => GetAdditionalField(StringField);
        set => SetAdditionalField(value);
    }

    public string? SortAlbum
    {
        get => GetAdditionalField(StringField);
        set => SetAdditionalField(value);
    }

    public string? SortAlbumArtist
    {
        get => GetAdditionalField(StringField);
        set => SetAdditionalField(value);
    }

    public string? SortArtist
    {
        get => GetAdditionalField(StringField);
        set => SetAdditionalField(value);
    }

    public string? EncodedBy
    {
        get => GetAdditionalField(StringField);
        set => SetAdditionalField(value);
    }

    public string? EncoderSettings
    {
        get => GetAdditionalField(StringField);
        set => SetAdditionalField(value);
    }

    public string? EncodingTool
    {
        get => GetAdditionalField(StringField);
        set => SetAdditionalField(value);
    }

    public string? MovementName
    {
        get => GetAdditionalField(StringField);
        set => SetAdditionalField(value);
    }

    public string? Movement
    {
        get => GetAdditionalField(StringField);
        set => SetAdditionalField(value);
    }

    public string? LongDescription
    {
        get => GetAdditionalField(StringField);
        set => SetAdditionalField(value);
    }

    public string? Subtitle
    {
        get => GetAdditionalField(StringField);
        set => SetAdditionalField(value);
    }

    public string? SortTitle
    {
        get => GetAdditionalField(StringField);
        set => SetAdditionalField(value);
    }

    public string? Narrator
    {
        get => GetAdditionalField(StringField);
        set => SetAdditionalField(value);
    }

    public DateTime? PurchaseDate
    {
        get => GetAdditionalField(DateTimeField);
        set => SetAdditionalField(value);
    }

    public ItunesMediaType? ItunesMediaType
    {
        get => GetAdditionalField(EnumField<ItunesMediaType?>);
        set => SetAdditionalField(value);
    }

    public ItunesGapless? ItunesGapless
    {
        get => GetAdditionalField(EnumField<ItunesGapless?>);
        set => SetAdditionalField(value);
    }


    public MetadataTrack()
    {
        // https://pastebin.com/DQTZFE6H
        // possible improvements for the future:
        
        // BeatsPerMinute (BPM)
        // Compilation (COMPILATION)
        // SortComposer (COMPOSERSORT) 
        // ...
    }

    public MetadataTrack(string path, IProgress<float>? writeProgress = null, bool load = true)
        : base(path, writeProgress, load)
    {
    }

    public MetadataTrack(IFileSystemInfo fileInfo, IProgress<float>? writeProgress = null, bool load = true)
        : base(fileInfo.FullName, writeProgress, load)
    {
    }


    private static string? StringField(string? value) => value;

    // private static int? IntField(string? value) => int.TryParse(value, out var result) ? result : default;
    private static DateTime? DateTimeField(string? value) =>
        DateTime.TryParse(value, out var result) ? result : default;

    private static T? EnumField<T>(string? value) =>
        Enum.TryParse(typeof(T), value, out var result) && result != null ? (T)result : default;

    private T? GetAdditionalField<T>(Func<string, T?> converter, [CallerMemberName] string key = "")
    {
        var mappedKey = MapAdditionalField(AudioFormat, key);
        if (mappedKey == "" || !AdditionalFields.ContainsKey(mappedKey) || AdditionalFields[mappedKey] == null)
        {
            return default;
        }

        return converter(AdditionalFields[mappedKey]);
    }

    private void SetAdditionalField<T>(T? value, [CallerMemberName] string key = "")
    {
        var mappedKey = MapAdditionalField(AudioFormat, key);
        if (mappedKey == "")
        {
            return;
        }

        if (value == null)
        {
            if (AdditionalFields.ContainsKey(mappedKey))
            {
                AdditionalFields.Remove(mappedKey);
            }

            return;
        }

        AdditionalFields[mappedKey] = value switch
        {
            DateTime d => d.ToString("yyyy/MM/dd"),
            _ => value.ToString() ?? ""
        };
    }


    private static string MapAdditionalField(Format format, string key) => format.ID switch
    {
        // Format is not really appropriate, because it should be the TaggingFormat (id3v2, etc.), not the audio file format
        // TODO: Map others
        AudioDataIOFactory.CID_MP3 => TagMapping.ContainsKey(key) ? TagMapping[key].ID3v23 : "",
        AudioDataIOFactory.CID_MP4 => TagMapping.ContainsKey(key) ? TagMapping[key].MP4 : "",
        _ => ""
    };
}