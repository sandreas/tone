using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using OperationResult;
using tone.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Rendering;
using tone.Metadata;
using tone.Metadata.Formats;
using tone.Metadata.Taggers;
using static OperationResult.Helpers;

namespace tone.Commands;

public class TagCommand : AsyncCommand<TagCommandSettings>
{
    private readonly DirectoryLoaderService _dirLoader;
    private readonly ChptFmtNativeMetadataFormat _chapterFormat;
    private readonly GrokPatternService _grok;
    private readonly SpectreConsoleService _console;

    public TagCommand(SpectreConsoleService console, DirectoryLoaderService dirLoader,
        ChptFmtNativeMetadataFormat chapterFormat,
        GrokPatternService grok)
    {
        _console = console;
        _dirLoader = dirLoader;
        _chapterFormat = chapterFormat;
        _grok = grok;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, TagCommandSettings settings)
    {
        var taggerResult = await BuildTaggerCompositeAsync(_console, settings);
        if (!taggerResult)
        {
            return await Task.FromResult((int)taggerResult.Error);
        }

        var tagger = taggerResult.Value;
        var audioExtensions = DirectoryLoaderService.ComposeAudioExtensions(settings.IncludeExtensions);
        var inputFiles = _dirLoader.FindFilesByExtension(settings.Input, audioExtensions);
        var inputFilesAsArray = inputFiles.ToArray();
        if (!settings.DryRun)
        {
            switch (inputFilesAsArray.Length)
            {
                case 0:
                    _console.Error.WriteLine("no files found to tag");
                    return (int)ReturnCode.GeneralError;
                case > 1 when !settings.AssumeYes &&
                              !_console.Confirm($"Tagging {inputFilesAsArray.Length} files, continue?"):
                    _console.WriteLine("user aborted");
                    return (int)ReturnCode.UserAbort;
            }
        }

        var tasks = inputFilesAsArray.Select(file => Task.Run(async () =>
            {
                var track = new MetadataTrack(file);
                var result = await tagger.UpdateAsync(track);
                if (!result)
                {
                    _console.Error.WriteLine($"Could not update tags for file {file}: {result.Error}");
                    return;
                }

                if (settings.DryRun)
                {
                    var currentMetadata = new MetadataTrack(file);
                    var diffListing = track.Diff(currentMetadata);
                    if (diffListing.Count == 0)
                    {
                        _console.Write(new Rule($"[green]unchanged: {Markup.Escape(track.Path ?? "")}[/]").LeftAligned());
                    }
                    else
                    {
                        var diffTable = new Table().Expand();
                        diffTable.Title = new TableTitle($"[red]DIFF: {Markup.Escape(track.Path ?? "")}[/]");
                        diffTable.AddColumn("property")
                            .AddColumn("current")
                            .AddColumn("new");
                        foreach (var (property, currentValue, newValue) in diffListing)
                        {
                            diffTable.AddRow(
                                property.ToString(),
                                Markup.Escape(newValue?.ToString() ?? "<null>"),
                                Markup.Escape(currentValue?.ToString() ?? "<null>")
                            );
                        }
                        _console.Write(diffTable);
                    }


                    
                    return;
                }
                if (!track.Save())
                {
                    _console.Error.Write(new Rule($"[red]FAIL: {Markup.Escape(track.Path ?? "")}[/]").LeftAligned());
                } else  {
                    _console.Write(new Rule($"[green]OK: {Markup.Escape(track.Path ?? "")}[/]").LeftAligned());
                }
            }))
            .ToList();

        await Task.WhenAll(tasks);

        if (settings.DryRun)
        {
            _console.WriteLine();
            _console.Write(new Markup("[blue]!!! This was a dry-run, no changes where actually saved !!![/]"));
        }

        return await Task.FromResult((int)ReturnCode.Success);
    }


    private async Task<Result<ITagger, ReturnCode>> BuildTaggerCompositeAsync(SpectreConsoleService console,
        TagSettingsBase settings)
    {
        var tagger = new TaggerComposite();
        tagger.Taggers.Add(new MetadataTagger(settings));
        tagger.Taggers.Add(new CoverTagger(_dirLoader.FileSystem ?? new FileSystem(),
            settings.Covers /*, settings.AutoImportCovers*/));

        var customPatterns = settings.PathPatternExtension.Concat(new[]
        {
            "NOTDIRSEP [^/\\\\]*",
            "PARTNUMBER \\b[0-9-.IVXLCDM]+\\b"
        });
        var grokDefinitions = await _grok.BuildAsync(settings.PathPattern, customPatterns);
        if (!grokDefinitions)
        {
            console.Error.WriteLine("Could not parse `--path-pattern`: " + grokDefinitions.Error);
            return Error(ReturnCode.GeneralError);
        }

        tagger.Taggers.Add(new PathPatternTagger(grokDefinitions.Value));
        tagger.Taggers.Add(new AdditionalFieldsRemoveTagger(settings.RemoveAdditionalFields));
        if (settings.AutoImportChapters || settings.ImportChaptersFile != "")
        {
            tagger.Taggers.Add(new ChptFmtNativeTagger(_dirLoader.FileSystem, _chapterFormat,
                settings.ImportChaptersFile));
        }

        tagger.Taggers.Add(new EquateTagger(settings.Equate));
        tagger.Taggers.Add(new M4BFillUpTagger());
        return Ok((ITagger)tagger);
    }

    /*
    private static async Task<bool> Confirm(, string message, bool confirmIsDefault = false)
    {
        var confirmString = confirmIsDefault ? "[Y/n]" : "[y/N]";
        await console.Output.WriteAsync($"{message} {confirmString}");
        var answer = await console.Input.ReadLineAsync();
        return answer?.Trim().ToLower() != "y";
    }
    */
}