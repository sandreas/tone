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

    public override async Task<Status<string>> UpdateAsync(IMetadata metadata, IMetadata? originalMetadata = null)
    {
        originalMetadata ??= metadata;
        if (!_enabled || !HasMovementOrPart(metadata) || (string.IsNullOrEmpty(metadata.Description) &&
                                                          string.IsNullOrEmpty(metadata.LongDescription))){
            return await Task.FromResult(Ok());
        }

        var originalMovementPrefix = BuildMovementPrefix(originalMetadata);
        var movementPrefix = BuildMovementPrefix(metadata);
        var desc = (!string.IsNullOrEmpty(metadata.Description) ? metadata.Description : metadata.LongDescription) ?? "";
        var descSuffix = "";
        var longDesc = (!string.IsNullOrEmpty(metadata.LongDescription) ? metadata.LongDescription : metadata.Description) ?? "";
        if(desc.EndsWith("..."))
        {
            desc = desc[..^3];
            descSuffix = "...";
        }
        var descStripped = StripPrefix(desc, originalMovementPrefix, movementPrefix);
        var longDescStripped = StripPrefix(longDesc, originalMovementPrefix, movementPrefix);
        var commentStripped = StripPrefix(metadata.Comment ?? "", originalMovementPrefix, movementPrefix);
        
        // if desc has same start chars as longDesc and has changed, use longDesc instead to not lose chars
        if(longDescStripped.StartsWith(descStripped[..^3]) && !longDesc.StartsWith(desc))
        {
            longDesc = longDescStripped;
            desc = longDesc;
            descSuffix = "";
        }
        
        if(!desc.StartsWith(movementPrefix))
        {
            metadata.Description = movementPrefix + desc + descSuffix;
        }
        
        if(!longDesc.StartsWith(movementPrefix))            {
            metadata.LongDescription = movementPrefix + longDesc;
        }
        
        if(commentStripped == longDescStripped)    {
            metadata.Comment = metadata.LongDescription;
        }
        
        return await Task.FromResult(Ok());
    }
    
    private static string BuildMovementPrefix(IMetadata metadata)
    {
        return MultiConcat(metadata.MovementName, " ", metadata.Part) + ": ";
    }

    
    private static string StripPrefix(string description, params string[] prefixes)
    {
        foreach(var prefix in prefixes)    {
            if(description.StartsWith(prefix))
            {
                description = description.Substring(prefix.Length);
            }
        }

        return description;
    }
    
}