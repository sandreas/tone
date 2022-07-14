using System;
using System.Collections.Generic;
using System.Reflection;
using ATL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace tone.Metadata.Converters;

public class ChaptersConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        writer.WriteStartArray();
        if (value is IList<ChapterInfo> chapters)
        {
            // var normalizedChapters = Normalize
            foreach (var chapter in chapters)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("start");
                writer.WriteValue(chapter.StartTime);

                var length = chapter.EndTime - chapter.StartTime;
                if (length > 0)
                {
                    writer.WritePropertyName("length");
                    writer.WriteValue(length);
                }

                if (!string.IsNullOrEmpty(chapter.Title))
                {
                    writer.WritePropertyName("title");
                    writer.WriteValue(chapter.Title);
                }

                if (!string.IsNullOrEmpty(chapter.Subtitle))
                {
                    writer.WritePropertyName("subtitle");
                    writer.WriteValue(chapter.Subtitle);
                }

                writer.WriteEndObject();
            }
        }

        writer.WriteEndArray();
    }

    // https://www.jerriepelser.com/blog/custom-converters-in-json-net-case-study-1/
    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        var result = new List<ChapterInfo>();

        if (reader.TokenType == JsonToken.StartArray)
        {
            var chapters = JArray.Load(reader);
            foreach (var chapter in chapters)
            {
                // only start is really required
                if (chapter["start"] == null)
                {
                    continue;
                }

                var start = chapter.Value<uint>("start");

                var title = chapter.Value<string>("title") ?? "";

                var length = chapter.Value<uint>("length");
                var end = start + length;


                var subtitle = chapter.Value<string>("subtitle");
                var chapterInfo = new ChapterInfo(start, title)
                {
                    Subtitle = subtitle,
                };
                if (end > start)
                {
                    chapterInfo.EndTime = end;
                }

                result.Add(chapterInfo);
            }
        }

        return result;
    }

    public override bool CanConvert(Type objectType)
    {
        return typeof(IList<ChapterInfo>).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo());
    }
}