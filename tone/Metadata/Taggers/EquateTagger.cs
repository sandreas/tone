using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OperationResult;
using static OperationResult.Helpers;

namespace tone.Metadata.Taggers;

public class EquateTagger : TaggerBase
{
    private readonly IEnumerable<string> _equateFieldSets;

    public EquateTagger(IEnumerable<string> equateFieldSets)
    {
        _equateFieldSets = equateFieldSets;
    }

    public override async Task<Status<string>> UpdateAsync(IMetadata metadata)
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

    /*
    private static Status<string> SetFieldByName(IMetadata metadata, PropertyInfo[] properties,
        string[] fields)
    {
        var sourceFieldName = fields.First();
        var destinationFieldNames = fields.Skip(1).ToArray();
        var baseProperty = properties.FirstOrDefault(p =>
            string.Equals(sourceFieldName, p.Name, StringComparison.CurrentCultureIgnoreCase));
        if (baseProperty == null)
        {
            return Error($"No metadata property found for field {sourceFieldName}");
        }

        var sourceValue = baseProperty.GetValue(metadata, null);

        try
        {
            foreach (var property in properties)
            {
                if (destinationFieldNames.Any(d =>
                        string.Equals(d, property.Name, StringComparison.CurrentCultureIgnoreCase)))
                {
                    property.SetValue(metadata, Convert.ChangeType(sourceValue, property.PropertyType), null);
                }
            }
        }
        catch (Exception e)
        {
            return Error($"Could not equate properties for {sourceFieldName}: " + e.Message);
        }

        return Ok();
    }
    */
}