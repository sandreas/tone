using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.CompilerServices;
using ATL;
using ATL.AudioData;

namespace tone.Metadata;


/*
Group
Subtitle
SortAlbum
ItunesMediaType (stik, =2)
ItunesGapless (pgap, =1)
ShowMovement (shwm, if Series then = 1 else blank)
MovementName (MVNM, SeriesTitle)
Movement (MVIN, SeriesPart)
TXXX (SERIES)** 	SeriesTitle
TXXX (SERIES-PART)** 	SeriesPart
TXXX (TMP_GENRE1)** 	Genre 1
TXXX (TMP_GENRE2)** 	Genre 2
TIT2 (TITLE) 	Not Scraped, but used for Chapter Title If no chapter data available set to filename
 */

// https://wiki.hydrogenaud.io/index.php?title=Tag_Mapping
// https://help.mp3tag.de/main_tags.html
// https://github.com/seanap/Plex-Audiobook-Guide#tags-that-are-being-set

// todo:
// - write series to group: TIT1 (CONTENTGROUP)
/*
TPE2 (ALBUMARTIST) 	Author
TIT1 (CONTENTGROUP) 	Series, Book #
TIT3 (SUBTITLE) 	Subtitle
COMM (COMMENT) 	Publisher's Summary (MP3)
TSOA (ALBUMSORT) 	If ALBUM only, then %Title%, If ALBUM and SUBTITLE, then %Title% - %Subtitle%, If Series, then %Series% %Series-part% - %Title%
stik (ITUNESMEDIATYPE) 	M4B Media type = Audiobook (Normal, Audiobook, Music Video, Movie, TV Show, Booklet)
pgap (ITUNESGAPLESS) 	M4B Gapless album = 1
shwm (SHOWMOVEMENT) 	Show Movement (M4B), if Series then = 1 else blank
MVNM (MOVEMENTNAME) 	Series
MVIN (MOVEMENT) 	Series Book #
TXXX (SERIES)** 	Series
TXXX (SERIES-PART)** 	Series Book #
TXXX (TMP_GENRE1)** 	Genre 1
TXXX (TMP_GENRE2)** 	Genre 2
TIT2 (TITLE) 	Not Scraped, but used for Chapter Title - If no chapter data available set to filename

ASIN (ASIN) 	Amazon Standard Identification Number
POPM (RATING WMP) 	Audible Rating
WOAF (WWWAUDIOFILE) 	Audible Album URL
 */
/*
TIT1 (CONTENTGROUP) 	Series, Book #
TALB (ALBUM) 	Title
TIT3 (SUBTITLE) 	Subtitle
TPE1 (ARTIST) 	Author, Narrator
TPE2 (ALBUMARTIST) 	Author
TCOM (COMPOSER) 	Narrator
TCON (GENRE) 	Genre1/Genre2
TYER (YEAR) 	Copyright Year*
COMM (COMMENT) 	Publisher's Summary (MP3)
desc (DESCRIPTION) 	Publisher's Summary (M4B)
TSOA (ALBUMSORT) 	If ALBUM only, then %Title%, If ALBUM and SUBTITLE, then %Title% - %Subtitle%, If Series, then %Series% %Series-part% - %Title%
TDRL (RELEASETIME) 	Audiobook Release Year
TPUB (PUBLISHER) 	Publisher
TCOP (COPYRIGHT) 	Copyright
ASIN (ASIN) 	Amazon Standard Identification Number
POPM (RATING WMP) 	Audible Rating
WOAF (WWWAUDIOFILE) 	Audible Album URL
stik (ITUNESMEDIATYPE) 	M4B Media type = Audiobook
pgap (ITUNESGAPLESS) 	M4B Gapless album = 1
shwm (SHOWMOVEMENT) 	Show Movement (M4B), if Series then = 1 else blank
MVNM (MOVEMENTNAME) 	Series
MVIN (MOVEMENT) 	Series Book #
TXXX (SERIES)** 	Series
TXXX (SERIES-PART)** 	Series Book #
TXXX (TMP_GENRE1)** 	Genre 1
TXXX (TMP_GENRE2)** 	Genre 2
CoverUrl 	Album Cover Art
TIT2 (TITLE) 	Not Scraped, but used for Chapter Title - If no chapter data available set to filename
 */

enum MappingKey
{
    Group,
    Subtitle,
    SortName,
    SortAlbum,
    SortArtist,
    SortAlbumArtist,
    LongDescription,
    EncodedBy,
    EncodingTool,
    PurchaseDate,
    MediaType,
    Narrator,
    SeriesTitle,
    SeriesPart
}

enum FormatKey
{
    Mp4,
    Mp3,
}

/*
0 = Movie (old)
1 = Normal (Music)
2= Audiobook
6= Music Video
9 = Movie
10 = TV Show
11 = Booklet
14 = Ringtone
23 = iTunes U
*/
enum ItunesMediaType
{
    MovieOld = 0,
    Normal = 1,
    Audiobook = 2,
    MusicVideo = 6,
    Movie = 9,
    TvShow = 10,
    Booklet = 11,
    Ringtone = 14,
    ItunesU = 23,
}
/*
class Track {

    private Format _format { get; set; }
    public IDictionary<string, string> AdditionalFields { get; set; }

    public string? Group
    {
        get => GetAdditionalField(_format);
        set => SetAdditionalField(_format, value); // if _format == MP4, key for Group = ©nam
    }

    private void SetAdditionalField<T>(Format format, T? value, [CallerMemberName] string key=null)
    {
        var mappedKey = MapAdditionalField(format, key);
        if (mappedKey == null)
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
        AdditionalFields[mappedKey] = Stringify(value);
    }

    private string Stringify<T>(T value)
    {
        if (value is string s)
        {
            return s;
        }

        if (value is DateTime d)
        {
            return d.ToString();
        }

        if (value is int i)
        {
            return i.ToString();
        }
        return "";
    }

    private string? GetAdditionalField(Format format, [CallerMemberName] string key=null)
    {
        var mappedKey = MapAdditionalField(format, key);
        if (mappedKey == null)
        {
            return null;
        }
        return AdditionalFields.ContainsKey(mappedKey) ? AdditionalFields[mappedKey] : null;
    }

    // this is new
    public string? MapAdditionalField(Format format, string key) => format.ID switch
    {
        // Format is not really appropriate, because it should be the TaggingFormat (id3v2, etc.), not the audio file format
        AudioDataIOFactory.CID_MP3 => MapMp3(key),
        AudioDataIOFactory.CID_MP4 => MapMp4(key),
        _ => null
    };
    
    private static string? MapMp3(string key) => key switch
    {
        nameof(Group)    => "TIT1",
        _ => null,
    };
    
    private static string? MapMp4(string key) => key switch
    {
        nameof(Group)    => "©grp",
        _ => null,
    };
    // ...
}
*/
public class MetadataTrack : Track, IMetadata
{
    public new string? Path => base.Path;

    public static Dictionary<string, (string id3v23, string id3v24, string mp4)> TagMapping { get; set; } = new()
    {
        {nameof(Group), ("TIT1", "TIT1", "grp")},
        /*

    ALBUMSORT (SortAlbum): ["TSOA","TSOA","soal","T=50 SORT_WITH","WM/AlbumSortOrder",""]
    ALBUMARTISTSORT (SortAlbumArtist): ["TSO2","TSO2","soaa","T=30","",""]
    ARTISTSORT (SortArtist): ["TSOP","TSOP","soar","T=30","WM/ArtistSortOrder",""]
    CONTENTGROUP (Group): ["TIT1","TIT1","©grp","T=30","WM/ContentGroupDescription",""]
    ENCODEDBY (EncodedBy): ["TENC","TENC","","T=30 ENCODED_BY","WM/EncodedBy",""]
    ENCODERSETTINGS (EncoderSettings): ["TSSE","TSSE","©enc","T=30","WM/EncodingSettings",""]
    MOVEMENTNAME (SeriesTitle): ["MVNM","MVNM","©mvn","T=20 TITLE","",""]
    MOVEMENT (SeriesPart): ["MVIN","MVIN","©mvi","T=20 PART_NUMBER","",""]
    MOVEMENTTOTAL (SeriesPartsTotal): ["MVIN","MVIN","©mvc","T=30","",""]
    PODCASTDESC (LongDescription): ["TDES","TDES","ldes","T=30","",""]
    SUBTITLE (Subtitle): ["TIT3","TIT3","----:com.apple.iTunes:SUBTITLE","T=30","WM/SubTitle",""]
    TITLESORT (SortTitle): ["TSOT","TSOT","sonm","T=30 SORT_WITH","WM/TitleSortOrder",""]

         */
    };
    
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

    public string? Group
    {
        get => GetAdditionalField(StringField); // todo: if not string, convert GetAdditionalField<DateTime>();
        set => SetAdditionalField(value); // if _format == MP4, key for Group = ©nam
    }

    private static string? StringField(string? value) => value;
    private static int? IntField(string? value) => int.TryParse(value, out var result) ? result : default;
    
    private T? GetAdditionalField<T>(Func<string, T?> converter, [CallerMemberName] string key = "")
    {
        var mappedKey = MapAdditionalField(AudioFormat, key);
        if (mappedKey == null || !AdditionalFields.ContainsKey(mappedKey) || AdditionalFields[mappedKey] == null)
        {
            return default;
        }
        return converter(AdditionalFields[mappedKey]);
    }

    
    private void SetAdditionalField<T>(T? value, [CallerMemberName] string key="")
    {
        var mappedKey = MapAdditionalField(AudioFormat, key);
        if (mappedKey == null)
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
            DateTime d => DateTimeAsString(d, AudioFormat),
            _ => value.ToString()??""
        };
    }
    
    private static string DateTimeAsString(DateTime value, Format format)
    {
        return value.ToString("yyyy/MM/dd");
    }


    // private T? GetAdditionalField<T>(T field, [CallerMemberName] string key="")
    // {
    //     var mappedKey = MapAdditionalField(AudioFormat, key);
    //     if (mappedKey == null || !AdditionalFields.ContainsKey(mappedKey) || AdditionalFields[mappedKey] == null)
    //     {
    //         return default;
    //     }
    //
    //     return field switch
    //     {
    //         string s => GetAsString()
    //     };
        /*
        try
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            return (T)converter.ConvertFromString(AdditionalFields[mappedKey])!;
        }
        catch (NotSupportedException)
        {
            return default;
        }
        */
        
        
        
        
        // if (typeof(T) == typeof(string))
        // {
        //     return (T)Convert.ChangeType(AdditionalFields[mappedKey], typeof(T));
        // }
        //
        // if (typeof(T) == typeof(DateTime))
        // {
        //     return (T)Convert.ChangeType(TryParse<DateTime>(AdditionalFields[mappedKey], DateTime.TryParse), typeof(DateTime));
        // }
        
        
        /*
        if (typeof(T) == typeof(DateTime))
        {
            if (DateTime.TryParse(AdditionalFields[mappedKey], out var value))
            {
                return (T)Convert.ChangeType(value, typeof(T));                
            }

            return default;
        }
        
        
        if (typeof(T) == typeof(int))
        {
            if (int.TryParse(AdditionalFields[mappedKey], out var value))
            {
                return (T)Convert.ChangeType(value, typeof(T));                
            }

            return default;
        }
        return default;
        */
    // }
    
    // delegate is required because out is not part of the Func spec (see https://stackoverflow.com/questions/1283127/funct-with-out-parameter)
    // public delegate bool TryParseHandler<T>(string value, out T result);
    // public static T? TryParse<T>(string value, TryParseHandler<T> handler) where T : struct
    // {
    //     return handler(value, out var result) ? result : default;
    // }
    /*
    public static T Convert<T>(string? input)
    {
        try
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            if(converter != null)
            {
                // Cast ConvertFromString(string text) : object to (T)
                return (T)converter.ConvertFromString(input);
            }
            return default(T);
        }
        catch (NotSupportedException)
        {
            return default(T);
        }
    }
    */
    


    private static string? MapAdditionalField(Format format, string key) => format.ID switch
    {
        // Format is not really appropriate, because it should be the TaggingFormat (id3v2, etc.), not the audio file format
        AudioDataIOFactory.CID_MP3 => MapId3MetadataField(key),
        AudioDataIOFactory.CID_MP4 => MapMp4MetadataField(key),
        _ => null
    };
    


    private static string? MapId3MetadataField(string key) => key switch
    {
        nameof(Group)    => "TIT1",
        _ => null,
    };
    
    private static string? MapMp4MetadataField(string key) => key switch
    {
        nameof(Group)    => "©grp",
        _ => null,
    };
    
    
    
    
    
    private string? _subtitle = "";

    public string? Subtitle
    {
        get => _subtitle;
        set => SetValue(ref _subtitle, value, nameof(Subtitle));
    }

   

    private string? _sortTitle = "";

    public string? SortTitle
    {
        get => _sortTitle;
        set => SetValue(ref _sortTitle, value, nameof(SortTitle));
    }

    private string? _sortAlbum = "";

    public string? SortAlbum
    {
        get => _sortAlbum;
        set => SetValue(ref _sortAlbum, value, nameof(SortAlbum));
    }

    private string? _sortArtist = "";

    public string? SortArtist
    {
        get => _sortArtist;
        set => SetValue(ref _sortArtist, value, nameof(SortArtist));
    }

    private string? _sortAlbumArtist = "";

    public string? SortAlbumArtist
    {
        get => _sortAlbumArtist;
        set => SetValue(ref _sortAlbumArtist, value, nameof(SortAlbumArtist));
    }

    private string? _longDescription = "";

    public string? LongDescription
    {
        get => _longDescription;
        set => SetValue(ref _longDescription, value, nameof(LongDescription));
    }

    private string? _seriesTitle = "";

    public string? SeriesTitle
    {
        get => _seriesTitle;
        set => SetValue(ref _seriesTitle, value, nameof(SeriesTitle));
    }

    private string? _seriesPart = "";

    public string? SeriesPart
    {
        get => _seriesPart;
        set => SetValue(ref _seriesPart, value, nameof(SeriesPart));
    }

    private string? _encodedBy = "";

    public string? EncodedBy
    {
        get => _encodedBy;
        set => SetValue(ref _encodedBy, value, nameof(EncodedBy));
    }

    private string? _encodingTool = "";

    public string? EncodingTool
    {
        get => _encodingTool;
        set => SetValue(ref _encodingTool, value, nameof(EncodingTool));
    }

    private DateTime? _purchaseDate;

    public DateTime? PurchaseDate
    {
        get => _purchaseDate;
        set => SetValue(ref _purchaseDate, value, nameof(PurchaseDate));
    }

    private string? _mediaType;

    public string? MediaType
    {
        get => _mediaType;
        set => SetValue(ref _mediaType, value, nameof(MediaType));
    }

    private string? _narrator;

    public string? Narrator
    {
        get => _narrator;
        set => SetValue(ref _narrator, value, nameof(Narrator));
    }

    private void SetValue<T>(ref T? property, T? value, string propertyName)
    {
        if (EqualityComparer<T>.Default.Equals(property, value))
        {
            return;
        }

        property = value;
        UpdateAdditionalProperties(propertyName);
    }

    private void UpdateAdditionalProperties(params string[] updatedPropertyNames)
    {
        // Group => nothing
        // SortTitle, SortAlbum, SortArtist, SortAlbumArtist => nothing

        // Subtitle => Update SortAlbum: If ALBUM only, then %Title%, If ALBUM and SUBTITLE, then %Title% - %Subtitle%, If Series, then %Series% %Series-part% - %Title%
        // Series => Update SortAlbum(ifnull?) and Group(ifnull?)

        // ifnull? maybe keep updatedProps in a history listing and only change if
        // it not has been changed manually
        // foreach (var propertyName in updatedPropertyNames)
        // {
        //     
        // }

        switch (AudioFormat.ID)
        {
            case AudioDataIOFactory.CID_MP3:
                UpdatedAdditionalFieldsFromPropertiesForMp3();
                break;
            case AudioDataIOFactory.CID_MP4:
                UpdatedAdditionalFieldsFromPropertiesForMp4();
                break;
        }
    }

    private void UpdatedAdditionalFieldsFromPropertiesForMp3()
    {
        // https://id3.org/id3v2.3.0
        
        MakeAdditionalField("TIT1", Group, JoinSeparatedString(" ", SeriesTitle, SeriesPart));
        MakeAdditionalField("TIT3", Subtitle);
        MakeAdditionalField("TSOT", SortTitle, Title);
        MakeAdditionalField("TSOA", SortAlbum, Album);
        MakeAdditionalField("TSOP", SortArtist, Artist);
        // MakeAdditionalField("????", SortAlbumArtist, Artist);

        if ((Comment ?? "") == "")
        {
            // MakeAdditionalField("????", LongDescription, Description);
            Comment = LongDescription;
        }
        
        // TENC
        // MakeAdditionalField("©enc",EncodedBy);
        
        // TSSM
        // MakeAdditionalField("©too",EncodingTool);
        
        // maybe TOWN? (ownership frame)
        // MakeAdditionalField("purd",PurchaseDate?.ToString("yyyy/MM/dd"));
        
        // not possible
        // MakeAdditionalField("stik",MediaType);
        
        // put narrator to conductor if set and conductor is empty (TPE3
        if (Narrator?.Length > 0 && (Conductor?.Length ?? 0) == 0)
        {
            Conductor = Narrator;
        }
        MakeAdditionalField("MVNM",SeriesTitle);
        MakeAdditionalField("MVIN",SeriesPart);
        MakeAdditionalField("TXXX:SERIES",SeriesTitle);
        MakeAdditionalField("TXXX:SERIES-PART",SeriesPart);
        MakeAdditionalField("TXXX:TMP_GENRE1",Genre);
        // todo:
        // MakeAdditionalField("TXXX:TMP_GENRE2",Genre);

    }

    private void UpdatedAdditionalFieldsFromPropertiesForMp4()
    {
        /*
;FFMETADATA1
major_brand=M4A 
minor_version=512
compatible_brands=isomiso2
track=1/1
disc=1/1
gapless_playback=1
description=He was a husband, a father, a preacher - and the preeminent leader of a movement that continues to transform America and the world. Martin Luther King, Jr. was one of the twentieth century's most influential men and lived one of its most extraordinary lives. Now, in a special program commissioned and authorized by his family, here is the life and times of Martin Luther King, Jr. drawn from a comprehensive collection of writings, recordings, and documentary materials, many of which have never before been made public. This history-making autobiography is Martin Luther King in his own words as read by others and in his own words and voice: the mild-mannered, inquisitive child and student who chafed under and eventually rebelled against segregation\; the dedicated young minister who continually questioned the depths of his faith and the limits of his wisdom\; the loving husband and father who sought to balance his family's needs with those of a growing, nationwide movement\; and the reflective, world-famous leader who was fired by a vision of equality for people everywhere. In original recordings of his own vivid, compassionate voice, here at last is Martin Luther King, Jr.'s unforgettable chronicle of his life and his legacy. Years in the making, and woven together from thousands of recordings and documents, including letters to his family and diary entries, this program is a unique compilation which includes many rare recordings of Martin Luther King, Jr. delivering sermons, speeches, lectures, and addresses. Highlights include "I Have a Dream," "Letter from a Birmingham Jail," and "The Nobel Prize Acceptance Speech."
genre=Biographies & Memoirs/Cultural, Ethnic & Regional
media_type=2
date=1998
WWWAUDIOFILE=https://www.audible.com/pd/The-Autobiography-of-Martin-Luther-King-Jr-Audiobook/B002V01BJQ?ipRedirectOverride\=true&overrideBaseCountry
album=The Autobiography of Martin Luther King, Jr.
tmp_Genre2=Cultural, Ethnic & Regional
tmp_Genre1=Biographies & Memoirs
title=The Autobiography of Martin Luther King, Jr.
RELEASETIME=&\#16
RATING WMP=4.7
Publisher=\;1998 Time Warner AudioBooks (Packaging Only), A Division of Time Warner Trade Publishing \; 1998 Intellectual Properties Management, Inc., All Rights Reserved\; 16 9\; 1998 Time Warner AudioBooks (Packaging Only), A Division of Time Warner Trade Publishing
copyright=1998 Intellectual Properties Management, Inc., All Rights Reserved
composer=Levar Burton
comment=He was a husband, a father, a preacher - and the preeminent leader of a movement that continues to transform America and the world. Martin Luther King, Jr. was one of the twentieth century's most influential men and lived one of its most extraordinary lives. Now, in a special program commissioned and authorized by his family, here is the life and times of Martin Luther King, Jr. drawn from a comprehensive collection of writings, recordings, and documentary materials, many of which have never before been made public. This history-making autobiography is Martin Luther King in his own words as read by others and in his own words and voice: the mild-mannered, inquisitive child and student who chafed under and eventually rebelled against segregation\; the dedicated young minister who continually questioned the depths of his faith and the limits of his wisdom\; the loving husband and father who sought to balance his family's needs with those of a growing, nationwide movement\; and the reflective, world-famous leader who was fired by a vision of equality for people everywhere. In original recordings of his own vivid, compassionate voice, here at last is Martin Luther King, Jr.'s unforgettable chronicle of his life and his legacy. Years in the making, and woven together from thousands of recordings and documents, including letters to his family and diary entries, this program is a unique compilation which includes many rare recordings of Martin Luther King, Jr. delivering sermons, speeches, lectures, and addresses. Highlights include "I Have a Dream," "Letter from a Birmingham Jail," and "The Nobel Prize Acceptance Speech."
ASIN=B002V01BJQ
artist=Dr. Martin Luther King Jr., Levar Burton
sort_album=The Autobiography of Martin Luther King, Jr.
album_artist=Dr. Martin Luther King Jr.
encoder=Lavf57.83.100
         */
        
        MakeAdditionalField("©grp", Group, JoinSeparatedString(" ", SeriesTitle, SeriesPart));
        MakeAdditionalField("----:com.apple.iTunes:SUBTITLE", Subtitle);
        MakeAdditionalField("sonm", SortTitle, Title);
        MakeAdditionalField("soal", SortAlbum, Album);
        MakeAdditionalField("soar", SortArtist, Artist);
        MakeAdditionalField("soaa", SortAlbumArtist, AlbumArtist);
        MakeAdditionalField("ldes", LongDescription, Description);
        MakeAdditionalField("©enc", EncodedBy, "tone");
        MakeAdditionalField("©too", EncodingTool, "tone");
        MakeAdditionalField("purd", PurchaseDate?.ToString("yyyy/MM/dd"));
        MakeAdditionalField("stik", MediaType);
        
        if (MediaType == "2")
        {
            // M4B Gapless album = 1
            MakeAdditionalField("pgap", "1");
        }
        if (SeriesTitle?.Length > 0)
        {
            // Show Movement (M4B), if Series then = 1 else blank
            MakeAdditionalField("shwm", "1");
        }
        
        MakeAdditionalField("©nrt", Narrator, Composer);
        MakeAdditionalField("----:com.pilabor.tone:SERIES_TITLE", SeriesTitle);
        MakeAdditionalField("----:com.pilabor.tone:SERIES_PART", SeriesPart);
    }

    private static string? JoinSeparatedString(string separator, params string?[] values)
    {
        return JoinSeparatedString(new[] { separator }, values);
    }

    /*
var list = new[]
        {
            ("Harry Potter 1 - Harry Potter und der Stein der Weisen",
                new[] { "Harry Potter", "1", "Harry Potter und der Stein der Weisen" }),
            ("Harry Potter - Harry Potter und der Stein der Weisen",
                new[] { "Harry Potter", "", "Harry Potter und der Stein der Weisen" }),
            ("Harry Potter und der Stein der Weisen",
                new[] { "", "", "Harry Potter und der Stein der Weisen" }),
            ("Harry Potter",
                new[] { "Harry Potter", "", "" }),
            ("Harry Potter 1",
                new[] { "Harry Potter", "1", "" }),
            ("1 - Harry Potter und der Stein der Weisen",
                new[] { "", "1", "Harry Potter und der Stein der Weisen" }),
        };
        var separators = new[] { " ", " - " };

        foreach (var (expected, input) in list)
        {
            var actual = ConcatMulti(separators, input);
            if (expected != actual)
            {
                _console.Error.WriteLine($"error: <{expected}>!=<{actual}>");
            }
        }        
     */
    private static string JoinSeparatedString(IReadOnlyList<string> separator, params string?[] values)
    {
        var concatValues = new List<string?>();
        for (var i = 0; i < values.Length; i++)
        {
            if (string.IsNullOrEmpty(values[i]))
            {
                continue;
            }
            var separatorIndex = i - 1;
            if (separatorIndex >= 0 && separatorIndex < separator.Count && concatValues.Count > 0)
            {
                concatValues.Add(separator[separatorIndex]);
            }
            concatValues.Add(values[i]);
        }
        return string.Join("", concatValues);
    }
    private void MakeAdditionalField(string key, params string?[] values)
    {
        foreach (var value in values)
        {
            if (value != null)
            {
                AdditionalFields[key] = value;
                return;
            }
        }
    }


    protected new void Update(bool onlyReadEmbeddedPictures = false)
    {
        base.Update(onlyReadEmbeddedPictures);
        switch (AudioFormat.ID)
        {
            case AudioDataIOFactory.CID_MP3:
                // todo: LoadPropertiesFromAdditionalFieldsForMp3
                break;
            case AudioDataIOFactory.CID_MP4:
                // todo: LoadPropertiesFromAdditionalFieldsForMp4
                break;
        }
    }


    public MetadataTrack()
    {
        // this.Genre
    }

    public MetadataTrack(string path, IProgress<float>? writeProgress = null, bool load = true)
        : base(path, writeProgress, load)
    {
    }

    public MetadataTrack(IFileSystemInfo fileInfo, IProgress<float>? writeProgress = null, bool load = true)
        : base(fileInfo.FullName, writeProgress, load)
    {
    }


    // todo: Switch mapping to array
    // get value: get first non empty one
    // set value: set all values
    private static readonly Dictionary<MappingKey, string[][]> AdditionalFieldMapping = new()
    {
        {
            MappingKey.Group, new[]
            {
                new[] { "©grp" },
                new[] { "TIT1" }
            }
        },
        {
            MappingKey.Subtitle, new[]
            {
                new[] { "----:com.apple.iTunes:SUBTITLE" },
                new[] { "TIT1" }
            }
        },
        {
            MappingKey.SortName, new[]
            {
                new[] { "sonm" },
                new[] { "TSOT" }
            }
        },
        {
            MappingKey.SortAlbum, new[]
            {
                new[] { "soal" },
                new[] { "TSOA" }
            }
        },
        {
            MappingKey.SortArtist, new[]
            {
                new[] { "soar" },
                new[] { "TSOP" }
            }
        },
        {
            MappingKey.SortAlbumArtist, new[]
            {
                new[] { "soaa" },
                new[] { "TSO2" }
            }
        },
        {
            MappingKey.LongDescription, new[]
            {
                new[] { "ldes" },
                new[] { "TDES" }
            }
        },
        {
            MappingKey.EncodedBy, new[]
            {
                new[] { "©enc" },
                new[] { "TENC" }
            }
        },
        {
            MappingKey.EncodingTool, new[]
            {
                new[] { "©too" },
                new[] { "TSSE" }
            }
        },
        {
            MappingKey.PurchaseDate, new[]
            {
                new[] { "purd" },
                Array.Empty<string>()
            }
        },
        {
            MappingKey.MediaType, new[]
            {
                new[] { "stik" },
                Array.Empty<string>()
            }
        },
        {
            MappingKey.Narrator, new[]
            {
                new[] { "©nrt" },
                new[] { "TCOM" },
            }
        },
        {
            MappingKey.SeriesTitle, new[]
            {
                new[] { "----:com.pilabor.tone:SERIES_TITLE" },
                new[] { "MVNM" }
            }
        },
        {
            MappingKey.SeriesPart, new[]
            {
                new[] { "----:com.pilabor.tone:SERIES_PART" },
                Array.Empty<string>()
            }
        },
    };


    // private string? GetAdditionalField(MappingKey key)
    // {
    //     var resolvedKeys = ResolveKey(AudioFormat, key);
    //     if (resolvedKeys.Length == 0)
    //     {
    //         return null;
    //     }
    //
    //     foreach (var resolvedKey in resolvedKeys)
    //     {
    //         if (AdditionalFields.ContainsKey(resolvedKey) && AdditionalFields[resolvedKey] != null &&
    //             AdditionalFields[resolvedKey] != "")
    //         {
    //             return AdditionalFields[resolvedKey];
    //         }
    //     }
    //
    //     return null;
    // }

    // private DateTime? GetAdditionalFieldDate(MappingKey key)
    // {
    //     var stringValue = GetAdditionalField(key);
    //     if (stringValue == null)
    //     {
    //         return null;
    //     }
    //
    //     if (DateTime.TryParse(stringValue, out var result))
    //     {
    //         return result;
    //     }
    //
    //     return null;
    // }

    // private void SetAdditionalField(MappingKey key, string? value)
    // {
    //     var resolvedKeys = ResolveKey(AudioFormat, key);
    //     if (resolvedKeys.Length == 0)
    //     {
    //         return;
    //     }
    //
    //     if (value != null)
    //     {
    //         foreach (var resolvedKey in resolvedKeys)
    //         {
    //             AdditionalFields[resolvedKey] = value;
    //         }
    //
    //         return;
    //     }
    //
    //     foreach (var resolvedKey in resolvedKeys)
    //     {
    //         if (AdditionalFields.ContainsKey(resolvedKey))
    //         {
    //             AdditionalFields.Remove(resolvedKey);
    //         }
    //     }
    // }

    // private void SetAdditionalField(MappingKey key, DateTime? value)
    // {
    //     SetAdditionalField(key, value?.ToString("yyyy/MM/dd"));
    // }

    private static string[] ResolveKey(ATL.Format format, MappingKey key)
    {
        return format.ID switch
        {
            AudioDataIOFactory.CID_MP4 => ResolveKey(FormatKey.Mp4, key),
            AudioDataIOFactory.CID_MP3 => ResolveKey(FormatKey.Mp3, key),
            _ => Array.Empty<string>()
        };
    }

    private static string[] ResolveKey(FormatKey formatKey, MappingKey key)
    {
        if (!AdditionalFieldMapping.ContainsKey(key))
        {
            return Array.Empty<string>();
        }

        var formatKeyAsInt = (int)formatKey;
        return AdditionalFieldMapping[key].Length < formatKeyAsInt
            ? Array.Empty<string>()
            : AdditionalFieldMapping[key][formatKeyAsInt];
    }
}