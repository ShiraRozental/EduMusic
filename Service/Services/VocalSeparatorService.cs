using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Service.Interfaces;
using System.Diagnostics;

namespace Service.Services
{
    public class VocalSeparatorService : IVocalSeparatorService
    {
        private readonly ILogger<VocalSeparatorService> _logger;
        private readonly IConfiguration _config;

        public VocalSeparatorService(ILogger<VocalSeparatorService> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public async Task<string> SeparateVocalsAsync(string inputFilePath, CancellationToken ct = default)
        {
            var outputDir = Path.Combine(Path.GetTempPath(), "demucs_out");
            Directory.CreateDirectory(outputDir);

            var pythonExe = _config["Demucs:PythonExecutable"] ?? "python";
            var model = _config["Demucs:Model"] ?? "htdemucs";

            _logger.LogInformation("Starting vocal separation for {File}", Path.GetFileName(inputFilePath));

            var psi = new ProcessStartInfo
            {
                FileName = pythonExe,
                ArgumentList =
            {
                "-m", "demucs",
                "--two-stems=vocals",
                "--model", model,
                "--out", outputDir,
                inputFilePath
            },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = psi };
            process.Start();

            var stdoutTask = process.StandardOutput.ReadToEndAsync(ct);
            var stderrTask = process.StandardError.ReadToEndAsync(ct);

            await process.WaitForExitAsync(ct);

            if (process.ExitCode != 0)
            {
                var stderr = await stderrTask;
                _logger.LogError("Demucs failed (exit {Code}): {Stderr}", process.ExitCode, stderr);
                throw new InvalidOperationException($"Demucs vocal separation failed: {stderr}");
            }

            var fileNameNoExt = Path.GetFileNameWithoutExtension(inputFilePath);
            var vocalsPath = Path.Combine(outputDir, model, fileNameNoExt, "vocals.wav");

            if (!File.Exists(vocalsPath))
                throw new FileNotFoundException($"Demucs output not found at path: {vocalsPath}");

            return vocalsPath;
        }

        public void CleanupOutput(string vocalsPath)
        {
            try
            {
                var jobDir = Path.GetDirectoryName(vocalsPath);
                if (jobDir != null && Directory.Exists(jobDir))
                    Directory.Delete(jobDir, recursive: true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to clean up Demucs output: {Path}", vocalsPath);
            }
        }
    }
}
