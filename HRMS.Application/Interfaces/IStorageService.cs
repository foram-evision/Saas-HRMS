namespace HRMS.Application.Interfaces;

/// <summary>
/// Abstraction over physical file storage.
/// V1 implementation: LocalStorageService (saves to wwwroot/uploads/).
/// Future: AzureBlobStorageService — swap only the DI registration, no other code changes.
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Persist a file stream and return the stored relative path (or blob URI).
    /// The path is suitable for storage in the database FilePath column.
    /// </summary>
    Task<string> UploadAsync(Stream fileStream, string storedFileName, string subFolder);

    /// <summary>
    /// Delete a file by its stored path. Returns false if file not found.
    /// </summary>
    Task<bool> DeleteAsync(string filePath);

    /// <summary>
    /// Resolve the absolute physical path from the stored relative path.
    /// Used by the download endpoint to serve the file.
    /// </summary>
    string GetAbsolutePath(string filePath);
}
