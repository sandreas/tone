using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ATL;
using ATL.AudioData;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace tone.Metadata.Converters;

public class LyricsConverter : JsonConverter
{
    private bool _startWritten = false;

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is LyricsInfo lyricsInfo)
        {
            if (lyricsInfo.ContentType != LyricsInfo.LyricsType.LYRICS)
            {
                _writeStartOnce(writer);
                writer.WritePropertyName("type");
                writer.WriteValue(lyricsInfo.ContentType);
            }

            if (lyricsInfo.Description != "")
            {
                _writeStartOnce(writer);

                writer.WritePropertyName("description");
                writer.WriteValue(lyricsInfo.Description);
            }

            if (lyricsInfo.LanguageCode != "")
            {
                _writeStartOnce(writer);

                writer.WritePropertyName("language");
                writer.WriteValue(lyricsInfo.LanguageCode);
            }

            if (lyricsInfo.UnsynchronizedLyrics != "")
            {
                _writeStartOnce(writer);

                writer.WritePropertyName("unsynchronized");
                writer.WriteValue(lyricsInfo.UnsynchronizedLyrics);
            }

            if (lyricsInfo.SynchronizedLyrics.Count > 0)
            {
                _writeStartOnce(writer);

                writer.WritePropertyName("unsynchronized");
                writer.WriteStartArray();
                foreach (var syncItem in lyricsInfo.SynchronizedLyrics)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("time");
                    writer.WriteValue(syncItem.TimestampMs);
                    writer.WritePropertyName("text");
                    writer.WriteValue(syncItem.Text);
                    writer.WriteEndObject();
                }

                writer.WriteEndArray();
            }

            if (_startWritten)
                writer.WriteEndObject();
        }

        if(!_startWritten)
            writer.WriteNull();

    }

    private void _writeStartOnce(JsonWriter writer)
    {
        if (_startWritten)
            return;
        writer.WriteStartObject();
        _startWritten = true;
    }

    // https://www.jerriepelser.com/blog/custom-converters-in-json-net-case-study-1/
    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.StartObject)
        {
            var result = new LyricsInfo();

            var lyrics = JObject.Load(reader);

            var type = lyrics["type"]?.ToString() ?? LyricsInfo.LyricsType.LYRICS.ToString();
            if (Enum.TryParse<LyricsInfo.LyricsType>(type, out var lyricsType))
            {
                result.ContentType = lyricsType;
            }

            result.Description = lyrics["description"]?.ToString() ?? "";
            result.LanguageCode = lyrics["language"]?.ToString() ?? "";
            result.UnsynchronizedLyrics = lyrics["unsynchronized"]?.ToString() ?? "";
            if (lyrics["synchronized"] is JArray syncItems)
            {
                foreach (var syncItem in syncItems)
                {
                    if (syncItem["time"] == null || syncItem["text"] == null)
                    {
                        continue;
                    }

                    var phrase =
                        new LyricsInfo.LyricsPhrase(syncItem.Value<int>("time"), syncItem.Value<string>("text"));
                    result.SynchronizedLyrics.Add(phrase);
                }
            }

            return result;
        }

        return null;
    }

    public override bool CanConvert(Type objectType)
    {
        return typeof(IList<PictureInfo>).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo());
    }
}