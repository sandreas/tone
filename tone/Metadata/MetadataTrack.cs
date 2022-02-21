using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using ATL;
using ATL.AudioData;

namespace tone.Metadata;

enum MappingKey
{
    Group,
    SortName,
    SortAlbum,
    SortArtist,
    SortAlbumArtist,
    LongDescription,
    EncodingTool,
    PurchaseDate,
    MediaType,
}

enum FormatKey
{
    Mp4,
    Mp3,
}

public class MetadataTrack : Track, IMetadata
{
    public MetadataTrack(string path, IProgress<float>? writeProgress = null, bool load = true)
        : base(path, writeProgress, load)
    {
    }
    
    public MetadataTrack(IFileSystemInfo fileInfo, IProgress<float>? writeProgress = null, bool load = true)
        : base(fileInfo.FullName, writeProgress, load)
    {
    }

    private static readonly Dictionary<MappingKey, string?[]> AdditionalFieldMapping = new()
    {
        {
            MappingKey.Group, new[]
            {
                "©grp",
                "TIT1"
            }
        },
        {
            MappingKey.SortName, new[]
            {
                "sonm",
                "TSOT"
            }
        },
        {
            MappingKey.SortAlbum, new[]
            {
                "soal",
                "TSOA"
            }
        },
        {
            MappingKey.SortArtist, new[]
            {
                "soar",
                "TSOP"
            }
        },
        {
            MappingKey.SortAlbumArtist, new[]
            {
                "soaa",
                "TSO2"
            }
        },
        {
            MappingKey.LongDescription, new[]
            {
                "ldes",
                "TDES"
            }
        },
        {
            MappingKey.EncodingTool, new[]
            {
                "©too",
                null
            }
        },
        {
            MappingKey.PurchaseDate, new[]
            {
                "purd",
                null
            }
        },
        {
            MappingKey.MediaType, new[]
            {
                "stik",
                null // "TMED" is not correct here
            }
        },
    };

    public string? Group
    {
        get => GetAdditionalField(MappingKey.Group);
        set => SetAdditionalField(MappingKey.Group, value);
    }

    public string? SortName
    {
        get => GetAdditionalField(MappingKey.SortName);
        set => SetAdditionalField(MappingKey.SortName, value);
    }

    public string? SortAlbum
    {
        get => GetAdditionalField(MappingKey.SortAlbum);
        set => SetAdditionalField(MappingKey.SortAlbum, value);
    }

    public string? SortArtist
    {
        get => GetAdditionalField(MappingKey.SortArtist);
        set => SetAdditionalField(MappingKey.SortArtist, value);
    }

    public string? SortAlbumArtist
    {
        get => GetAdditionalField(MappingKey.SortAlbumArtist);
        set => SetAdditionalField(MappingKey.SortAlbumArtist, value);
    }

    public string? LongDescription
    {
        get => GetAdditionalField(MappingKey.LongDescription);
        set => SetAdditionalField(MappingKey.LongDescription, value);
    }

    public string? EncodingTool
    {
        get => GetAdditionalField(MappingKey.EncodingTool);
        set => SetAdditionalField(MappingKey.EncodingTool, value);
    }

    public DateTime? PurchaseDate
    {
        get => GetAdditionalFieldDate(MappingKey.PurchaseDate);
        set => SetAdditionalField(MappingKey.PurchaseDate, value);
    }

    public string? MediaType
    {
        get => GetAdditionalField(MappingKey.MediaType);
        set => SetAdditionalField(MappingKey.MediaType, value);
    }

    private string? GetAdditionalField(MappingKey key)
    {
        var resolvedKey = ResolveKey(AudioFormat, key);
        if (resolvedKey == null)
        {
            return null;
        }

        return AdditionalFields.ContainsKey(resolvedKey) ? AdditionalFields[resolvedKey] : null;
    }

    private DateTime? GetAdditionalFieldDate(MappingKey key)
    {
        var stringValue = GetAdditionalField(key);
        if (stringValue == null)
        {
            return null;
        }

        if (DateTime.TryParse(stringValue, out var result))
        {
            return result;
        }

        return null;
    }

    private void SetAdditionalField(MappingKey key, string? value)
    {
        var resolvedKey = ResolveKey(AudioFormat, key);
        if (resolvedKey == null)
        {
            return;
        }

        if (value != null)
        {
            AdditionalFields[resolvedKey] = value;
        }
        else if (AdditionalFields.ContainsKey(resolvedKey))
        {
            AdditionalFields.Remove(resolvedKey);
        }
    }

    private void SetAdditionalField(MappingKey key, DateTime? value)
    {
        SetAdditionalField(key, value?.ToString("yyyy/MM/dd"));
    }

    private string? ResolveKey(Format format, MappingKey key)
    {
        return format.ID switch
        {
            AudioDataIOFactory.CID_MP4 => ResolveKey(FormatKey.Mp4, key),
            AudioDataIOFactory.CID_MP3 => ResolveKey(FormatKey.Mp3, key),
            _ => null
        };
    }

    private string? ResolveKey(FormatKey formatKey, MappingKey key)
    {
        if (!AdditionalFieldMapping.ContainsKey(key))
        {
            return null;
        }

        var formatKeyAsInt = (int)formatKey;
        return AdditionalFieldMapping[key].Length < formatKeyAsInt ? null : AdditionalFieldMapping[key][formatKeyAsInt];
    }
}