using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ATL;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Sandreas.AudioMetadata;

namespace tone.Metadata;

public class ToneJsonContractResolver : CamelCasePropertyNamesContractResolver
{
    private  MetadataProperty[] _forcedProperties;

    public ToneJsonContractResolver(MetadataProperty[]? forcedProperties = null)
    {
        _forcedProperties = forcedProperties??Array.Empty<MetadataProperty>();
    }
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);
        if (property.DeclaringType == null || property.DeclaringType != typeof(ToneJsonMeta)) {
            return property;
        }

        if (!Enum.TryParse<MetadataProperty>(property.PropertyName, true, out var metadataProperty)){
            return property;
        }
        
        if(_forcedProperties.Contains(metadataProperty))
        {
            property.NullValueHandling = NullValueHandling.Include;
            property.DefaultValueHandling = DefaultValueHandling.Include;
        }
        property.ShouldSerialize = instance => {
            if(instance is not IMetadata toneJsonMeta || _forcedProperties.Contains(metadataProperty))
            {
                return true;
            }
            return !MetadataExtensions.IsEmpty(toneJsonMeta.GetMetadataPropertyValue(metadataProperty));
        };
        
        return property;
    }

}