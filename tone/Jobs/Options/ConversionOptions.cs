using System.IO.Abstractions;

namespace tone.Jobs.Options;

public class ConversionOptions : FormatOptions
{
    public IFileSystem FileSystem { get; set; }
    public IDirectoryInfo TemporaryDirectory { get; set; }
}