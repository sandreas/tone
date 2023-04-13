using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using OperationResult;
using Sandreas.AudioMetadata;
using tone.Metadata.Formats;
using static System.Array;
using static OperationResult.Helpers;

namespace tone.Metadata.Taggers;

public abstract class AbstractFilesystemTagger : ITagger
{
    private readonly IFileSystem _fs;
    private readonly IMetadataFormat _parser;
    private readonly string _forcedImportFilename;
    private readonly bool _autoImport;

    protected AbstractFilesystemTagger(IFileSystem? fileSystem, IMetadataFormat parser,
        string forcedImportFilename = "", bool autoImport=false)
    {
        _fs = fileSystem ?? new FileSystem();
        _parser = parser;
        _forcedImportFilename = forcedImportFilename;
        _autoImport = autoImport;
    }

    public async Task<Status<string>> UpdateAsync(IMetadata metadata, IMetadata? originalMetadata = null)
    {
        if (!_autoImport && _forcedImportFilename == "")
        {
            return Ok();
        }
        
        var audioFile = _fs?.FileInfo.FromFileName(metadata.Path);
        if (audioFile == null)
        {
            return Error($"Could not create fileInfo for file {metadata.Path}");
        }
        
        string? preferredFileName;

        IEnumerable<IFileInfo> metadataFiles;
        if (_forcedImportFilename == "")
        {
            preferredFileName = BuildPreferredFileName(audioFile);
            metadataFiles = _fs?.Directory.EnumerateFiles(audioFile.DirectoryName)
                .Select(f => _fs.FileInfo.FromFileName(f))
                .Where(FilterCallback).ToArray() ?? Empty<IFileInfo>();
        }
        else
        {
            preferredFileName = null;
            var forcedFile = _fs?.FileInfo.FromFileName(_forcedImportFilename);
            metadataFiles = Empty<IFileInfo>();
            
            if (forcedFile != null)
            {
                if (!forcedFile.Exists)
                {
                    forcedFile = _fs?.FileInfo.FromFileName(_fs.Path.Combine(audioFile.DirectoryName ?? "", _forcedImportFilename));
                }
                if (forcedFile is { Exists: true })
                {
                    metadataFiles = new[] { forcedFile };
                }
            }
        }
        
        var preferredFile = metadataFiles.First();
        if (preferredFileName != null)
        {
            preferredFile = metadataFiles.FirstOrDefault(f => f.Name == preferredFileName) ?? preferredFile;
        }
        
        await using var stream = _fs?.File.OpenRead(preferredFile.FullName);
        if (stream == null)
        {
            return Error($"Could not open file ${preferredFile.FullName}");
        }

        var parsedMeta = await _parser.ReadAsync(stream);
        return !parsedMeta ? Error(parsedMeta.Error) : TransferPropertiesCallback(parsedMeta.Value, metadata);
    }

    public static string ConcatPreferredFileName(IFileInfo audioFile, string suffix = "")
    {
        var extensionNoDot = audioFile.Extension.TrimStart('.');
        return string.Concat(audioFile.FullName.AsSpan(0, audioFile.FullName.Length - extensionNoDot.Length), suffix);
    }
    
    protected abstract string? BuildPreferredFileName(IFileInfo audioFile);

    protected abstract bool FilterCallback(IFileInfo f);
    
    protected abstract Status<string> TransferPropertiesCallback(IMetadata parsedMetaValue, IMetadata metadata);
}