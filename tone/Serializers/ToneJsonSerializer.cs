using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sandreas.AudioMetadata;
using tone.Metadata;

namespace tone.Serializers;

public class ToneJsonSerializer : IMetadataSerializer
{
    private readonly ToneJsonMeta _toneJsonMeta;
    private readonly JsonSerializerSettings _settings;
    private readonly FileSystem _fs;

    public ToneJsonSerializer(FileSystem fs, ToneJsonMeta toneJsonMeta, JsonSerializerSettings settings)
    {
        _fs = fs;
        _toneJsonMeta = toneJsonMeta;
        _settings = settings;
    }

    public async Task<string> SerializeAsync(IMetadata metadata)
    {
        _toneJsonMeta.OverwriteProperties(metadata, metadata.MappedAdditionalFields.Keys);


        var container = new ToneJsonContainer()
        {
            Meta = _toneJsonMeta
        };

        if (metadata is MetadataTrack track)
        {
            try
            {
                container.Audio = new ToneJsonAudio
                {
                    Format = track.AudioFormat?.Name ?? "",
                    FormatShort = track.AudioFormat?.ShortName ?? "",
                    Bitrate = track.Bitrate,
                    SampleRate = track.SampleRate,
                    Duration = track.TotalDuration.TotalMilliseconds,
                    Vbr = track.IsVBR,
                    Channels = new
                    {
                        Count = track.ChannelsArrangement?.NbChannels, 
                        track.ChannelsArrangement?.Description
                    },
                    Frames = new
                    {
                        Offset = track.TechnicalInformation?.AudioDataOffset,
                        Length = track.TechnicalInformation?.AudioDataSize
                    },
                    MetaFormat = track.MetadataSpecifications
                };
            }
            catch (Exception e)
            {
                container.Audio = new ToneJsonAudio()
                {
                    Format = "Format detection error: " + e.Message
                };
            }
        }

        if (metadata.Path != "" && _fs.File.Exists(metadata.Path))
        {
            try
            {
                var file = _fs.FileInfo.New(metadata.Path);
                // basePath
                container.File = new ToneJsonFile
                {
                    Size = file.Length,
                    Created = file.CreationTime,
                    Modified = file.LastWriteTime,
                    Accessed = file.LastAccessTime,
                    Path = file.DirectoryName ?? "",
                    Name = file.Name,
                };
            }
            catch (Exception e)
            {
                container.File = new ToneJsonFile()
                {
                    Name = "Load file error: " + e.Message,
                };
            }
        }

        var result = JsonConvert.SerializeObject(container, _settings);
        return await Task.FromResult(result);
    }
}