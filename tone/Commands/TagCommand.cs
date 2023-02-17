using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sandreas.AudioMetadata;
using Sandreas.SpectreConsoleHelpers.Commands;
using tone.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Rendering;
using tone.Commands.Settings;
using tone.Directives;
using tone.Matchers;
using tone.Metadata.Taggers;

namespace tone.Commands;

public class TagCommand : CancellableAsyncCommand<TagCommandSettings>
{
    private readonly DirectoryLoaderService _dirLoader;
    private readonly SpectreConsoleService _console;
    private readonly StartupErrorService _startup;
    private readonly PathPatternMatcher _pathPatternMatcher;
    private readonly TaggerComposite _tagger;
    private readonly JavaScriptApi _api;
    private readonly SerializerService _serializer;

    public TagCommand(SpectreConsoleService console, StartupErrorService startup, DirectoryLoaderService dirLoader,
        PathPatternMatcher pathPatternMatcher, TaggerComposite tagger,  SerializerService serializerService, JavaScriptApi api)
    {
        _console = console;
        _startup = startup;
        _dirLoader = dirLoader;
        _pathPatternMatcher = pathPatternMatcher;
        _tagger = tagger;
        _serializer = serializerService;
        _api = api; // unused but necessary (Dependency Injection Initialisation)
    }

    public override async Task<int> ExecuteAsync(CommandContext context, TagCommandSettings settings,
        CancellationToken cancellation)
    {
        // it throws a TaskCanceledException
        if (_startup.HasErrors)
        {
            return _startup.ShowErrors(_console.Error.WriteLine);
        }

        var audioExtensions = DirectoryLoaderService.ComposeAudioExtensions(settings.IncludeExtensions);
        var inputFiles = _dirLoader.FindFilesByExtension(settings.Input, audioExtensions);
        if (settings.PathPattern.Length > 0)
        {
            inputFiles = inputFiles.Where(f => _pathPatternMatcher.TryMatchSinglePattern(f.FullName, out _));
        }

        var inputFilesAsArray = inputFiles
            .Apply(new OrderByDirective(settings.OrderBy))
            .Apply(new LimitDirective(settings.Limit))
            .ToArray();

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

        var returnCode = ReturnCode.Success;
        var showDryRunMessage = false;
        foreach (var p in packages)
        {
            if (cancellation.IsCancellationRequested)
            {
                _console.Error.Write(new Markup($"[red]User cancelled on package: {p.BaseDirectory}[/]"));
                returnCode = ReturnCode.UserAbort;
                break;
            }

            var (retCode, dryRun) = await ProcessAudioBookPackage(p, settings, cancellation);
            if (dryRun)
            {
                showDryRunMessage = true;
            }

            if (retCode == ReturnCode.UserAbort)
            {
                returnCode = ReturnCode.UserAbort;
                break;
            }
        }
        // https://stackoverflow.com/questions/27238232/how-can-i-cancel-task-whenall
        // await Task.Run(() => Task.WaitAll(tasks), cancellation);

        if (showDryRunMessage)
        {
            _console.WriteLine();
            _console.Write(new Markup("[blue]!!! This was a dry-run, no changes where actually saved !!![/]"));
            _console.WriteLine();
        }

        return await Task.FromResult((int)returnCode);
    }

    private async Task<(ReturnCode returnCode, bool dryRun)> ProcessAudioBookPackage(AudioBookPackage p,
        TagCommandSettings settings,
        CancellationToken cancellation)
    {
        var shouldShowDryRunMessage = false;
        foreach (var file in p.Files)
        {
            try
            {
                if (cancellation.IsCancellationRequested)
                {
                    _console.Error.Write(new Markup($"[red]User cancelled on file: {file}[/]"));
                    return (ReturnCode.UserAbort, shouldShowDryRunMessage);
                }

                var track = new MetadataTrack(file)
                {
                    BasePath = p.BaseDirectory?.FullName
                };
                var status = await _tagger.UpdateAsync(track);
                
                if (!status)
                {
                    _console.Error.WriteLine($"Could not update tags for file {file}: {status.Error}");
                    continue;
                }
                var currentMetadata = new MetadataTrack(file);
                var diffListing = track.Diff(currentMetadata);

                var dryRun = settings.DryRun;
                var force = settings.Force;
                var hasDiff = diffListing.Count > 0;
                var shouldSave = !dryRun && (force || hasDiff);
                var shouldSerializeOutput = settings.Format != SerializerFormat.Default;
                var saveSuccessful = dryRun || !shouldSave;

                // save track if required
                if (shouldSave)
                {
                    saveSuccessful = track.Save();
                }
                
                shouldShowDryRunMessage = dryRun && hasDiff && !shouldSerializeOutput;
                
                var escapedPath = Markup.Escape(track.Path ?? "");
                var actionMessage = BuildActionMessage(escapedPath, dryRun, force, hasDiff, saveSuccessful);
                var ruleMessage = new Rule(actionMessage).LeftJustified();

                // save or force save has failed
                if (!saveSuccessful)
                {
                    _console.Error.Write(ruleMessage);
                    continue;
                }
                
                if (!hasDiff)
                {
                    // if NOT shouldSerializeOutput, show info and success messages, be quiet otherwise
                    if(!shouldSerializeOutput) {
                        _console.Write(ruleMessage);
                    }
                    continue;
                }

                if (shouldSerializeOutput)
                {
                    _console.WriteLine(await _serializer.SerializeAsync(track, settings.Format));
                }
                else
                {
                    _console.Write(BuildDiffTable(settings, diffListing, actionMessage, escapedPath));    
                }
            }
            catch (Exception e)
            {
                _console.Error.WriteException(e);
            }
        }

        return (ReturnCode.Success, shouldShowDryRunMessage);
    }

    private static string BuildActionMessage(string escapedPath, bool dryRun, bool force, bool hasDiff, bool saveSuccessful)
    {
        var actionMessageDescriptor = force && !hasDiff ? "Forced update" : "Update";
        if (!saveSuccessful)
        {
            return $"[red]{actionMessageDescriptor} failed: {escapedPath}[/]";
        }
        
        if (dryRun || (!force && !hasDiff))
        {
            return $"[green]unchanged: {escapedPath}[/]";
        }
        
        return $"[green]{actionMessageDescriptor}: {escapedPath}[/]";
    }
    private static IRenderable BuildDiffTable(TagCommandSettings settings, List<(MetadataProperty Property, object? CurrentValue, object? NewValue)> diffListing, string actionMessage, string escapedPath)
    {
        var diffTable = new Table().Expand();
        diffTable.Title = new TableTitle($"[blue]DIFF: {escapedPath}[/]");
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

        if (!settings.DryRun)
        {
            diffTable.Caption = new TableTitle(actionMessage);
        }

        return diffTable;
    }
}