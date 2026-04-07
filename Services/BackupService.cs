using System.Diagnostics;

namespace Andreev.Services;

public interface IBackupService
{
    Task<string> CreateBackupAsync();
}

public class BackupService : IBackupService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<BackupService> _logger;
    private readonly string _pgDumpPath = @"C:\Program Files\PostgreSQL\17\bin\pg_dump.exe";
    private readonly string _dbName = "mycust";
    private readonly string _dbUser = "postgres";
    private readonly string _dbPassword = "postgres";
    private readonly string _dbHost = "localhost";
    private readonly int _dbPort = 5433;

    public BackupService(IWebHostEnvironment env, ILogger<BackupService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task<string> CreateBackupAsync()
    {
        var backupFileName = $"mycust_{DateTime.Now:yyyyMMdd_HHmmss}.sql";
        var backupPath = Path.Combine(_env.WebRootPath, "backups", backupFileName);

        Directory.CreateDirectory(Path.GetDirectoryName(backupPath)!);

        var args = $"-h {_dbHost} -p {_dbPort} -U {_dbUser} -d {_dbName} -f \"{backupPath}\"";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _pgDumpPath,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        process.StartInfo.EnvironmentVariables["PGPASSWORD"] = _dbPassword;

        process.Start();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync();
            _logger.LogError($"pg_dump failed: {error}");
            throw new Exception($"Backup failed: {error}");
        }

        return backupFileName;
    }
}