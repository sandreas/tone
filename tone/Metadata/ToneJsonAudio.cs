using ATL;

namespace tone.Metadata;

public class ToneJsonAudio
{
    public int Bitrate { get; set; }
    public string Format { get; set; }
    public string FormatShort { get; set; }

    public double SampleRate { get; set; }
    public double Duration { get; set; }
    public ChannelsArrangements.ChannelsArrangement ChannelsArrangement { get; set; }
    public bool Vbr { get; set; }
}