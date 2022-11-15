using System.Threading.Tasks;
using OperationResult;
using Sandreas.AudioMetadata;
using static OperationResult.Helpers;

namespace tone.Metadata.Taggers;

public class M4BFillUpTagger : AbstractNamedTagger
{
    // private const string RomanLetters = "IVXLCDM";

    public override string Name => nameof(M4BFillUpTagger);

    public override async Task<Status<string>> UpdateAsync(IMetadata metadata, IMetadata? originalMetadata = null)
    {
        if (!IsM4B(metadata))
        {
            return await Task.FromResult(Ok());
        }
        metadata.Album = GetFirstNonEmptyValueOrNull(metadata.Album, metadata.Title);
        metadata.Title = GetFirstNonEmptyValueOrNull(metadata.Title, metadata.Album);
        metadata.ItunesMediaType = GetFirstNonEmptyValueOrNull(metadata.ItunesMediaType, ItunesMediaType.Audiobook);
        metadata.ItunesPlayGap = GetFirstNonEmptyValueOrNull(metadata.ItunesPlayGap, ItunesPlayGap.NoGap);
        metadata.Narrator = GetFirstNonEmptyValueOrNull(metadata.Narrator, metadata.Composer);
        metadata.Comment = GetFirstNonEmptyValueOrNull(metadata.Comment, metadata.LongDescription, metadata.Description);

        if (ShouldUpdateSortTitle(metadata))
        {
            // If ALBUM only, then %Title%
            //     If ALBUM and SUBTITLE, then %Title% - %Subtitle%
            //     If Series, then %Series% %Series-part% - %Title%
            metadata.SortTitle = MultiConcat(metadata.MovementName, " ", metadata.Part, " - ", metadata.Title);
            metadata.SortAlbum = GetFirstNonEmptyValueOrNull(  metadata.SortAlbum, metadata.SortTitle);
            
            metadata.AdditionalFields["shwm"] = "1"; // show movement
        }
        
        return await Task.FromResult(Ok());
    }



}