using System.Threading.Tasks;
using CliWrap;
using tone.Jobs.Options;
using tone.Metadata;

namespace tone.Executables;

public class Ffmpeg : Command
{
    public Ffmpeg(string targetPath="ffmpeg") : base(targetPath)
    {
        
    }
/*
    public Task<IMetadata> ExtractAsync(IMetadata track, ConversionOptions options)
    {
        
    }
    */
}