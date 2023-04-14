namespace Services.Azure.Storage
{
    public class BlobStorageClientSettings
    {
        public string ConnectionString { get; set; } = string.Empty;

        //use managed identity if connection string is null.
        public string Uri { get; set; } = string.Empty;
        public string ContainerName { get; set; } = string.Empty;
        public string IdentityId { get; set; } = string.Empty;
    }
}