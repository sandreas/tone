namespace tone.Jobs.Options;

public class FormatOptions
{
    public string Format { get; set; } = "";
    public string Codec { get; set; } = "";
    public int Channels { get; set; }
    public int BitRate { get; set; }
    public int SampleRate { get; set; }
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