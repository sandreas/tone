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

            var value = toneJsonMeta.GetMetadataPropertyValue(metadataProperty);
            return value == null || ShouldSerializeToneJsonProperty(value);
        };
        
        return property;
    }

    // todo: make IMetadataExtensions.IsConsideredEmpty public
    private static bool ShouldSerializeToneJsonProperty(object t) => t switch
    {
         int i => i != 0,
         float f => f != 0.0f,
         double d => d != 0.0d,
         string s => !string.IsNullOrEmpty(s),
         DateTime d => d != DateTime.MinValue,
         LyricsInfo ly => string.IsNullOrEmpty(ly.UnsynchronizedLyrics) && ly.SynchronizedLyrics.Count == 0,
         IList<ChapterInfo> { Count: 0 }
             or IList<PictureInfo> { Count: 0 }
             or IDictionary<string, string> { Count: 0 } => false,
         _ => true
    };
    
    
}