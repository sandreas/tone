using System.Collections.Generic;
using System.Threading.Tasks;
using tone.Jobs.Options;
using tone.Metadata;

namespace tone.Services;

public class SplitService
{
    private readonly ConversionService _converter;

    public SplitService(ConversionService converter)
    {
        _converter = converter;
    }


}