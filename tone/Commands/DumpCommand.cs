using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Sandreas.AudioMetadata;
using Sandreas.SpectreConsoleHelpers.Commands;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;
using tone.Commands.Settings;
using tone.Common.TextWriters;
using tone.Directives;
using tone.Metadata.Taggers;
using tone.Services;


namespace tone.Commands;

public class DumpCommand : CancellableAsyncCommand<DumpCommandSettings>
{
    private const string NoSpecSuffix = "tone-dump.txt";
    private readonly SerializerService _serializerService;
    private readonly DirectoryLoaderService _dirLoader;
    private readonly SpectreConsoleService _console;
    private readonly ILogger _logger;
    private readonly FileSystem _fs;

    public DumpCommand(ILogger logger, SpectreConsoleService console, DirectoryLoaderService dirLoader,
        SerializerService serializerService, FileSystem fs)
    {
        _console = console;
        _dirLoader = dirLoader;
        _serializerService = serializerService;
        _logger = logger;
        _fs = fs;
    }
    private async Task<ReturnCode> SpectreConsoleDelegate(IFileSystem fs, string outputFile, IEnumerable<string> lines,  CancellationToken ct) 
    {
        foreach (var line in lines)
        {
            _console.WriteLine(line);
        }
        return await Task.FromResult(ReturnCode.Success);
    }
    
    private async Task<ReturnCode> NoBreakDelegate(IFileSystem fs, string outputFile, IEnumerable<string> lines,  CancellationToken ct) 
    {
        foreach (var line in lines)
        {
            _console.WriteNoBreakLine(line);
        }
        return await Task.FromResult(ReturnCode.Success);
    }
    
    private async Task<ReturnCode> WriteFileDelegate(IFileSystem fs, string outputFile, IEnumerable<string> lines,  CancellationToken ct) 
    {
        try
        {
            await fs.File.WriteAllLinesAsync(outputFile, lines, ct);
        }
        catch (Exception)
        {
            return ReturnCode.GeneralError;
        }

        return ReturnCode.Success;
    }
    public override async Task<int> ExecuteAsync(CommandContext context, DumpCommandSettings settings, CancellationToken cancellationToken)
    {
        var audioExtensions = DirectoryLoaderService.ComposeAudioExtensions(settings.IncludeExtensions);
        var inputFiles = _dirLoader.FindFilesByExtension(settings.Input, audioExtensions)
            .Apply(new OrderByDirective(settings.OrderBy))
            .Apply(new LimitDirective(settings.Limit))
            .ToArray();
        var returnCode = ReturnCode.Success;
        
        var outputDelegate = SpectreConsoleDelegate;
        var outputSuffix = "";

        
        // default format is not supported for file export
        if(settings.Export)
        {
            if(settings.Format == SerializerFormat.Default) {
                settings.Format = SerializerFormat.Json;
            }
            
            // outputDelegate = async  lines => ;
            outputDelegate = WriteFileDelegate;
            
            outputSuffix = settings.Format switch
            {
                SerializerFormat.Json => ToneJsonTagger.DefaultFileSuffix,
                SerializerFormat.ChptFmtNative => ChptFmtNativeTagger.DefaultFileSuffix,
                SerializerFormat.Ffmetadata => FfmetadataTagger.DefaultFileSuffix,
                _ => NoSpecSuffix
            };
        }
        else if(settings.Format != SerializerFormat.Default && Console.IsOutputRedirected)
        {
            // Output width needs to be ajusted when output is redirected and format is json
            outputDelegate = NoBreakDelegate;
        }
        
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
                        return await outputDelegate(_fs, AbstractFilesystemTagger.ConcatPreferredFileName(file, NoSpecSuffix), tokens.Select(t => t.ToString()), cancellationToken);
                    }
                    catch (Exception e)
                    {
                        _console.Error.WriteException(e);
                        return await Task.FromResult(ReturnCode.GeneralError);
                    }
                }
                
                return await outputDelegate(_fs, AbstractFilesystemTagger.ConcatPreferredFileName(file, outputSuffix), new []{serializeResult}, cancellationToken);
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
                _console.Error.WriteLine(output);
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