using Azure;
using Microsoft.Extensions.Options;

namespace Backend.Service
{
    public class OpenAIClientSettings
    {
        public int ProviderType { get; set; } = 1;
        public string Organization { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
    }
}
