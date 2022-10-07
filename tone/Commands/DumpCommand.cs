using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Sandreas.AudioMetadata;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;
using tone.Commands.Settings;
using tone.Common.TextWriters;
using tone.Directives;
using tone.Services;


namespace tone.Commands;

public class DumpCommand : AsyncCommand<DumpCommandSettings>
{
    private readonly SerializerService _serializerService;
    private readonly DirectoryLoaderService _dirLoader;
    private readonly SpectreConsoleService _console;
    private readonly ILogger _logger;

    public DumpCommand(ILogger logger, SpectreConsoleService console, DirectoryLoaderService dirLoader,
        SerializerService serializerService)
    {
        _console = console;
        _dirLoader = dirLoader;
        _serializerService = serializerService;
        _logger = logger;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, DumpCommandSettings settings)
    {
        var audioExtensions = DirectoryLoaderService.ComposeAudioExtensions(settings.IncludeExtensions);
        var inputFiles = _dirLoader.FindFilesByExtension(settings.Input, audioExtensions)
            .Apply(new OrderByDirective(settings.OrderBy))
            .Apply(new LimitDirective(settings.Limit))
            .ToArray();
        var returnCode = ReturnCode.Success;
        foreach (var file in inputFiles)
        {
            var fileReturnCode = await SuppressConsoleOutputAsync(async () =>
            {
                var track = new MetadataTrack(file);
                if (settings.IncludeProperties.Length > 0)
                {
                    track.ClearProperties(settings.IncludeProperties);
                }

                if (settings.ExcludeProperties.Length > 0)
                {
                    var propertiesToKeep =
                        MetadataExtensions.MetadataProperties.Where(p => !settings.ExcludeProperties.Contains(p));
                    track.ClearProperties(propertiesToKeep);
                }

                var serializeResult = await _serializerService.SerializeAsync(track, settings.Format);
                if (settings.Format == SerializerFormat.Json && settings.Query != "")
                {
                    try
                    {
                        var o = JObject.Parse(serializeResult);
                        var tokens = o.SelectTokens(settings.Query);
                        foreach (var token in tokens)
                        {
                            _console.WriteLine(token.ToString());
                        }
                    }
                    catch (Exception e)
                    {
                        _console.Error.WriteException(e);
                        return await Task.FromResult(ReturnCode.GeneralError);
                    }
                }
                else
                {
                    _console.WriteLine(serializeResult);
                }

                return ReturnCode.Success;
            });

            if (fileReturnCode != ReturnCode.Success)
            {
                returnCode = fileReturnCode;
            }
        }

        return await Task.FromResult((int)returnCode);
    }

    private async Task<ReturnCode> SuppressConsoleOutputAsync(Func<Task<ReturnCode>> callback)
    {
        // atldotnet pollutes Console with messages
        // this is handled here in the following way:
        // - Logger writes warning to logfile, if --debug is given or --log-level is set and value for --log-file is a valid file (default: TMPDIR/tone.log)
        // - All console out is suppressed and redirected into the logfile
        var consoleOutOriginal = Console.Out;
        try
        {
            var suppressedConsoleOutput = false;
            Console.SetOut(new CallbackTextWriter((output) =>
            {
                if (!suppressedConsoleOutput)
                {
                    _logger.Warning("Suppressed console output");    
                }
                _logger.Warning("{Output}",output);
                suppressedConsoleOutput = true;
            }));
            
            var returnCode = await callback();
            if (suppressedConsoleOutput)
            {
                returnCode = ReturnCode.SuppressedConsoleOutput;
            }
            return returnCode;
        }
        catch (Exception e)
        {
            _console.Error.WriteException(e);
            return ReturnCode.UncaughtException;
        }
        finally
        {
            Console.SetOut(consoleOutOriginal);
        }
    }
}