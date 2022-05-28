using System.Linq;
using System.Threading.Tasks;
using tone.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using tone.Commands.Settings;
using tone.Matchers;
using tone.Metadata;
using tone.Metadata.Taggers;

namespace tone.Commands;

public class TagCommand : AsyncCommand<TagCommandSettings>
{
    private readonly DirectoryLoaderService _dirLoader;
    private readonly SpectreConsoleService _console;
    private readonly StartupErrorService _startup;
    private readonly PathPatternMatcher _pathPatternMatcher;
    private readonly TaggerComposite _tagger;

    public TagCommand(SpectreConsoleService console, StartupErrorService startup, DirectoryLoaderService dirLoader,
        PathPatternMatcher pathPatternMatcher, TaggerComposite tagger)
    {
        _console = console;
        _startup = startup;
        _dirLoader = dirLoader;
        _pathPatternMatcher = pathPatternMatcher;
        _tagger = tagger;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, TagCommandSettings settings)
    {
        if (_startup.HasErrors)
        {
            return _startup.ShowErrors(_console.Error.WriteLine);
        }

        var audioExtensions = DirectoryLoaderService.ComposeAudioExtensions(settings.IncludeExtensions);
        var inputFiles = _dirLoader.FindFilesByExtension(settings.Input, audioExtensions);
        var inputFilesAsArray = (settings.PathPattern.Length == 0
            ? inputFiles
            : inputFiles.Where(f => _pathPatternMatcher.TryMatchSinglePattern(f.FullName, out _))).ToArray();
        var packages = _dirLoader.BuildPackages(inputFilesAsArray, _pathPatternMatcher, settings.Input).ToArray();
        var fileCount = packages.Sum(p => p.Files.Count);

        if (!settings.DryRun)
        {
            switch (fileCount)
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

        var showDryRunMessage = false;

        var tasks = packages.Select(p => Task.Run(async () =>
            {
                foreach (var file in p.Files)
                {
                    var track = new MetadataTrack(file)
                    {
                        BasePath = p.BaseDirectory?.FullName
                    };
                    var status = await _tagger.UpdateAsync(track);

                    if (!status)
                    {
                        _console.Error.WriteLine($"Could not update tags for file {file}: {status.Error}");
                        return;
                    }

                    var currentMetadata = new MetadataTrack(file);
                    var diffListing = track.Diff(currentMetadata);
                    if (diffListing.Count == 0)
                    {
                        _console.Write(new Rule($"[green]unchanged: {Markup.Escape(track.Path ?? "")}[/]")
                            .LeftAligned());
                        return;
                    }


                    showDryRunMessage = settings.DryRun;
                    var diffTable = new Table().Expand();
                    diffTable.Title = new TableTitle($"[blue]DIFF: {Markup.Escape(track.Path ?? "")}[/]");
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
                    
                    if (settings.DryRun)
                    {
                        _console.Write(diffTable);
                        return;
                    }

                    var path = Markup.Escape(track.Path ?? "");
                    var message = !track.Save() ? $"[red]Update failed: {path}[/]" : $"[green]Updated: {path}[/]";
                    diffTable.Caption = new TableTitle(message);
                    _console.Write(diffTable);
                }
            }))
            .ToList();

        await Task.WhenAll(tasks);

        if (showDryRunMessage)
        {
            _console.WriteLine();
            _console.Write(new Markup("[blue]!!! This was a dry-run, no changes where actually saved !!![/]"));
        }

        return await Task.FromResult((int)ReturnCode.Success);
    }


    /*private Task<Result<ITagger, ReturnCode>> BuildTaggerCompositeAsync(TagSettingsBase settings,
        PathPatternMatcher matcher)
    {
        var tagger = new TaggerComposite();
        tagger.Taggers.Add(new MetadataTagger(settings));
        tagger.Taggers.Add(new CoverTagger(_dirLoader.FileSystem,
            settings.Covers, settings.AutoImport.Contains(AutoImportValue.Covers)));

        tagger.Taggers.Add(new PathPatternTagger(matcher));
        tagger.Taggers.Add(new AdditionalFieldsRemoveTagger(settings.RemoveAdditionalFields));
        if (settings.AutoImport.Contains(AutoImportValue.Chapters) || settings.ImportChaptersFile != "")
        {
            tagger.Taggers.Add(new ChptFmtNativeTagger(_dirLoader.FileSystem, _chapterFormat,
                settings.ImportChaptersFile));
        }

        tagger.Taggers.Add(new EquateTagger(settings.Equate));
        tagger.Taggers.Add(new M4BFillUpTagger());
        return Task.FromResult<Result<ITagger, ReturnCode>>(Ok((ITagger)tagger));
    }*/

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