// namespace tone.Commands;

// [Command("merge")]
// public class MergeCommand : ICommand
// {
//     private readonly ILogger _logger;
//     private readonly DirectoryLoaderService _dirLoader;
//     private readonly FileWalker _fileWalker;
//
//     public MergeCommand(ILogger logger, FileWalker fileWalker, DirectoryLoaderService dirLoader)
//     {
//         _logger = logger;
//         _fileWalker = fileWalker;
//         _dirLoader = dirLoader;
//     }
//
//
// //     public async Task<int> ExecuteAsync2(MergeOptions options)
// //     {
// // // todo: fix this with a . and some content
// //         var audioExtensions = DirectoryLoaderService.ComposeAudioExtensions(options.IncludeExtensions);
// //         var inputFiles = _dirLoader.SeekFiles(options.Input, audioExtensions).ToList();
// //         Console.WriteLine(inputFiles.First().Item1 + ": " + string.Join(", ", inputFiles.First().Item2));
//
//         /*
//         Steps for batch pattern parsing
//         - Normalize batch pattern (remove leading /, replace \ with /)
//         - Normalize input directory
//         - Split input directory
//         
//         var jobs = _jobBuilder.PrepareMerge(options);
//         foreach (var job in jobs)
//         {
//             // var mergeTask = _taskBuilder.CreateMergeTask(job);
//             // _taskRunner.Enqueue(task);
//         }
//         
//         // return await _taskRunner.ExecuteAsync();
//         
//         
//         _logger.LogError(string.Join(',', options.Input));
//         _logger.LogError(options.Output);
// */
//         /*
//         var fs = new FileSystem();
//         var fileInfos = FileSystemEnumerator.Walk(fs, "../../").Where(p => p.Contains("tone")); //.Select(p => fs.FileInfo.FromFileName(p));
//         foreach (var f in fileInfos)
//         {
//             // Console.WriteLine("MergeCommand.ExecuteAsync: " + f.Name + ", " + f.CreationTime);
//             // Console.WriteLine("MergeCommand.ExecuteAsync: " + f );
//         }
//         */
//     //     return await Task.FromResult(0);
//     // }
//
//     public async ValueTask ExecuteAsync(IConsole console)
//     {
//         await console.Output.WriteLineAsync("hello merge command");
//     }
// }