using HRMS.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace HRMS.Infrastructure.Services;

/// <summary>
/// V1 implementation of IStorageService using local filesystem.
/// Files are stored in wwwroot/uploads/documents/...
/// </summary>
public class LocalStorageService : IStorageService
{
    private readonly string _webRootPath;

    public LocalStorageService(IWebHostEnvironment env)
    {
        _webRootPath = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    }

    public async Task<string> UploadAsync(Stream fileStream, string storedFileName, string subFolder)
    {
        var relativePath = Path.Combine("uploads", subFolder, storedFileName).Replace("\\", "/");
        var absolutePath = Path.Combine(_webRootPath, "uploads", subFolder);

        if (!Directory.Exists(absolutePath))
            Directory.CreateDirectory(absolutePath);

        var filePath = Path.Combine(absolutePath, storedFileName);

        using (var fileStreamTarget = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fileStreamTarget);
        }

        return relativePath;
    }

    public Task<bool> DeleteAsync(string filePath)
    {
        var absolutePath = GetAbsolutePath(filePath);

        if (File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public string GetAbsolutePath(string filePath)
    {
        return Path.Combine(_webRootPath, filePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
    }
}
