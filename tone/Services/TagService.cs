using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace tone.Services;

public class TagService: ITagService
{
    private readonly ILogger<TagService> _logger;

    public TagService(ILogger<TagService> logger)
    {
        _logger = logger;
    }
    public void DoSomething() {
        _logger.LogError("TagService.DoSomething");
    }
    
}