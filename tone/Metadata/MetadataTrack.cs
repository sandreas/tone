using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.CompilerServices;
using ATL;

namespace tone.Metadata;

public class MetadataTrack : Track, IMetadata
{
    // meet IMetadata interface requirements
    public new string? Path => base.Path;

    private readonly MetadataSpecification _manualMetadataSpecification = MetadataSpecification.Undefined;
    public MetadataSpecification[] MetadataSpecifications => MetadataFormats
        .Select(AtlFileFormatToMetadataFormat)
        .Concat(new []{_manualMetadataSpecification})
    .Where(tagType => tagType != MetadataSpecification.Undefined)
    .ToArray();
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
        .Where(kvp =>
        {
            var (key, _) = kvp;
            return MetadataSpecifications.Any(spec => spec switch
            {
                MetadataSpecification.Id3V23 => TagMapping.Any(t => t.Value.ID3v23 == key),
                MetadataSpecification.Id3V24 => TagMapping.Any(t => t.Value.ID3v24 == key),
                MetadataSpecification.Mp4 => TagMapping.Any(t => t.Value.Mp4 == key),
                MetadataSpecification.Matroska => TagMapping.Any(t => t.Value.Matroska == key),
                MetadataSpecification.WindowsMediaAsf => TagMapping.Any(t => t.Value.WindowsMediaAsf == key),
                MetadataSpecification.Ape => TagMapping.Any(t => t.Value.Ape == key),
                MetadataSpecification.Vorbis => TagMapping.Any(t => t.Value.Vorbis == key),
                _ => false
            });
        })
        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    private static Dictionary<string, (string ID3v23, string ID3v24, string Mp4, string Matroska, string WindowsMediaAsf, string Ape, string Vorbis, string RiffInfo)> TagMapping { get; } = new()
    {
        // ("ID3v2.3","ID3v2.4","MP4","Matroska","ASF/Windows Media", "APE" "RIFF INFO")
        { nameof(Bpm), ("TBPM", "TBPM", "tmpo", "T=30", "WM/BeatsPerMinute", "BPM", "BPM", "") },
        { nameof(EncodedBy), ("TENC", "TENC", "", "T=30 ENCODED_BY", "WM/EncodedBy", "EncodedBy", "ENCODED-BY", "") },
        { nameof(EncoderSettings), ("TSSE", "TSSE", "©enc", "T=30", "WM/EncodingSettings", "", "ENCODER SETTINGS", "") },
        { nameof(EncodingTool), ("", "", "©too", "", "WM/ToolName", "", "ENCODER", "") },
        { nameof(Group), ("TIT1", "TIT1", "©grp", "T=30", "WM/ArtistSortOrder", "Grouping", "GROUPING", "") },
        { nameof(ItunesCompilation), ("TCMP", "TCMP", "cpil", "T=30", "", "Compilation", "COMPILATION", "") },
        { nameof(ItunesMediaType), ("", "", "stik", "", "", "", "", "") },
        { nameof(ItunesPlayGap), ("", "", "pgap", "", "", "", "", "") },
        { nameof(LongDescription), ("TDES", "TDES", "ldes", "T=30", "", "", "", "") },
        { nameof(Part), ("TXXX:PART", "TXXX:PART", "----:com.pilabor.tone:PART", "T=20 PART_NUMBER", "", "", "PARTNUMBER", "") },
        { nameof(Movement), ("MVIN", "MVIN", "©mvi", "T=20 PART_NUMBER", "", "MOVEMENT", "MOVEMENT", "") },
        { nameof(MovementName), ("MVNM", "MVNM", "©mvn", "T=20 TITLE", "", "MOVEMENTNAME", "MOVEMENTNAME", "") },
        // {nameof(MovementTotal), ("MVIN","MVIN","©mvc","T=30","","")}, // special case: MVIN has to be appended, not replaced
        { nameof(Narrator), ("", "", "©nrt", "T=30", "", "", "", "") },
        { nameof(PurchaseDate), ("", "", "purd", "", "", "", "", "") },
        { nameof(SortAlbum), ("TSOA", "TSOA", "soal", "T=50 SORT_WITH", "WM/AlbumSortOrder", "ALBUMSORT", "ALBUMSORT", "") },
        { nameof(SortAlbumArtist), ("TSO2", "TSO2", "soaa", "T=30", "", "ALBUMARTISTSORT", "ALBUMARTISTSORT", "") },
        { nameof(SortArtist), ("TSOP", "TSOP", "soar", "T=30", "WM/ArtistSortOrder", "ARTISTSORT", "ARTISTSORT", "") },
        { nameof(SortComposer), ("TSOC", "TSOC", "soco", "T=30", "", "", "", "") },
        { nameof(SortTitle), ("TSOT", "TSOT", "sonm", "T=30 SORT_WITH", "WM/TitleSortOrder", "TITLESORT", "TITLESORT", "") },
        { nameof(Subtitle), ("TIT3", "TIT3", "----:com.apple.iTunes:SUBTITLE", "T=30", "WM/SubTitle", "Subtitle", "SUBTITLE", "") },
        /*mp4 => ©st3 ? */
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


    public MetadataTrack(MetadataSpecification type = MetadataSpecification.Undefined)
    {
        _manualMetadataSpecification = type;
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
        return GetFirstValuedKey(key) != "";
    }

    private string GetFirstValuedKey(string key = "")
    {
        foreach (var spec in MetadataSpecifications)
        {
            var mappedKey = MapAdditionalFieldKey(spec, key);
            if (mappedKey != "" && AdditionalFields.ContainsKey(mappedKey) && AdditionalFields[mappedKey] != null)
            {
                return mappedKey;
            }
        }
        return "";
    }
    
    private T? GetAdditionalField<T>(Func<string?, T?> converter, [CallerMemberName] string key = "")
    {
        var mappedKey = GetFirstValuedKey(key);
        return mappedKey == "" ? default : converter(AdditionalFields[mappedKey]);
    }

    private void SetAdditionalField<T>(T? value, [CallerMemberName] string key = "")
    {
        // movement MUST contain an integer value, which leads to an exception, if a string like 1.5 is stored
        // to store a non-integer value, use Part instead
        if (key == nameof(Movement) && value is string v && !int.TryParse(v, out _))
        {
            return;
        }
        
        foreach (var spec in MetadataSpecifications)
        {
            var mappedKey = MapAdditionalFieldKey(spec, key);
            if (mappedKey == "")
            {
                continue;
            }

            if (value == null)
            {
                if (AdditionalFields.ContainsKey(mappedKey))
                {
                    AdditionalFields.Remove(mappedKey);
                }
                continue;
            }
            

            AdditionalFields[mappedKey] = value switch
            {
                DateTime d => FormatDate(spec, d),
                Enum e => ((int)(object)e).ToString(), // cast enum to int
                _ => value.ToString() ?? ""
            };
        }
    }

    private static string FormatDate(MetadataSpecification spec, DateTime date) => spec switch
    {
        /*
d.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss").Replace(" 00:00:00", "")
For ID3v2.3 and older, you'd combine the TYER tag (YYYY) with the TDAT tag (MMDD) and TIME tag (HHMM).
For ID3v2.4, you'd use TDRC or TDRA (or any of the other timestamp frames), with any level of accuracy you want, up to: YYYYMMDDTHHMMSS. Include Year, throw in month, throw in day, add the literal T and throw in hour, minute, second.
Vorbis: ISO8601 
*/
        MetadataSpecification.Vorbis => date.ToString("yyyy-MM-ddTHH:mm:ssZ"),
        _ => date.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss").Replace(" 00:00:00", "")
    };
    
    private static MetadataSpecification AtlFileFormatToMetadataFormat(Format format) => format.ShortName.ToLower() switch
    {
        "native" => AtlNativeFileFormatToMetadataFormat(format),
        "id3v1" => MetadataSpecification.Id3V1,
        "id3v2" => MetadataSpecification.Id3V23, // todo: find out about id3v24
        "ape" => MetadataSpecification.Ape,
        _ => MetadataSpecification.Undefined
    };

    private static MetadataSpecification AtlNativeFileFormatToMetadataFormat(Format format)
    {
        // todo: Add more
        return format.Name.Contains("MPEG-4") ? MetadataSpecification.Mp4 : MetadataSpecification.Undefined;
    }

    private static string MapAdditionalFieldKey(MetadataSpecification format, string key) => format switch
    {
        // ignored atm: MetadataSpecification.Id3v1 => TagMapping.ContainsKey(key) ? TagMapping[key].Id3v1 : "",
        MetadataSpecification.Id3V23 => TagMapping.ContainsKey(key) ? TagMapping[key].ID3v23 : "",
        MetadataSpecification.Id3V24 => TagMapping.ContainsKey(key) ? TagMapping[key].ID3v24 : "",
        MetadataSpecification.Mp4 => TagMapping.ContainsKey(key) ? TagMapping[key].Mp4 : "",
        MetadataSpecification.WindowsMediaAsf => TagMapping.ContainsKey(key) ? TagMapping[key].WindowsMediaAsf : "",
        MetadataSpecification.Matroska => TagMapping.ContainsKey(key) ? TagMapping[key].Matroska : "",
        MetadataSpecification.Ape => TagMapping.ContainsKey(key) ? TagMapping[key].Ape : "",
        _ => ""
    };
}