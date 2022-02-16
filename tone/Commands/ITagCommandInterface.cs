using System;
using CliFx.Attributes;

namespace tone.Commands;

public interface ITagCommandInterface
{
    /// <summary>Title</summary>
    [CommandOption("tag-title")]    
    public string Title { get; set; }

    /// <summary>Artist</summary>
    [CommandOption("tag-artist")]    
    public string Artist { get; set; }

    /// <summary>Composer</summary>
    [CommandOption("tag-composer")]    
    public string Composer { get; set; }

    /// <summary>Comments</summary>
    [CommandOption("tag-comment")]    
    public string Comment { get; set; }

    /// <summary>Genre</summary>
    [CommandOption("tag-genre")]    
    public string Genre { get; set; }

    /// <summary>Title of the album</summary>
    [CommandOption("tag-album")]    
    public string Album { get; set; }

    /// <summary>Title of the original album</summary>
    [CommandOption("tag-original-album")]    
    public string OriginalAlbum { get; set; }

    /// <summary>Original artist</summary>
    [CommandOption("tag-original-artist")]    
    public string OriginalArtist { get; set; }

    /// <summary>Copyright</summary>
    [CommandOption("tag-copyright")]    
    public string Copyright { get; set; }

    /// <summary>General description</summary>
    [CommandOption("tag-description")]    
    public string Description { get; set; }

    /// <summary>Publisher</summary>
    [CommandOption("tag-publisher")]    
    public string Publisher { get; set; }

    /// <summary>Publishing date (set to DateTime.MinValue to remove)</summary>
    [CommandOption("tag-publishing-date")]
    public DateTime PublishingDate { get; set; }

    /// <summary>Album Artist</summary>
    [CommandOption("tag-album-artist")]    
    public string AlbumArtist { get; set; }

    /// <summary>Conductor</summary>
    [CommandOption("input")]    
    public string Conductor { get; set; }

    /// <summary>Recording Date (set to DateTime.MinValue to remove)</summary>
    [CommandOption("tag-date")]
    public DateTime Date { get; set; }
    

    /// <summary>Track number</summary>
    [CommandOption("tag-track-number")]
    public int TrackNumber { get; set; }

    /// <summary>Total track number</summary>
    [CommandOption("tag-track-total")]
    public int TrackTotal { get; set; }

    /// <summary>Disc number</summary>
    [CommandOption("tag-disc-number")]
    public int DiscNumber { get; set; }

    /// <summary>Total disc number</summary>
    [CommandOption("tag-disc-total")]
    public int DiscTotal { get; set; }

    /// <summary>
    /// Popularity (0% = 0 stars to 100% = 5 stars)
    /// e.g. 3.5 stars = 70%
    /// </summary>
    [CommandOption("tag-popularity")]
    public float Popularity { get; set; }
    
    
    /*
public $encoder;
    public $title;
    public $sortTitle; // -sortname on mp4tags (means sort chapter title in itunes)
    public $artist;
    public $sortArtist; // -sortartist on mp4tags (means sort author in itunes)
    public $genre;
    public $writer;
    public $sortWriter;
    public $album;
    public $sortAlbum; // -sortalbum on mp4tags (means sort title in itunes)
    public $disk;
    public $disks;
    public $grouping;
    public $purchaseDate;
    public $albumArtist;
    public $sortAlbumArtist;
    public $year;
    public $track;
    public $tracks;
    public $cover;
    public $description;
    public $longDescription;
    public $comment;
    public $copyright;
    public $encodedBy;
    public $type = self::MEDIA_TYPE_AUDIO_BOOK;

    // MP3 Specific
    public $performer; // TPE3
    public $language; // TLAN
    public $publisher; // TPUB
    public $lyrics; // TSLT

    public $chapters = [];

    // pseudo tags that are used to auto generate sort properties, if not present
    public $series;
    public $seriesPart;
 
     */
    
    /// <summary>
    /// List of picture IDs stored in the tag
    ///     PictureInfo.PIC_TYPE : internal, normalized picture type
    ///     PictureInfo.NativePicCode : native picture code (useful when exploiting the UNSUPPORTED picture type)
    ///     NB : PictureInfo.PictureData (raw binary picture data) is _not_ valued here; see EmbeddedPictures field
    /// </summary>
    // public IList<PictureInfo> PictureTokens { get; set; }

    /// <summary>Chapters table of content description</summary>
    // public string ChaptersTableDescription { get; set; }

    /// <summary>
    /// Contains any other metadata field that is not represented by a getter in the above interface
    /// </summary>
    // public IList<ChapterInfo> Chapters { get; set; }
}
















