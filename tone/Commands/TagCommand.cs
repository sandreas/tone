using System.Linq;
using System.Threading.Tasks;
using tone.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using tone.Metadata;
using tone.Metadata.Formats;
using tone.Metadata.Taggers;

namespace tone.Commands;


public class TagCommand  : AsyncCommand<TagCommandSettings>
{
    private readonly DirectoryLoaderService _dirLoader;
    private readonly ChptFmtNativeMetadataFormat _chapterFormat;
    private readonly GrokPatternService _grok;
    private readonly SpectreConsoleService _console;

    public TagCommand(SpectreConsoleService console, DirectoryLoaderService dirLoader, ChptFmtNativeMetadataFormat chapterFormat,
        GrokPatternService grok)
    {
        _console = console;
        _dirLoader = dirLoader;
        _chapterFormat = chapterFormat;
        _grok = grok;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, TagCommandSettings commandSettingsBase)
    {
        var audioExtensions = DirectoryLoaderService.ComposeAudioExtensions(commandSettingsBase.IncludeExtensions);
        var inputFiles = _dirLoader.FindFilesByExtension(commandSettingsBase.Input, audioExtensions);
        var inputFilesAsArray = inputFiles.ToArray();
        
        if (inputFilesAsArray.Length > 1 && !commandSettingsBase.AssumeYes)
        {
            if (!_console.Confirm( $"Tagging {inputFilesAsArray.Length} files, continue?"))
            {
                _console.WriteLine("aborted");
                return (int)ReturnCode.UserAbort;
            }
        }
        
        var tagger = new TaggerComposite();
        tagger.Taggers.Add(new MetadataTagger(commandSettingsBase));

        var customPatterns = commandSettingsBase.PathPatternExtension.Concat(new[]
        {
            "NOTDIRSEP [^/\\\\]*"
        });

        var grokDefinitions = await _grok.BuildAsync(commandSettingsBase.PathPattern, customPatterns);
        if (!grokDefinitions)
        {
            _console.Error.WriteLine("Could not parse `--path-pattern`: " + grokDefinitions.Error);
            return await Task.FromResult(1); // todo: error codes!
        }
        
        tagger.Taggers.Add(new PathPatternTagger(grokDefinitions.Value));
        //tagger.Taggers.Add(new ExtraFieldsTagger(commandSettingsBase.ExtraFields));
        tagger.Taggers.Add(new ExtraFieldsRemoveTagger(commandSettingsBase.RemoveAdditionalFields));
        if (IsTrue(commandSettingsBase.AutoImportChapters) || commandSettingsBase.ImportChaptersFile != "")
        {
            tagger.Taggers.Add(new ChptFmtNativeTagger(_dirLoader.FileSystem, _chapterFormat,
                commandSettingsBase.ImportChaptersFile));
        }

        tagger.Taggers.Add(new EquateTagger(commandSettingsBase.Equate));
        tagger.Taggers.Add(new M4BFillUpTagger());

        var tasks = inputFilesAsArray.Select(file => Task.Run(async () =>
            {
                var track = new MetadataTrack(file);
                var result = await tagger.Update(track);
                if (!result)
                {
                    _console.Error.WriteLine($"Could not update tags for file {file}: {result.Error}");
                    return;
                }
                
                if (!track.Save())
                {
                    _console.Error.WriteLine($"Could not save tags for {file}");
                }
            }))
            .ToList();

        await Task.WhenAll(tasks);
        return await Task.FromResult(1);
    }

    private static bool IsTrue(BooleanValue autoImportChapters)
    {
        return autoImportChapters == BooleanValue.True;
    }

    /*
    private static async Task<bool> Confirm(, string message, bool confirmIsDefault = false)
    {
        var confirmString = confirmIsDefault ? "[Y/n]" : "[y/N]";
        await _console.Output.WriteAsync($"{message} {confirmString}");
        var answer = await _console.Input.ReadLineAsync();
        return answer?.Trim().ToLower() != "y";
    }
    */
}