using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ATL;
using OperationResult;

namespace tone.Metadata.Taggers;

public abstract class TaggerBase: ITagger
{
    public abstract Task<Status<string>> Update(IMetadata metadata);

    protected static void TransferMetadataProperties(IMetadata source, IMetadata metadata)
    {
        // https://stackoverflow.com/questions/1402803/passing-properties-by-reference-in-c-sharp
        metadata.Artist = TransferMetadataProperty( source.Artist, metadata.Artist);
        metadata.TrackNumber = TransferMetadataProperty( source.TrackNumber, metadata.TrackNumber);
        metadata.TrackTotal = TransferMetadataProperty( source.TrackTotal, metadata.TrackTotal);
        metadata.DiscNumber = TransferMetadataProperty( source.DiscNumber, metadata.DiscNumber);
        metadata.DiscTotal = TransferMetadataProperty( source.DiscTotal, metadata.DiscTotal);
        metadata.Popularity = TransferMetadataProperty( source.Popularity, metadata.Popularity);
        metadata.Title = TransferMetadataProperty( source.Title, metadata.Title);
        metadata.Composer = TransferMetadataProperty( source.Composer, metadata.Composer);
        metadata.Comment = TransferMetadataProperty( source.Comment, metadata.Comment);
        metadata.Genre = TransferMetadataProperty( source.Genre, metadata.Genre);
        metadata.Album = TransferMetadataProperty( source.Album, metadata.Album);
        metadata.OriginalAlbum = TransferMetadataProperty( source.OriginalAlbum, metadata.OriginalAlbum);
        metadata.OriginalArtist = TransferMetadataProperty( source.OriginalArtist, metadata.OriginalArtist);
        metadata.Copyright = TransferMetadataProperty( source.Copyright, metadata.Copyright);
        metadata.Description = TransferMetadataProperty( source.Description, metadata.Description);
        metadata.Publisher = TransferMetadataProperty( source.Publisher, metadata.Publisher);
        metadata.AlbumArtist = TransferMetadataProperty( source.AlbumArtist, metadata.AlbumArtist);
        metadata.Conductor = TransferMetadataProperty( source.Conductor, metadata.Conductor);
        metadata.Group = TransferMetadataProperty( source.Group, metadata.Group);
        metadata.SortTitle = TransferMetadataProperty( source.SortTitle, metadata.SortTitle);
        metadata.SortAlbum = TransferMetadataProperty( source.SortAlbum, metadata.SortAlbum);
        metadata.SortArtist = TransferMetadataProperty( source.SortArtist, metadata.SortArtist);
        metadata.SortAlbumArtist = TransferMetadataProperty( source.SortAlbumArtist, metadata.SortAlbumArtist);
        metadata.LongDescription = TransferMetadataProperty( source.LongDescription, metadata.LongDescription);
        metadata.EncodingTool = TransferMetadataProperty( source.EncodingTool, metadata.EncodingTool);
        metadata.ItunesMediaType = TransferMetadataProperty( source.ItunesMediaType, metadata.ItunesMediaType);
        metadata.ChaptersTableDescription = TransferMetadataProperty( source.ChaptersTableDescription, metadata.ChaptersTableDescription);
        metadata.PublishingDate = TransferMetadataProperty( source.PublishingDate, metadata.PublishingDate);
        metadata.RecordingDate = TransferMetadataProperty( source.RecordingDate, metadata.RecordingDate);
        metadata.PurchaseDate = TransferMetadataProperty( source.PurchaseDate, metadata.PurchaseDate);
        metadata.Lyrics = TransferMetadataProperty( source.Lyrics, metadata.Lyrics);
        metadata.Narrator = TransferMetadataProperty( source.Narrator, metadata.Narrator);
        metadata.MovementName = TransferMetadataProperty( source.MovementName, metadata.MovementName);
        metadata.Movement = TransferMetadataProperty( source.Movement, metadata.Movement);
    }

    private static T? TransferMetadataProperty<T>(params T?[] source)
    {
        foreach (var s in source)
        {
            if (s != null)
            {
                return s;
            }
        }
        return default;
    }

    protected static void TransferMetadataLists(IMetadata source, IMetadata metadata)
    {
        TransferMetadataList(source.Chapters, metadata.Chapters);
        TransferMetadataList(source.EmbeddedPictures, metadata.EmbeddedPictures);
    }

    
    protected static void TransferMetadataList<T>(IList<T>? source, IList<T>? destination) where T:class
    {
        if (source == null || destination == null)
        {
            return;
        }
        if (source.Count == 0)
        {
            return;
        }

        destination.Clear();
        foreach (var s in source)
        {
            destination.Add(s);
        }
    }


    
}    