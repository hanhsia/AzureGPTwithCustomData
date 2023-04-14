using Azure;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace Services.Azure.Storage
{
    /// <summary>
    /// 存储账号的Blob服务.
    /// </summary>
    public interface IBlobStorageService
    {
        /// <summary>
        /// Download blob
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        Task<bool> DownloadToAsync(Stream stream, string blobPath);

        /// <summary>
        /// Download blob
        /// </summary>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        Task<bool> DownloadToAsync(string path, string blobPath);

        /// <summary>
        /// 上传blob.
        /// </summary>
        /// <param name="stream">数据流.</param>
        /// <param name="blobPath">存储路径.</param>
        /// <returns></returns>
        Task<BlobContentInfo?> UploadBlobAsync(Stream stream, string blobPath, string? contentType = null);

        /// <summary>
        /// 上传blob.
        /// </summary>
        /// <param name="filePath">本地文件存储路径.</param>
        /// <param name="blobPath">Blob文件存储路径.</param>
        /// <returns></returns>
        Task<BlobContentInfo?> UploadBlobAsync(string filePath, string blobPath);

        //// <summary>
        /// 上传blob.
        /// </summary>
        /// <param name="bytes">文件数据.</param>
        /// <param name="blobPath">存储路径.</param>
        /// <returns></returns>
        Task<BlobContentInfo?> UploadBlobAsync(byte[] bytes, string blobPath, string? contentType = null);

        /// <summary>
        /// 删除文件.
        /// </summary>
        /// <param name="blobPath">存储路径.</param>
        /// <returns></returns>
        Task<bool> DeleteAsync(string blobPath);

        /// <summary>
        /// 获取文件链接地址.
        /// </summary>
        /// <param name="blobPath">存储路径.</param>
        /// <param name="expiryTime">过期时间.</param>
        /// <param name="permissions">读写权限.</param>
        /// <returns>返回文件链接地址.</returns>
        Task<Uri?> GetBlobUriAsync(string blobPath, DateTimeOffset? expiryTime = null, BlobSasPermissions permissions = BlobSasPermissions.Read);

        /// <summary>
        /// 获取数据块.
        /// </summary>
        /// <param name="blobPath">存储路径.</param>
        /// <returns>返回数据块Stream.</returns>
        Task<Stream?> GetBlobAsync(string blobPath);

        /// <summary>
        /// 获取数据库属性
        /// </summary>
        /// <param name="blobPath">存储路径.</param>
        /// <returns>返回数据块属性.</returns>
        Task<BlobProperties?> GetBlobPropertiesAsync(string blobPath);

        /// <summary>
        /// 获取Container下指定前缀下所有的blob
        /// </summary>
        /// <param name="blobContainerName">container name</param>
        /// <param name="prefix">prifex name</param>
        /// <returns>Pageble Blobs</returns>
        Task<AsyncPageable<BlobItem>?> GetBlobsAsync(string? blobContainerName, string? prefix);

        /// <summary>
        /// 获取Container下所有的blob
        /// </summary>
        /// <param name="blobContainerName">container name</param>
        /// <returns>Pageble Blobs</returns>
        Task<AsyncPageable<BlobItem>?> GetBlobsAsync(string blobContainerName);

        /// <summary>
        /// 获取Container下所有的blob
        /// </summary>
        /// <returns>Pageble Blobs</returns>
        Task<AsyncPageable<BlobItem>?> GetBlobsAsync();

        /// <summary>
        /// 判断数据块是否存在.
        /// </summary>
        /// <param name="filename">存储路径.</param>
        /// <returns>true: 存在，false: 不存在.</returns>
        Task<bool> BlockExistsAsync(string blobPath);
    }
}