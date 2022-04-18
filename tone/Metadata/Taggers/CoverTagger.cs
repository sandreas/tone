using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading.Tasks;
using ATL;
using OperationResult;

namespace tone.Metadata.Taggers;
using static Helpers;

public class CoverTagger: TaggerBase
{
    private readonly IFileSystem _fs;
    private readonly IEnumerable<string> _covers;

    public CoverTagger(IFileSystem fs, IEnumerable<string> covers)
    {
        _fs = fs;
        _covers = covers;
    }
    public override async Task<Status<string>> UpdateAsync(IMetadata metadata)
    {

        foreach (var cover in _covers)
        {
            if (!_fs.File.Exists(cover))
            {
                return Error($"cover file does not exist: {cover}");
            }
            
            var picInfo = PictureInfo.fromBinaryData(await _fs.File.ReadAllBytesAsync(cover));
            picInfo.ComputePicHash();
            metadata.EmbeddedPictures.Add(picInfo);
        }

        return Ok();
    }
}