using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ATL;
using CliWrap;
using CliWrap.Buffered;
using Spectre.Console.Cli;
using tone.Commands.Settings;
using tone.Interceptors;

namespace tone.Commands;

public class SplitCommand : AsyncCommand<SplitCommandSettings>
{
    public SplitCommand(CommandSettingsProvider o)
    {
        if (o.Settings is SplitCommandSettings s)
        {
            Console.WriteLine("splitcommandsettings");
        }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, SplitCommandSettings settings)
    {
        
        var t = new Track();
        
        
        
        
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(10));

        var x = Cli.Wrap("pwd");


        // .WithArguments("test")
        var y = await x.WithWorkingDirectory("/tmp")
                .WithArguments(Array.Empty<string>())
                .WithEnvironmentVariables(new Dictionary<string, string?>
                {
                    // ["GIT_AUTHOR_NAME"] = "John",
                    // ["GIT_AUTHOR_EMAIL"] = "john@email.com"
                })
                .ExecuteBufferedAsync(cts.Token)
            ;
        Console.WriteLine("stdout:");
        Console.WriteLine(y.StandardOutput);
        Console.WriteLine("stderr:");
        Console.WriteLine(y.StandardError);
        Console.WriteLine("code:");
        Console.WriteLine(y.ExitCode);


        return await Task.FromResult((int)ReturnCode.Success);
    }
}

/*
$options = new FileConverterOptions();
    $options->source = $sourceFile;
    $options->destination = $destinationFile;
    $options->tempDir = $outputTempDir;
    $options->extension = $this->optAudioExtension;
    $options->codec = $this->optAudioCodec;
    $options->format = $this->optAudioFormat;
    $options->channels = $this->optAudioChannels;
    $options->sampleRate = $this->optAudioSampleRate;
    $options->vbrQuality = (float)$this->optAudioVbrQuality ?? 0;
    $options->bitRate = $this->optAudioBitRate;
    $options->force = $this->optForce;
    $options->debug = $this->optDebug;
    $options->profile = $this->input->getOption(static::OPTION_AUDIO_PROFILE);
$options->trimSilenceStart = (bool)$this->input->getOption(static::OPTION_TRIM_SILENCE);
$options->trimSilenceEnd = (bool)$this->input->getOption(static::OPTION_TRIM_SILENCE);
$options->ignoreSourceTags = $this->input->getOption(static::OPTION_IGNORE_SOURCE_TAGS);
$options->noConversion = $this->input->getOption(static::OPTION_NO_CONVERSION);
return $options;
*/