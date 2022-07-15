using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Sandreas.AudioMetadata;
using Spectre.Console;
using Spectre.Console.Cli;
using tone.Commands.Settings;
using tone.Directives;
using tone.Services;


namespace tone.Commands;

public class DumpCommand : AsyncCommand<DumpCommandSettings>
{
    private readonly SerializerService _serializerService;
    private readonly DirectoryLoaderService _dirLoader;
    private readonly SpectreConsoleService _console;

    public DumpCommand(SpectreConsoleService console, DirectoryLoaderService dirLoader,
        SerializerService serializerService)
    {
        _console = console;
        _dirLoader = dirLoader;
        _serializerService = serializerService;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, DumpCommandSettings settings)
    {
        var audioExtensions = DirectoryLoaderService.ComposeAudioExtensions(settings.IncludeExtensions);
        var inputFiles = _dirLoader.FindFilesByExtension(settings.Input, audioExtensions)
            .Apply(new OrderByDirective(settings.OrderBy))
            .Apply(new LimitDirective(settings.Limit))
            .ToArray();
        
        foreach (var file in inputFiles)
        {
            var track = new MetadataTrack(file);

            if (settings.IncludeProperties.Length > 0)
            {
                track.ClearProperties(settings.IncludeProperties);
            }

            var serializeResult = await _serializerService.SerializeAsync(track, settings.Format);
            if(settings.Format == SerializerFormat.Json && settings.Query != "")
            {
                try
                {
                    var o =  JObject.Parse(serializeResult);
                    var tokens = o.SelectTokens(settings.Query);
                    foreach(var token in tokens) {
                        _console.WriteLine(token.ToString());
                    }
                }
                catch (Exception e)
                {
                    _console.WriteException(e);
                    return await Task.FromResult((int)ReturnCode.GeneralError);
                }
            }
            else
            {
                _console.WriteLine(serializeResult);
            }
            
        }

        return await Task.FromResult((int)ReturnCode.Success);
    }


}