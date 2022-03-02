using System.Collections.Generic;
using System.Threading.Tasks;
using CommandLine;
using OperationResult;

namespace tone.Metadata.Taggers;

public abstract class TaggerBase: ITagger
{
    public abstract Task<Status<string>> Update(IMetadata metadata);

    protected void TransferMetadataProperties(IMetadata source, IMetadata metadata)
    {
        
        metadata.TrackNumber ??= source.TrackNumber;
        metadata.TrackTotal ??= source.TrackTotal;
        metadata.DiscNumber ??= source.DiscNumber;
        metadata.DiscTotal ??= source.DiscTotal;
        metadata.Popularity ??= source.Popularity;
        metadata.Title ??= source.Title;
        metadata.Artist ??= source.Artist;
        metadata.Composer ??= source.Composer;
        metadata.Comment ??= source.Comment;
        metadata.Genre ??= source.Genre;
        metadata.Album ??= source.Album;
        metadata.OriginalAlbum ??= source.OriginalAlbum;
        metadata.OriginalArtist ??= source.OriginalArtist;
        metadata.Copyright ??= source.Copyright;
        metadata.Description ??= source.Description;
        metadata.Publisher ??= source.Publisher;
        metadata.AlbumArtist ??= source.AlbumArtist;
        metadata.Conductor ??= source.Conductor;
        metadata.Group ??= source.Group;
        metadata.SortName ??= source.SortName;
        metadata.SortAlbum ??= source.SortAlbum;
        metadata.SortArtist ??= source.SortArtist;
        metadata.SortAlbumArtist ??= source.SortAlbumArtist;
        metadata.LongDescription ??= source.LongDescription;
        metadata.EncodingTool ??= source.EncodingTool;
        metadata.MediaType ??= source.MediaType;
        metadata.ChaptersTableDescription ??= source.ChaptersTableDescription;
        metadata.PublishingDate ??= source.PublishingDate;
        metadata.RecordingDate ??= source.RecordingDate;
        metadata.PurchaseDate ??= source.PurchaseDate;
        metadata.Lyrics ??= source.Lyrics;
    }

    protected void TransferMetadataLists(IMetadata source, IMetadata metadata)
    {
        TransferMetadataList(source.Chapters, metadata.Chapters);
        TransferMetadataList(source.EmbeddedPictures, metadata.EmbeddedPictures);
    }

    protected void TransferMetadataList<T>(IList<T>? source, IList<T>? destination)
    {
        if (destination?.Count == 0 && source?.Count > 0)
        {
            foreach (var p in source)
            {
                destination.Add(p);
            }
        }
    }
}