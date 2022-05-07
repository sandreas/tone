using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.CompilerServices;
using ATL;
using ATL.AudioData;

namespace tone.Metadata;

public class MetadataTrack : Track, IMetadata
{
    // meet IMetadata interface requirements
    public new string? Path => base.Path;

    public DateTime? RecordingDate
    {
        get => Date;
        set => Date = value;
    }

    // be able to set totalDuration, if it has not been detected or its a dummy track
    private TimeSpan? _totalDuration;

    public TimeSpan TotalDuration
    {
        get => _totalDuration ?? TimeSpan.FromMilliseconds(DurationMs);
        set => _totalDuration = value;
    }

    
    

    public IDictionary<string, string> MappedAdditionalFields => AdditionalFields
        .Where(kvp => IsAdditionalFieldKeyMapped(AudioFormat, kvp.Key))
        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    /*
    private static readonly List<(string, Action<MetadataTrack>)> RemoveFieldCallbacks = new()
    {
        (nameof(Bpm), track => track.Bpm = null),
        (nameof(EncodedBy), track => track.EncodedBy = null),
        (nameof(EncoderSettings), track => track.EncoderSettings = null),
        (nameof(Subtitle), track => track.Subtitle = null),
        (nameof(ItunesCompilation), track => track.ItunesCompilation = null),
        (nameof(ItunesMediaType), track => track.ItunesMediaType = null),
        (nameof(ItunesPlayGap), track => track.ItunesPlayGap = null),
        (nameof(DiscNumber), track => track.DiscNumber = null),
        (nameof(DiscTotal), track => track.DiscTotal = null),
        (nameof(TrackNumber), track => track.TrackNumber = null),
        (nameof(TrackTotal), track => track.TrackTotal = null),
        (nameof(Popularity), track => track.Popularity = null),
        (nameof(Title), track => track.Title = null),
        (nameof(Artist), track => track.Artist = null),
        (nameof(Composer), track => track.Composer = null),
        (nameof(Comment), track => track.Comment = null),
        (nameof(Genre), track => track.Genre = null),
        (nameof(Album), track => track.Album = null),
        (nameof(OriginalAlbum), track => track.OriginalAlbum = null),
        (nameof(OriginalArtist), track => track.OriginalArtist = null),
        (nameof(Copyright), track => track.Copyright = null),
        (nameof(Description), track => track.Description = null),
        (nameof(Publisher), track => track.Publisher = null),
        (nameof(AlbumArtist), track => track.AlbumArtist = null),
        (nameof(Conductor), track => track.Conductor = null),
        (nameof(Group), track => track.Group = null),
        (nameof(SortTitle), track => track.SortTitle = null),
        (nameof(SortAlbum), track => track.SortAlbum = null),
        (nameof(SortArtist), track => track.SortArtist = null),
        (nameof(SortAlbumArtist), track => track.SortAlbumArtist = null),
        (nameof(SortComposer), track => track.SortComposer = null),
        (nameof(LongDescription), track => track.LongDescription = null),
        (nameof(EncodingTool), track => track.EncodingTool = null),
        (nameof(ChaptersTableDescription), track => track.ChaptersTableDescription = null),
        (nameof(Narrator), track => track.Narrator = null),
        (nameof(MovementName), track => track.MovementName = null),
        (nameof(Movement), track => track.Movement = null),
        (nameof(PublishingDate), track => track.PublishingDate = null),
        (nameof(RecordingDate), track => track.RecordingDate = null),
        (nameof(PurchaseDate), track => track.PurchaseDate = null),
        (nameof(Lyrics), track => track.Lyrics = null),
        (nameof(Chapters), track => track.Chapters.Clear()),
        (nameof(EmbeddedPictures), track => track.EmbeddedPictures.Clear()),
    };
    */
    private static Dictionary<string, (string ID3v23, string ID3v24, string MP4, string Matroska, string
        ASF_WindowsMedia, string RiffInfo)> TagMapping { get; } = new()
    {
        // ("ID3v2.3","ID3v2.4","MP4","Matroska","ASF/Windows Media","RIFF INFO")
        { nameof(Bpm), ("TBPM", "TBPM", "tmpo", "T=30", "WM/BeatsPerMinute", "") },
        { nameof(EncodedBy), ("TENC", "TENC", "", "T=30 ENCODED_BY", "WM/EncodedBy", "") },
        { nameof(EncoderSettings), ("TSSE", "TSSE", "©enc", "T=30", "WM/EncodingSettings", "") },
        { nameof(EncodingTool), ("", "", "©too", "", "WM/ToolName", "") },
        { nameof(Group), ("TIT1", "TIT1", "©grp", "T=30", "WM/ArtistSortOrder", "") },
        { nameof(ItunesCompilation), ("TCMP", "TCMP", "cpil", "T=30", "", "") },
        { nameof(ItunesMediaType), ("", "", "stik", "", "", "") },
        { nameof(ItunesPlayGap), ("", "", "pgap", "", "", "") },
        { nameof(LongDescription), ("TDES", "TDES", "ldes", "T=30", "", "") },
        { nameof(Part), ("TXXX:PART", "TXXX:PART", "----:com.pilabor.tone:PART", "T=20 PART_NUMBER", "", "") },
        { nameof(Movement), ("MVIN", "MVIN", "©mvi", "T=20 PART_NUMBER", "", "") },
        { nameof(MovementName), ("MVNM", "MVNM", "©mvn", "T=20 TITLE", "", "") },
        // {nameof(MovementTotal), ("MVIN","MVIN","©mvc","T=30","","")}, // special case: MVIN has to be appended, not replaced
        { nameof(Narrator), ("", "", "©nrt", "T=30", "", "") },
        { nameof(PurchaseDate), ("", "", "purd", "", "", "") },
        { nameof(SortAlbum), ("TSOA", "TSOA", "soal", "T=50 SORT_WITH", "WM/AlbumSortOrder", "") },
        { nameof(SortAlbumArtist), ("TSO2", "TSO2", "soaa", "T=30", "", "") },
        { nameof(SortArtist), ("TSOP", "TSOP", "soar", "T=30", "WM/ArtistSortOrder", "") },
        { nameof(SortComposer), ("TSOC", "TSOC", "soco", "T=30", "", "") },
        { nameof(SortTitle), ("TSOT", "TSOT", "sonm", "T=30 SORT_WITH", "WM/TitleSortOrder", "") },
        /*mp4 => ©st3 ? */
        { nameof(Subtitle), ("TIT3", "TIT3", "----:com.apple.iTunes:SUBTITLE", "T=30", "WM/SubTitle", "") },
    };

    // todo: for fields that are combined multiple values
    // private Dictionary<string, string> TagMappingValues = new();
    //
    // // idea: MVIN, () => { return $"{Movement}/{MovementTotal}"}, (additionalFieldValue) => {additionalFieldValue.Split("/")...}
    // private static Dictionary<string, Func<string>> TagMappingBuilders = new()
    // {
    //
    // };

    public int? Bpm
    {
        get => GetAdditionalField(IntField);
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

    public string? Group
    {
        get => GetAdditionalField(StringField);
        set => SetAdditionalField(value);
    }

    public ItunesCompilation? ItunesCompilation
    {
        get => HasAdditionalField() ? GetAdditionalField(EnumField<ItunesCompilation>) : null;
        set => SetAdditionalField(value);
    }

    public ItunesMediaType? ItunesMediaType
    {
        get => HasAdditionalField() ? GetAdditionalField(EnumField<ItunesMediaType>) : null;
        set => SetAdditionalField(value);
    }

    public ItunesPlayGap? ItunesPlayGap
    {
        get => HasAdditionalField() ? GetAdditionalField(EnumField<ItunesPlayGap>) : null;
        set => SetAdditionalField(value);
    }

    public string? LongDescription
    {
        get => GetAdditionalField(StringField);
        set => SetAdditionalField(value);
    }

    public string? Movement
    {
        get => GetAdditionalField(StringField);
        set => SetAdditionalField(value);
    }
    
    public string? Part
    {
        get => GetAdditionalField(StringField) ?? Movement;
        set
        {
            SetAdditionalField(value);
            Movement = value;
        }
    }

    public string? MovementName
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

    public string? SortComposer
    {
        get => GetAdditionalField(StringField);
        set => SetAdditionalField(value);
    }

    public string? SortTitle
    {
        get => GetAdditionalField(StringField);
        set => SetAdditionalField(value);
    }


    public string? Subtitle
    {
        get => GetAdditionalField(StringField);
        set => SetAdditionalField(value);
    }


    public MetadataTrack()
    {
        InitMetadataTrack();
        // https://pastebin.com/DQTZFE6H

        // json spec for tag field mapping
        // Fields => (preferredField,writeTemplate,parseCallback), (alternate1, template, parseCallback), (alternate2,template)

        // possible improvements for the future (unclear/unspecified mapping):
        // ACOUSTID_ID
        // ACOUSTID_FINGERPRINT
        // BARCODE
        // CATALOGNUMBER
        // INITIALKEY
        // INVOLVEDPEOPLE
        // MUSICBRAINZ_*
        // PODCAST*

        // ffmpeg: https://kodi.wiki/view/Video_file_tagging
        // author => ©aut

        // Remove is indeed to remove an entire tag type (e.g. ID3v2 or APE) from a file
        // If you want to remove a field, just assign an empty value "" to it and save
    }

    public MetadataTrack(string path, IProgress<float>? writeProgress = null, bool load = true)
        : base(path, writeProgress, load)
    {
        InitMetadataTrack();
    }

    public MetadataTrack(IFileSystemInfo fileInfo, IProgress<float>? writeProgress = null, bool load = true)
        : base(fileInfo.FullName, writeProgress, load)
    {
        InitMetadataTrack();
    }

    private void InitMetadataTrack()
    {
        Chapters ??= new List<ChapterInfo>();
        AdditionalFields ??= new Dictionary<string, string>();
    }

    private static string? StringField(string? value) => value;
    private static int? IntField(string? value) => int.TryParse(value, out var result) ? result : null;

    private static DateTime? DateTimeField(string? value) =>
        DateTime.TryParse(value, out var result) ? result : null;

    private static T? EnumField<T>(string? value) where T : Enum?
    {
        if (string.IsNullOrEmpty(value))
        {
            return default;
        }

        if (int.TryParse(value, out var enumValueInt) && Enum.IsDefined(typeof(T), enumValueInt))
        {
            return (T)Enum.ToObject(typeof(T), enumValueInt);
        }

        if (Enum.TryParse(typeof(T), value, out var enumValue) && enumValue != null)
        {
            return (T)enumValue;
        }

        return default;
    }


    private bool HasAdditionalField([CallerMemberName] string key = "")
    {
        var mappedKey = MapAdditionalField(AudioFormat, key);
        return mappedKey != "" && AdditionalFields.ContainsKey(mappedKey) && AdditionalFields[mappedKey] != null;
    }

    private T? GetAdditionalField<T>(Func<string?, T?> converter, [CallerMemberName] string key = "")
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

        // ©mvi MUST contain an integer value, which leads to an exception, if a string like 1.5 is stored
        if (mappedKey == "©mvi" && value is string v && !int.TryParse(v, out _))
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

        /*
         d.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss").Replace(" 00:00:00", "")
For ID3v2.3 and older, you'd combine the TYER tag (YYYY) with the TDAT tag (MMDD) and TIME tag (HHMM).
For ID3v2.4, you'd use TDRC or TDRA (or any of the other timestamp frames), with any level of accuracy you want, up to: YYYYMMDDTHHMMSS. Include Year, throw in month, throw in day, add the literal T and throw in hour, minute, second.
Vorbis: ISO8601 
         */
        // var formatString = AudioFormat.ID switch
        // {
        //     AudioDataIOFactory.CID_MP3
        // };
        AdditionalFields[mappedKey] = value switch
        {
            DateTime d => d.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss").Replace(" 00:00:00", ""),
            Enum e => ((int)(object)e).ToString(), // cast enum to int
            _ => value.ToString() ?? ""
        };

        // todo store / remove original value unmapped for 
        // TagMappingValues[key] = AdditionalFields[mappedKey];
    }


    private static string MapAdditionalField(Format? format, string key) => format?.ID switch
    {
        // Format is not really appropriate, because it should be the TaggingFormat (id3v2, etc.), not the audio file format
        // TODO: Map others
        AudioDataIOFactory.CID_MP3 => TagMapping.ContainsKey(key) ? TagMapping[key].ID3v23 : "",
        AudioDataIOFactory.CID_MP4 => TagMapping.ContainsKey(key) ? TagMapping[key].MP4 : "",
        _ => ""
    };

    private static bool IsAdditionalFieldKeyMapped(Format format, string fieldName)
    {
        foreach (var (_, tuple) in TagMapping)
        {
            var fieldNameToCheck = format.ID switch
            {
                AudioDataIOFactory.CID_MP3 => tuple.ID3v23,
                AudioDataIOFactory.CID_MP4 => tuple.MP4,
                _ => ""
            };

            if (fieldNameToCheck == fieldName)
            {
                return true;
            }
        }

        return false;
    }

    /*
    public string[] RemoveFields(params string[] fieldNames)
    {
        var lcFieldNames = fieldNames.Select(f => f.ToLowerInvariant()).ToArray();
        var removedFieldNames = new Stack<string>();

        foreach (var (fieldName, callback) in RemoveFieldCallbacks)
        {
            if (!lcFieldNames.Contains(fieldName.ToLowerInvariant()))
            {
                continue;
            }

            callback(this);
            removedFieldNames.Push(fieldName);
        }

        return removedFieldNames.ToArray();
    }
    */



}