using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OperationResult;
using Sandreas.AudioMetadata;
using tone.Commands.Settings.Interfaces;
using static OperationResult.Helpers;

namespace tone.Metadata.Taggers;

public class EquateTagger : INamedTagger
{
    public string Name => nameof(EquateTagger);

    private readonly IEnumerable<string> _equateFieldSets;

    public EquateTagger(IEnumerable<string> equateFieldSets)
    {
        _equateFieldSets = equateFieldSets;
    }

    public EquateTagger(IEquateTaggerSettings settings)
    {
        _equateFieldSets = settings.Equate;
    }

    public async Task<Status<string>> UpdateAsync(IMetadata metadata)
    {
        foreach (var fieldSet in _equateFieldSets)
        {
            var equateFields = fieldSet.Split(",").Select(f => f.Trim()).Where(f => !string.IsNullOrEmpty(f)).ToArray();
            var equateProperties = equateFields.Where(f => Enum.TryParse(f, out MetadataProperty _)).Select(Enum.Parse<MetadataProperty>)
                .ToArray();
            if (equateProperties.Length < 2)
            {
                return Error($"equate fieldset does not contain destination: {fieldSet}");
            }

            var sourceProperty = equateProperties.First();
            var destinationProperties = equateProperties.Skip(1).ToArray();

            var sourceValue = metadata.GetMetadataPropertyValue(sourceProperty);
            var sourceValueType = metadata.GetMetadataPropertyType(sourceProperty);
            if (sourceValueType == null)
            {
                return Error($"source value type is not defined: {fieldSet}");
            }
            foreach (var destinationProperty in destinationProperties)
            {
                if (sourceValueType.Name != metadata.GetMetadataPropertyType(destinationProperty)?.Name)
                {
                    return Error($"source type is not equal to destination type: {fieldSet} ({sourceProperty.ToString()}≠{destinationProperty.ToString()})");
                }
                metadata.SetMetadataPropertyValue(destinationProperty, sourceValue);
            }
            
        }

        return await Task.FromResult(Ok());
    }

}