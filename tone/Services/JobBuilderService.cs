using System.Collections.Generic;
using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using tone.Models;

namespace tone.Services;

public class JobBuilderService
{
    private readonly ILogger<JobBuilderService> _logger;

    public JobBuilderService(ILogger<JobBuilderService> logger)
    {
        _logger = logger;
    }
    
    public IEnumerable<MergeJob> PrepareMerge((string, IEnumerable<IFileInfo>) inputFiles, IEnumerable<string> batchPatterns)
    {
        
        return new List<MergeJob>();
    }
}