using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Nodes;
using Xunit;

namespace tone.Tests.Integration;

public class ToneM4bIntegrationTest
{
    [Fact]
    public async Task DumpFfmetadata_FromGeneratedM4b_Works()
    {
        if (!IsCommandAvailable("ffmpeg", "-version"))
        {
            Skip("ffmpeg is not available in PATH.");
        }

        var tempDir = Path.Combine(Path.GetTempPath(), $"tone-it-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var m4bFile = Path.Combine(tempDir, "sample.m4b");
            var ffmpeg = await RunProcessAsync(
                "ffmpeg",
                "-hide_banner",
                "-loglevel", "error",
                "-f", "lavfi",
                "-i", "sine=frequency=1000:duration=1",
                "-c:a", "aac",
                "-b:a", "64k",
                "-f", "mp4",
                "-y",
                m4bFile
            );

            Assert.True(ffmpeg.ExitCode == 0, $"ffmpeg failed: {ffmpeg.Stderr}");
            Assert.True(File.Exists(m4bFile), "Expected generated .m4b fixture file.");

            var toneDll = ResolveToneDll();
            var dump = await RunProcessAsync(
                "dotnet",
                new[] { toneDll, "dump", m4bFile, "--format", "ffmetadata" },
                env: ("DOTNET_ROLL_FORWARD", "Major")
            );

            Assert.True(dump.ExitCode == 0, $"tone dump failed: {dump.Stderr}\n{dump.Stdout}");
            Assert.Contains(";FFMETADATA", dump.Stdout, StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Fact]
    public async Task DumpJson_FromGeneratedMp3WithLrc_FindsLyrics()
    {
        if (!IsCommandAvailable("ffmpeg", "-version"))
        {
            Skip("ffmpeg is not available in PATH.");
        }

        var tempDir = Path.Combine(Path.GetTempPath(), $"tone-it-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var mp3File = Path.Combine(tempDir, "sample.mp3");
            var ffmpeg = await RunProcessAsync(
                "ffmpeg",
                "-hide_banner",
                "-loglevel", "error",
                "-f", "lavfi",
                "-i", "sine=frequency=440:duration=1",
                "-c:a", "libmp3lame",
                "-q:a", "5",
                "-metadata", "lyrics-eng=[00:00.10]line one\\n[00:00.50]line two",
                "-y",
                mp3File
            );

            Assert.True(ffmpeg.ExitCode == 0, $"ffmpeg failed: {ffmpeg.Stderr}");
            Assert.True(File.Exists(mp3File), "Expected generated .mp3 fixture file.");

            var toneDll = ResolveToneDll();
            var dump = await RunProcessAsync(
                "dotnet",
                new[] { toneDll, "dump", mp3File, "--format", "json" },
                env: ("DOTNET_ROLL_FORWARD", "Major")
            );

            Assert.True(dump.ExitCode == 0, $"tone dump failed: {dump.Stderr}\n{dump.Stdout}");

            var parsed = JsonNode.Parse(dump.Stdout);
            var lyrics = parsed?["meta"]?["additionalFields"]?["lyrics-eng"]?.GetValue<string>();
            Assert.False(string.IsNullOrWhiteSpace(lyrics));
            Assert.Contains("line one", lyrics!, StringComparison.Ordinal);
            Assert.Contains("line two", lyrics, StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    private static bool IsCommandAvailable(string command, params string[] args)
    {
        try
        {
            var result = RunProcessAsync(command, args).GetAwaiter().GetResult();
            return result.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private static void Skip(string reason)
    {
        var skipType = AppDomain.CurrentDomain
            .GetAssemblies()
            .Select(a => a.GetType("Xunit.Sdk.SkipException", throwOnError: false))
            .FirstOrDefault(t => t != null);

        if (skipType == null)
        {
            throw new InvalidOperationException(reason);
        }

        var ctor = skipType.GetConstructors().OrderBy(c => c.GetParameters().Length).First();
        var ctorArgs = ctor.GetParameters()
            .Select(p => p.ParameterType == typeof(string)
                ? reason
                : p.HasDefaultValue
                    ? p.DefaultValue
                    : p.ParameterType.IsValueType
                        ? Activator.CreateInstance(p.ParameterType)
                        : null)
            .ToArray();

        throw (Exception)ctor.Invoke(ctorArgs);
    }

    private static string ResolveToneDll()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current != null)
        {
            var slnPath = Path.Combine(current.FullName, "tone.sln");
            if (File.Exists(slnPath))
            {
                var toneDll = Path.Combine(current.FullName, "tone", "bin", "Debug", "net8.0", "tone.dll");
                if (File.Exists(toneDll))
                {
                    return toneDll;
                }
            }

            current = current.Parent;
        }

        throw new FileNotFoundException("Could not locate built tone.dll.");
    }

    private static async Task<(int ExitCode, string Stdout, string Stderr)> RunProcessAsync(
        string fileName,
        params string[] args)
    {
        return await RunProcessAsync(fileName, args, env: null);
    }

    private static async Task<(int ExitCode, string Stdout, string Stderr)> RunProcessAsync(
        string fileName,
        string[] args,
        (string Key, string Value)? env)
    {
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        foreach (var arg in args)
        {
            psi.ArgumentList.Add(arg);
        }

        if (env != null)
        {
            psi.Environment[env.Value.Key] = env.Value.Value;
        }

        using var process = Process.Start(psi);
        if (process == null)
        {
            throw new InvalidOperationException($"Failed to start process {fileName}.");
        }

        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return (process.ExitCode, await stdoutTask, await stderrTask);
    }
}
