using System.Threading.Tasks;
using OperationResult;
using Sandreas.AudioMetadata;
using static OperationResult.Helpers;

namespace tone.Metadata.Taggers;

public class PrependMovementToDescriptionTagger : AbstractNamedTagger
{
    private readonly bool _enabled;

    public PrependMovementToDescriptionTagger(bool enabled=false)
    {
        _enabled = enabled;
    }
    public override string Name => nameof(PrependMovementToDescriptionTagger);

    public override async Task<Status<string>> UpdateAsync(IMetadata metadata)
    {
        if (!_enabled || !HasMovementOrPart(metadata) || (string.IsNullOrEmpty(metadata.Description) &&
                                                          string.IsNullOrEmpty(metadata.LongDescription))){
            return await Task.FromResult(Ok());
        }

        var movementPrefix = MultiConcat(metadata.MovementName, " ", metadata.Part) + ": ";
        var desc = (!string.IsNullOrEmpty(metadata.Description) ? metadata.Description : metadata.LongDescription) ?? "";
        var longDesc = (!string.IsNullOrEmpty(metadata.LongDescription) ? metadata.LongDescription : metadata.Description) ?? "";

        if(!desc.StartsWith(movementPrefix))
        {
            metadata.Description = movementPrefix + desc;
        }
            
        if(!longDesc.StartsWith(movementPrefix))            {
            metadata.LongDescription = movementPrefix + longDesc;
        }
            
        if(metadata.Comment == longDesc)    {
            metadata.Comment = metadata.LongDescription;
        }
        
        return await Task.FromResult(Ok());
    }

    
}