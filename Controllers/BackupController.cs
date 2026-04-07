using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Andreev.Services;

namespace Andreev.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BackupController : ControllerBase
{
    private readonly IBackupService _backupService;
    private readonly ILogger<BackupController> _logger;

    public BackupController(IBackupService backupService, ILogger<BackupController> logger)
    {
        _backupService = backupService;
        _logger = logger;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateBackup()
    {
        try
        {
            var fileName = await _backupService.CreateBackupAsync();
            _logger.LogInformation($"Бэкап создан: {fileName}");
            return Ok(new { message = "Backup created successfully", file = fileName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании бэкапа");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}