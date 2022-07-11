using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using tone.Common.Extensions.Stream;
using tone.Metadata.Formats;

namespace tone.Metadata.Serializers;

public class ToneJsonSerializer : IMetadataSerializer
{
    private readonly ToneJsonMeta _toneJsonMeta;
    private readonly JsonSerializerSettings _settings;
    private readonly IFileSystem _fs;

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
        
        if(metadata is MetadataTrack track)
        {
            /*
                         Stringify(track.AudioFormat, s => fileTable.AddRow("format", Markup.Escape(s)));
            Stringify(track.Bitrate, s => fileTable.AddRow("bitrate", Markup.Escape(s)));
            Stringify(track.SampleRate, s => fileTable.AddRow("sample-rate", Markup.Escape(s)));
            Stringify(track.TotalDuration, s => fileTable.AddRow("duration", Markup.Escape(s)));
            Stringify(track.IsVBR, s => fileTable.AddRow("vbr", Markup.Escape(s)));
            Stringify(track.ChannelsArrangement, s => fileTable.AddRow("channels", Markup.Escape(s)));
            Stringify(track.EmbeddedPictures.Count, s => fileTable.AddRow("embedded pictures", Markup.Escape(s)));
             */
            container.Audio = new ToneJsonAudio
            {
                Format = track.AudioFormat.Name,
                FormatShort = track.AudioFormat.ShortName,
                Bitrate = track.Bitrate,
                SampleRate = track.SampleRate,
                Duration = track.TotalDuration.Milliseconds,
                ChannelsArrangement = track.ChannelsArrangement,
                Vbr = track.IsVBR
            };
        };
        
        if(metadata.Path != "" && _fs.File.Exists(metadata.Path))
        {
            var file = _fs.FileInfo.FromFileName(metadata.Path);
            // basePath
            container.File = new ToneJsonFile()
            {
                Size = file.Length,
                Created = file.CreationTime,
                Modified = file.LastWriteTime,
                Accessed = file.LastAccessTime,
                Path = file.DirectoryName,
                Name = file.Name,
            };
        };
        
        var result = JsonConvert.SerializeObject(container, _settings);
        return await Task.FromResult(result);
    }
}