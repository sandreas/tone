using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OperationResult;
using Sandreas.AudioMetadata;
using tone.Commands.Settings.Interfaces;

namespace tone.Metadata.Taggers;

using static Helpers;

public class ToneJsonTagger : INamedTagger
{
    public string Name => nameof(ToneJsonTagger);

    private readonly List<IFileInfo> _toneJsonFiles;
    private readonly bool _autoload;
    private readonly IFileSystem _fs;


    public ToneJsonTagger(IFileSystem fs, IEnumerable<string> toneJsonFiles, bool autoload = false)
    {
        _fs = fs;
        _toneJsonFiles = toneJsonFiles.Select(f => fs.FileInfo.FromFileName(f)).ToList();
        _autoload = autoload;
    }

    public ToneJsonTagger(IFileSystem fs, IToneJsonTaggerSettings settings)
    {
        _fs = fs;
        _toneJsonFiles = settings.ToneJsonFiles.Select(f => fs.FileInfo.FromFileName(f)).ToList();
        _autoload = settings.AutoImportToneJson;
    }

    public async Task<Status<string>> UpdateAsync(IMetadata metadata, IMetadata? originalMetadata = null)
    {
        if (_autoload && _toneJsonFiles.Count == 0 && metadata.BasePath != null)
        {
            _toneJsonFiles.AddRange(_fs.FindMatchingFiles(metadata.BasePath, "tone.json"));
        }


        var validMetadataFiles = _toneJsonFiles.Where(f => f.Exists);

        try
        {
            foreach (var file in validMetadataFiles)
            {
                var json = await _fs.File.ReadAllTextAsync(file.FullName);
                var toneJson = JsonConvert.DeserializeObject<ToneJsonContainer>(json);
                if (toneJson != null)
                {
                    metadata.OverwritePropertiesWhenNotEmpty(toneJson.Meta);
                }
            }
        }
        catch (Exception e)
        {
            return Error(e.Message);
        }


        return Ok();
    }
}