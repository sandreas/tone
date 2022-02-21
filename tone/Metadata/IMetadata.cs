using System;

namespace tone.Metadata;

public interface IMetadata
{
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
    public string? SortName { get; set; }
    public string? SortAlbum { get; set; }
    public string? SortArtist { get; set; }
    public string? SortAlbumArtist { get; set; }
    public string? LongDescription { get; set; }
    public string? EncodingTool { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public string? MediaType { get; set; }

    // /// <summary>Recording Date (set to DateTime.MinValue to remove)</summary>
    // public DateTime? Date { get; set; }
    //
    // /// <summary>Track number</summary>
    // public int? TrackNumber { get; set; }
    //
    // /// <summary>Total track number</summary>
    // public int? TrackTotal { get; set; }
    //
    // /// <summary>Disc number</summary>
    // public int? DiscNumber { get; set; }
    //
    // /// <summary>Total disc number</summary>
    // public int? DiscTotal { get; set; }
    //
    // /// <summary>
    // /// Popularity (0% = 0 stars to 100% = 5 stars)
    // /// e.g. 3.5 stars = 70%
    // /// </summary>
    // public float? Popularity { get; set; }
}