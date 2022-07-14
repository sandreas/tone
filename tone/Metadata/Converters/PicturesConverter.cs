using System;
using System.Collections.Generic;
using System.Reflection;
using ATL;
using ATL.AudioData;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace tone.Metadata.Converters;

public class PicturesConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        writer.WriteStartArray();
        if (value is IList<PictureInfo> pictures)
        {
            // var normalizedChapters = Normalize
            foreach (var picture in pictures)
            {
                writer.WriteStartObject();
                if (picture.PicType != PictureInfo.PIC_TYPE.Generic)
                {
                    writer.WritePropertyName("type");
                    writer.WriteValue(picture.PicType);
                }

                if (picture.NativePicCode != 0)
                {
                    writer.WritePropertyName("code");
                    writer.WriteValue(picture.NativePicCode);
                }

                writer.WritePropertyName("mimetype");
                writer.WriteValue(picture.MimeType);

                if (!string.IsNullOrEmpty(picture.Description))
                {
                    writer.WritePropertyName("description");
                    writer.WriteValue(picture.Description);
                }

                writer.WritePropertyName("data");
                writer.WriteValue(picture.PictureData);

                writer.WriteEndObject();
            }
        }

        writer.WriteEndArray();
    }

    // https://www.jerriepelser.com/blog/custom-converters-in-json-net-case-study-1/
    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        var result = new List<PictureInfo>();

        if (reader.TokenType == JsonToken.StartArray)
        {
            var pictures = JArray.Load(reader);
            var position = 1;
            foreach (var picture in pictures)
            {
                var base64 = picture.Value<string>("data") ?? "";
                if (base64 == "")
                {
                    continue;
                }

                var buffer = new Span<byte>(new byte[base64.Length]);
                if (!Convert.TryFromBase64String(base64, buffer, out _))
                {
                    continue;
                }

                var picTypeRaw = picture.Value<string>("type") ?? PictureInfo.PIC_TYPE.Generic.ToString();
                if (!Enum.TryParse<PictureInfo.PIC_TYPE>(picTypeRaw, out var picType))
                {
                    continue;
                }

                int? code = null;
                if (picture["code"] != null)
                {
                    code = picture.Value<int>("code");
                }

                var pictureInfo = PictureInfo.fromBinaryData(buffer.ToArray(), picType, MetaDataIOFactory.TagType.ANY,
                    code, position);
                pictureInfo.Description = picture.Value<string>("description") ?? "";
                pictureInfo.ComputePicHash();
                position++;
                result.Add(pictureInfo);
            }
        }

        return result;
    }

    public override bool CanConvert(Type objectType)
    {
        return typeof(IList<PictureInfo>).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo());
    }
}