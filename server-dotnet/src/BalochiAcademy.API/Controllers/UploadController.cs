using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BalochiAcademy.API.Controllers;

[ApiController, Route("api")]
public class UploadController(IWebHostEnvironment env, ILogger<UploadController> log) : ControllerBase
{
    private static readonly HashSet<string> AllowedExts = new(StringComparer.OrdinalIgnoreCase)
    {
        // Documents
        ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx", ".txt", ".csv",
        // Images
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg", ".bmp", ".ico",
        // Audio / Video
        ".mp4", ".webm", ".ogg", ".mp3", ".wav", ".m4a",
        // Archives
        ".zip", ".rar", ".7z",
    };

    /// <summary>POST /api/admin/upload — upload a file and return its public URL.</summary>
    /// <param name="file">The file to upload (multipart/form-data).</param>
    /// <param name="folder">Sub-folder under uploads/ (alphanumeric, -, _ only). Default: misc.</param>
    [HttpPost("admin/upload"), Authorize(Policy = "AdminOnly")]
    [RequestSizeLimit(52_428_800)] // 50 MB
    public async Task<IActionResult> Upload(IFormFile? file, [FromQuery] string? folder, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "No file provided." });

        if (file.Length > 52_428_800)
            return BadRequest(new { error = "File exceeds the 50 MB limit." });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExts.Contains(ext))
            return BadRequest(new { error = $"File type '{ext}' is not permitted." });

        // Sanitise folder param: only letters, digits, hyphens, underscores
        var safeFolder = string.IsNullOrWhiteSpace(folder)
            ? "misc"
            : new string(folder.Where(c => char.IsLetterOrDigit(c) || c is '-' or '_').ToArray());

        var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        var dir     = Path.Combine(webRoot, "uploads", safeFolder);
        Directory.CreateDirectory(dir);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(dir, fileName);

        await using var stream = System.IO.File.Create(filePath);
        await file.CopyToAsync(stream, ct);

        var url = $"/uploads/{safeFolder}/{fileName}";
        log.LogInformation("Uploaded {Original} → {Url} ({Bytes} bytes)", file.FileName, url, file.Length);
        return Ok(new { url });
    }
}
