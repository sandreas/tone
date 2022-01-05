using Microsoft.Extensions.Logging;
using tone.Options;
using tone.Services;

namespace tone.Commands;

public abstract class CommandBase
{
    protected readonly ILogger<CommandBase> _logger;

    protected CommandBase(ILogger<CommandBase> logger)
    {
        _logger = logger;
    }

    
    
}