using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ATL;
using tone.Executables;
using tone.Jobs.Options;
using tone.Metadata;

namespace tone.Services;

public class ConversionService
{
    private readonly Ffmpeg _ffmpeg;

    public ConversionService(Ffmpeg ffmpeg)
    {
        _ffmpeg = ffmpeg;
    }
        
    public async Task<IEnumerable<IMetadata>> SplitAsync(IMetadata trackToSplit, ConversionOptions options)
    {
        var result = new List<IMetadata>();

        var chapters = trackToSplit.NormalizeChapters();
        foreach (var chapter in chapters)
        {
            var start = new TimeSpan(chapter.StartTime);
            TimeSpan? end = chapter.EndTime <= start.TotalMilliseconds ? null : new TimeSpan(chapter.EndTime);
            result.Add( await ExtractPartAsync(trackToSplit, options, start, end));
        }
        return result;
    }

    public async Task<IMetadata> ExtractPartAsync(IMetadata track, ConversionOptions options, TimeSpan start, TimeSpan? end=null)
    {
        var x = options.FileSystem.Path.GetTempPath();
        var y = options.FileSystem.Path.GetTempFileName();
        var fileName = "part.tmp";

        return await Task.FromResult(track);
        // _ffmpeg.Extract();
        /*
        $tmpOutputFile = new SplFileInfo((string)$outputFile . "-finished." . $inputFile->getExtension());
        $tmpOutputFileConverting = new SplFileInfo((string)$outputFile . "-converting." . $inputFile->getExtension());
        if ((!$outputFile->isFile() && !$tmpOutputFile->isFile()) || $options->force) {
            if ($tmpOutputFileConverting->isFile()) {
                unlink($tmpOutputFileConverting);
            }
            if ($outputFile->isFile()) {
                unlink($outputFile);
            }
            $command = [
                "-i", $inputFile,
                "-vn",
                "-ss", $start->format(),
            ];

            if ($length->milliseconds() > 0) {
                $command[] = "-t";
                $command[] = $length->format();
            }
            if (!$options->ignoreSourceTags) {
                $command[] = "-map_metadata";
                $command[] = "a";
                $command[] = "-map";
                $command[] = "a";
            }
            $command[] = "-acodec";
            $command[] = "copy";


            $this->appendParameterToCommand($command, "-f", $this->mapFormat($options->source->getExtension()));
            $this->appendParameterToCommand($command, "-y", $options->force);
                     */
    }
}