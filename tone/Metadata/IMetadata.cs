using System;
using System.Collections.Generic;
using ATL;

namespace tone.Metadata;

// TagData
public interface IMetadata
{
    /// <summary>Track number</summary>
    public int? TrackNumber { get; set; }

    /// <summary>Total track number</summary>
    public int? TrackTotal { get; set; }

    /// <summary>Disc number</summary>
    public int? DiscNumber { get; set; }

    /// <summary>Total disc number</summary>
    public int? DiscTotal { get; set; }

    /// <summary>
    /// Popularity (0% = 0 stars to 100% = 5 stars)
    /// e.g. 3.5 stars = 70%
    /// </summary>
    public float? Popularity { get; set; }

    /// <summary>Title</summary>
    public string? Title { get; set; }

    /// <summary>Artist</summary>
    public string? Artist { get; set; }

    /// <summary>Composer</summary>
    public string? Composer { get; set; }

    /// <summary>Comments</summary>
    public string? Comment { get; set; }

    /// <summary>Genre</summary>
    public string? Genre { get; set; }

    /// <summary>Title of the album</summary>
    public string? Album { get; set; }

    /// <summary>Title of the original album</summary>
    public string? OriginalAlbum { get; set; }

    /// <summary>Original artist</summary>
    public string? OriginalArtist { get; set; }

    /// <summary>Copyright</summary>
    public string? Copyright { get; set; }

    /// <summary>General description</summary>
    public string? Description { get; set; }

    /// <summary>Publisher</summary>
    public string? Publisher { get; set; }

    /// <summary>Album Artist</summary>
    public string? AlbumArtist { get; set; }

    /// <summary>Conductor</summary>
    public string? Conductor { get; set; }


    public string? Group { get; set; }
    public string? SortTitle { get; set; }
    public string? SortAlbum { get; set; }
    public string? SortArtist { get; set; }
    public string? SortAlbumArtist { get; set; }
    public string? LongDescription { get; set; }
    public string? EncodingTool { get; set; }
    public ItunesMediaType? ItunesMediaType { get; set; }

    /// <summary>Chapters table of content description</summary>
    public string? ChaptersTableDescription { get; set; }

    public string? Narrator { get; set; }
    public string? MovementName { get; set; }
    public string? Movement { get; set; }
    
    public string? Path { get;}
    
    /// <summary>Publishing date (set to DateTime.MinValue to remove)</summary>
    public DateTime? PublishingDate { get; set; }

    /// <summary>Recording Date (set to DateTime.MinValue to remove)</summary>
    public DateTime? RecordingDate { get; set; }

    public DateTime? PurchaseDate { get; set; }
    
    /// <summary>Synchronized and unsynchronized lyrics</summary>
    public LyricsInfo? Lyrics { get; set; }

    public TimeSpan TotalDuration { get;}
    
    /// <summary>
    /// Contains any other metadata field that is not represented by a getter in the above interface
    /// </summary>
    public IList<ChapterInfo>? Chapters { get;  }

    /// <summary>
    /// List of picture IDs stored in the tag
    ///     PictureInfo.PIC_TYPE : internal, normalized picture type
    ///     PictureInfo.NativePicCode : native picture code (useful when exploiting the UNSUPPORTED picture type)
    ///     NB : PictureInfo.PictureData (raw binary picture data) is _not_ valued here; see EmbeddedPictures field
    /// </summary>
    public IList<PictureInfo>? EmbeddedPictures { get; }
    /// <summary>
    /// Contains any other metadata field that is not represented by a getter in the above interface
    /// </summary>
    public IDictionary<string, string>? AdditionalFields { get; set; }
    

}