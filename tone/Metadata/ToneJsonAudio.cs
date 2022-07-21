using ATL;
using Sandreas.AudioMetadata;

namespace tone.Metadata;

public class ToneJsonAudio
{
    public int Bitrate { get; set; }
    public string Format { get; set; }
    public string FormatShort { get; set; }

    public double SampleRate { get; set; }
    public double Duration { get; set; }
    public bool Vbr { get; set; }
    public object? Channels { get; set; }
    public object? Frames { get; set; }
    public MetadataSpecification[] MetaFormat { get; set; }
}