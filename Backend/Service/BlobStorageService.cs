using Azure;
using Azure.Identity;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;

namespace Services.Azure.Storage
{
    /// <summary>
    /// 存储账号的Blob服务.
    /// </summary>
    public class BlobStorageService : IBlobStorageService
    {
        private readonly ILogger<BlobStorageService> _logger;

        private readonly BlobServiceClient? _blobServiceClient = null;

        private readonly BlobContainerClient? _blobContainerClient;

        private readonly BlobStorageClientSettings? _blobStorageClientOptions = null;

        private readonly string _uri = string.Empty;
        private readonly string? _containerName = null;

        public BlobStorageService(
            IOptions<BlobStorageClientSettings> options,
            ILogger<BlobStorageService> logger)
        {
            if (options == null)
            {
                logger.LogError("options is null in BlobStorageService constructor");
                throw new ArgumentNullException(nameof(options));
            }

            try
            {
                _logger = logger;
                _blobStorageClientOptions=options.Value;

                if (!string.IsNullOrEmpty(_blobStorageClientOptions.ConnectionString))
                {
                    _blobServiceClient = new BlobServiceClient(_blobStorageClientOptions.ConnectionString);
                    var uriString = GetUriFromConnectionString(_blobStorageClientOptions.ConnectionString);
                    if (!string.IsNullOrEmpty(uriString))
                    {
                        _uri= uriString;
                    }
                    else
                    {
                        _logger.LogError("blob storage url is null in BlobStorageService constructor");
                        throw new ArgumentNullException(nameof(_blobServiceClient));
                    }
                }
                else if (!string.IsNullOrEmpty(_blobStorageClientOptions.Uri))
                {
                    _uri=_blobStorageClientOptions.Uri;
                    try
                    {
                        var uri = new Uri(_blobStorageClientOptions.Uri);
                        if (!string.IsNullOrEmpty(_blobStorageClientOptions.IdentityId))
                        {
                            _blobServiceClient = new BlobServiceClient(
                                uri,
                                new ManagedIdentityCredential(_blobStorageClientOptions.IdentityId));
                        }
                        else
                        {
                            _blobServiceClient = new BlobServiceClient(
                                uri,
                                new ManagedIdentityCredential());
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Exception From new BlobServiceClient");
                    }
                }

                if (!string.IsNullOrEmpty(_blobStorageClientOptions.ContainerName))
                {
                    _containerName=_blobStorageClientOptions.ContainerName;
                    _blobContainerClient = GetContainerClientAsync(_blobStorageClientOptions.ContainerName).Result;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Exception From {nameof(BlobStorageService)}");
                throw;
            }
        }

        /// <summary>
        /// download blobo
        /// </summary>
        /// <param name="stream">stream</param>
        /// <param name="blobPath">blobPath</param>
        /// <param name="options">options</param>
        /// <returns></returns>
        public async Task<bool> DownloadToAsync(Stream stream, string blobPath)
        {
            try
            {
                var blobClient = await GetBlobFromBlobPathAsync(blobPath);
                if (blobClient != null)
                {
                    var options = new StorageTransferOptions
                    {
                        InitialTransferSize =32*1024*1024,
                        // Set the maximum number of workers that
                        // may be used in a parallel transfer.
                        MaximumConcurrency = Environment.ProcessorCount,

                        // Set the maximum length of a transfer to 32MB.
                        MaximumTransferSize = 32* 1024 * 1024
                    };
                    var response = await blobClient.DownloadToAsync(stream, null, options);
                    if (!response.IsError)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception From {nameof(DownloadToAsync)}");
            }

            return false;
        }

        /// <summary>
        /// download blobo
        /// </summary>
        /// <param name="localPath">localPath</param>
        /// <param name="blobPath">blobPath</param>
        /// <param name="options">options</param>
        /// <returns></returns>
        public async Task<bool> DownloadToAsync(string localPath, string blobPath)
        {
            try
            {
                var blobClient = await GetBlobFromBlobPathAsync(blobPath);
                if (blobClient != null)
                {
                    var options = new StorageTransferOptions
                    {
                        InitialTransferSize =32*1024*1024,
                        // Set the maximum number of workers that
                        // may be used in a parallel transfer.
                        MaximumConcurrency = Environment.ProcessorCount,

                        // Set the maximum length of a transfer to 32MB.
                        MaximumTransferSize = 32* 1024 * 1024
                    };
                    var response = await blobClient.DownloadToAsync(localPath, null, options);
                    if (!response.IsError)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception From {nameof(DownloadToAsync)}");
            }

            return false;
        }

        /// <summary>
        /// 上传blob.
        /// </summary>
        /// <param name="stream">数据流.</param>
        /// <param name="blobPath">存储路径.</param>
        /// <returns></returns>
        public async Task<BlobContentInfo?> UploadBlobAsync(Stream stream, string blobPath, string? contentType = null)
        {
            try
            {
                var blobClient = await GetBlobFromBlobPathAsync(blobPath);
                if (blobClient != null)
                {
                    Response<BlobContentInfo> response;
                    var options = new BlobUploadOptions
                    {
                        TransferOptions=new StorageTransferOptions
                        {
                            InitialTransferSize =32*1024*1024,
                            // Set the maximum number of workers that
                            // may be used in a parallel transfer.
                            MaximumConcurrency = Environment.ProcessorCount,
                            // Set the maximum length of a transfer to 50MB.
                            MaximumTransferSize = 32* 1024 * 1024
                        }
                    };

                    if (!string.IsNullOrEmpty(contentType))
                    {
                        response = await blobClient.UploadAsync(
                            content: stream,
                            httpHeaders: new BlobHttpHeaders { ContentType = contentType },
                            transferOptions: options.TransferOptions);
                    }
                    else
                    {
                        response = await blobClient.UploadAsync(stream, options);
                    }

                    return response.Value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception From {nameof(UploadBlobAsync)}");
            }
            return null;
        }

        /// <summary>
        /// 上传blob.
        /// </summary>
        /// <param name="filePath">本地文件存储路径.</param>
        /// <param name="blobPath">Blob文件存储路径.</param>
        /// <returns></returns>
        public async Task<BlobContentInfo?> UploadBlobAsync(string filePath, string blobPath)
        {
            try
            {
                var blobClient = await GetBlobFromBlobPathAsync(blobPath);
                if (blobClient != null)
                {
                    var options = new BlobUploadOptions
                    {
                        TransferOptions=new StorageTransferOptions
                        {
                            InitialTransferSize =32*1024*1024,
                            // Set the maximum number of workers that
                            // may be used in a parallel transfer.
                            MaximumConcurrency = Environment.ProcessorCount,
                            // Set the maximum length of a transfer to 50MB.
                            MaximumTransferSize = 32* 1024 * 1024
                        }
                    };
                    var response = await blobClient.UploadAsync(filePath, options);
                    return response.Value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception From {nameof(UploadBlobAsync)}");
            }
            return null;
        }

        /// <summary>
        /// 上传blob.
        /// </summary>
        /// <param name="bytes">文件数据.</param>
        /// <param name="blobPath">存储路径.</param>
        /// <returns></returns>
        public async Task<BlobContentInfo?> UploadBlobAsync(byte[] bytes, string blobPath, string? contentType = null)
        {
            try
            {
                var blobClient = await GetBlobFromBlobPathAsync(blobPath);
                if (blobClient != null)
                {
                    MemoryStream stream = new MemoryStream(bytes);
                    Response<BlobContentInfo> response;
                    var options = new BlobUploadOptions
                    {
                        TransferOptions=new StorageTransferOptions
                        {
                            InitialTransferSize =32*1024*1024,
                            // Set the maximum number of workers that
                            // may be used in a parallel transfer.
                            MaximumConcurrency = Environment.ProcessorCount,
                            // Set the maximum length of a transfer to 50MB.
                            MaximumTransferSize = 32* 1024 * 1024
                        }
                    };
                    if (!string.IsNullOrEmpty(contentType))
                    {
                        response = await blobClient.UploadAsync(
                            content: stream,
                            httpHeaders: new BlobHttpHeaders { ContentType = contentType },
                            transferOptions: options.TransferOptions);
                    }
                    else
                    {
                        response = await blobClient.UploadAsync(stream, options);
                    }

                    return response.Value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception From {nameof(UploadBlobAsync)}");
            }
            return null;
        }

        /// <summary>
        /// 获取文件链接地址.
        /// </summary>
        /// <param name="blobPath">存储路径.</param>
        /// <param name="expiryTime">过期时间.</param>
        /// <returns>返回文件链接地址.</returns>
        public async Task<Uri?> GetBlobUriAsync(string blobPath, DateTimeOffset? expiryTime = null, BlobSasPermissions permissions = BlobSasPermissions.Read)
        {
            try
            {
                var blobClient = await GetBlobFromBlobPathAsync(blobPath);

                if (blobClient!=null && blobClient.CanGenerateSasUri)
                {
                    // Create a SAS token that's valid for one hour.
                    BlobSasBuilder sasBuilder = new BlobSasBuilder()
                    {
                        BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
                        BlobName = blobClient.Name,
                        Protocol = SasProtocol.Https,
                        Resource = "bc"
                    };
                    sasBuilder.ExpiresOn = expiryTime ?? DateTimeOffset.UtcNow.AddHours(1);
                    sasBuilder.SetPermissions(permissions);

                    Uri sasUri = blobClient.GenerateSasUri(sasBuilder);

                    return sasUri;
                }

                _logger.LogWarning(@"BlobClient must be authorized with Shared Key credentials to create a service SAS.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception From {nameof(GetBlobUriAsync)}");
            }
            return null;
        }

        /// <summary>
        /// 删除文件.
        /// </summary>
        /// <param name="blobPath">存储路径.</param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(string blobPath)
        {
            try
            {
                var blobClient = await GetBlobFromBlobPathAsync(blobPath);
                if (blobClient != null)
                {
                    return await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception From {nameof(DeleteAsync)}");
            }
            return false;
        }

        /// <summary>
        /// 获取数据块.
        /// </summary>
        /// <param name="blobPath">存储路径.</param>
        /// <returns>返回数据块Stream.</returns>
        public async Task<Stream?> GetBlobAsync(string blobPath)
        {
            try
            {
                var blobClient = await GetBlobFromBlobPathAsync(blobPath);
                if (blobClient != null)
                {
                    return await blobClient.OpenReadAsync(new BlobOpenReadOptions(false));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception From {nameof(GetBlobAsync)}");
            }
            return null;
        }

        /// <summary>
        /// 获取Blob属性
        /// </summary>
        /// <param name="blobPath">存储路径</param>
        /// <returns>返回属性值</returns>
        public async Task<BlobProperties?> GetBlobPropertiesAsync(string blobPath)
        {
            try
            {
                var blobClient = await GetBlobFromBlobPathAsync(blobPath);
                if (blobClient != null)
                {
                    var propertis = await blobClient.GetPropertiesAsync();
                    return propertis.Value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception From {nameof(GetBlobPropertiesAsync)}");
            }
            return null;
        }

        /// <summary>
        /// 获取Container下指定前缀下所有的blob
        /// </summary>
        /// <param name="blobContainerName">container name</param>
        /// <param name="prefix">prifex name</param>
        /// <returns>Pageble Blobs</returns>
        public async Task<AsyncPageable<BlobItem>?> GetBlobsAsync(string? blobContainerName, string? prefix)
        {
            var result = AsyncPageable<BlobItem>.FromPages(new Page<BlobItem>[] { });
            try
            {
                if (string.IsNullOrEmpty(blobContainerName))
                {

                    result = _blobContainerClient?.GetBlobsAsync(prefix: prefix);
                }
                else
                {
                    var blobContainerClient = await GetContainerClientAsync(blobContainerName);
                    result = blobContainerClient?.GetBlobsAsync(prefix: prefix);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception From {nameof(GetBlobsAsync)}");
            }
            return result;
        }

        /// <summary>
        /// 获取Container下所有的blob
        /// </summary>
        /// <param name="blobContainerName">container name</param>
        /// <returns>Pageble Blobs</returns>
        public async Task<AsyncPageable<BlobItem>?> GetBlobsAsync(string blobContainerName)
        {
            var result = AsyncPageable<BlobItem>.FromPages(new Page<BlobItem>[] { });
            try
            {
                var blobContainerClient = await GetContainerClientAsync(blobContainerName);
                result = blobContainerClient?.GetBlobsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception From {nameof(GetBlobsAsync)}");
            }
            return result;
        }

        /// <summary>
        /// 获取Container下所有的blob
        /// </summary>
        /// <param name="blobContainerName">container name</param>
        /// <returns>Pageble Blobs</returns>
        public async Task<AsyncPageable<BlobItem>?> GetBlobsAsync()
        {
            await Task.CompletedTask;
            var result = AsyncPageable<BlobItem>.FromPages(new Page<BlobItem>[] { });
            if (_blobContainerClient != null)
            {
                try
                {
                    return _blobContainerClient?.GetBlobsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Exception From {nameof(GetBlobsAsync)}");
                    return result;
                }
            }
            return result;
        }

        /// <summary>
        /// 判断数据块是否存在.
        /// </summary>
        /// <param name="filename">存储路径.</param>
        /// <returns>true: 存在，false: 不存在.</returns>
        public async Task<bool> BlockExistsAsync(string blobPath)
        {
            try
            {
                var blobClient = await GetBlobFromBlobPathAsync(blobPath);
                if (blobClient != null)
                {
                    var response = await blobClient.ExistsAsync();
                    return response.Value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception From {nameof(BlockExistsAsync)}");
            }
            return false;
        }

        private string? GetUriFromConnectionString(string connectionString)
        {
            var settings = ParseConnectionString(connectionString);
            if (settings!=null)
            {
                if (settings.ContainsKey("DefaultEndpointsProtocol") &&
                   settings.ContainsKey("AccountName") &&
                   settings.ContainsKey("EndpointSuffix"))
                {
                    return $"{settings["DefaultEndpointsProtocol"]}://{settings["AccountName"]}.blob.{settings["EndpointSuffix"]}";
                }
            }
            return null;
        }

        private IDictionary<string, string>? ParseConnectionString(string connectionString)
        {
            IDictionary<string, string> settings = new Dictionary<string, string>();
            string[] settingArray = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < settingArray.Length; i++)
            {
                string[] keyValuePair = settingArray[i].Split('=', 2);
                if (keyValuePair.Length != 2)
                {
                    return null;
                }
                if (settings.ContainsKey(keyValuePair[0]))
                {
                    return null;
                }
                settings.Add(keyValuePair[0], keyValuePair[1]);
            }
            return settings;
        }

        private async Task<BlobClient?> GetBlobFromBlobPathAsync(string blobPath)
        {
            if (string.IsNullOrWhiteSpace(blobPath))
            {
                var msg = "blobPath is null in GetBlobFromBlobPath";
                _logger.LogError(msg);
                throw new ArgumentException(msg);
            }
            BlobContainerClient? containerClient;
            string filePathWithoutContainerName;
            string trimmedFilePath = blobPath.TrimStart('/', '\\');
            List<string> pathItems = trimmedFilePath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (blobPath[0] == '/' || blobPath[0] == '\\')  //blobPath include container
            {
                var count = pathItems.Count();
                if (count > 1)
                {
                    containerClient = await GetContainerClientAsync(pathItems[0]);

                    pathItems.RemoveAt(0);
                    //filePathWithoutContainerName = Path.Combine(pathItems.ToArray());
                    filePathWithoutContainerName = String.Join('/', pathItems.ToArray());
                }
                else
                {
                    var msg = $"blobPath:{blobPath} is incorrect in GetBlobFromBlobPath";
                    _logger.LogError(msg);
                    throw new ArgumentException(msg);
                }
            }
            else  //blobPath exclude container
            {
                if (_blobContainerClient != null)
                {
                    containerClient = _blobContainerClient;
                    //filePathWithoutContainerName = Path.Combine(pathItems.ToArray());
                    filePathWithoutContainerName = String.Join('/', pathItems.ToArray());
                }
                else
                {
                    var msg = $"blobPath:{blobPath} without default container, error in GetBlobFromBlobPath";
                    _logger.LogError(msg);
                    throw new ArgumentException(msg);
                }
            }

            return containerClient?.GetBlobClient(filePathWithoutContainerName);
        }

        private async Task<BlobContainerClient?> GetContainerClientAsync(string? blobContainerName = null)
        {
            var containerName = _containerName;
            if (!string.IsNullOrEmpty(blobContainerName))
            {
                containerName=blobContainerName;
            }

            if (!string.IsNullOrEmpty(containerName))
            {
                BlobContainerClient? containerClient = null;
                bool fallback = false;
                if (_blobServiceClient!=null)
                {
                    try
                    {
                        containerClient = _blobServiceClient.GetBlobContainerClient(containerName.ToLower());
                    }
                    catch
                    {
                        fallback = true;
                    }
                }
                else
                {
                    fallback= true;
                }
                if (fallback)
                {
                    //fallback to new BlobContainerClient directly
                    string containerEndpoint = $"{_uri}/{containerName}";

                    if (!string.IsNullOrEmpty(_blobStorageClientOptions?.IdentityId))
                    {
                        containerClient = new BlobContainerClient(new Uri(containerEndpoint),
                                                    new ManagedIdentityCredential(_blobStorageClientOptions.IdentityId));
                    }
                    else
                    {
                        containerClient = new BlobContainerClient(new Uri(containerEndpoint),
                                                    new ManagedIdentityCredential());
                    }
                }
                if (containerClient != null)
                {
                    await containerClient.CreateIfNotExistsAsync();
                    return containerClient;
                }
            }
            else
            {
                _logger.LogError($"can't find container name, error in GetContainerClient");
            }
            return null;
        }
    }
}