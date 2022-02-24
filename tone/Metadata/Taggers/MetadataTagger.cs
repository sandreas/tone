namespace tone.Metadata.Taggers;

public class MetadataTagger: ITagger
{
    private readonly IMetadata _source;

    public MetadataTagger(IMetadata source)
    {
        _source = source;
    }
    
    public void Update(IMetadata metadata)
    {
        metadata.Album = _source.Album ?? metadata.Album;
        metadata.Title = _source.Title ?? metadata.Title;
        metadata.Artist = _source.Artist ?? metadata.Artist;
        metadata.Composer = _source.Composer ?? metadata.Composer;
        metadata.Comment = _source.Comment ?? metadata.Comment;
        metadata.Genre = _source.Genre ?? metadata.Genre;
        metadata.Album = _source.Album ?? metadata.Album;
        metadata.OriginalAlbum = _source.OriginalAlbum ?? metadata.OriginalAlbum;
        metadata.Copyright = _source.Copyright ?? metadata.Copyright;
        metadata.Description = _source.Description ?? metadata.Description;
        metadata.Publisher = _source.Publisher ?? metadata.Publisher;
        metadata.AlbumArtist = _source.AlbumArtist ?? metadata.AlbumArtist;
        metadata.Conductor = _source.Conductor ?? metadata.Conductor;
        metadata.Group = _source.Group ?? metadata.Group;
        metadata.SortName = _source.SortName ?? metadata.SortName;
        metadata.SortAlbum = _source.SortAlbum ?? metadata.SortAlbum;
        metadata.SortArtist = _source.SortArtist ?? metadata.SortArtist;
        metadata.SortAlbumArtist = _source.SortAlbumArtist ?? metadata.SortAlbumArtist;
        metadata.LongDescription = _source.LongDescription ?? metadata.LongDescription;
        metadata.EncodingTool = _source.EncodingTool ?? metadata.EncodingTool;
        metadata.PurchaseDate = _source.PurchaseDate ?? metadata.PurchaseDate;
        metadata.MediaType = _source.MediaType ?? metadata.MediaType;
    }
}