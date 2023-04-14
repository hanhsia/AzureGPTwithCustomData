using System.Text.Json.Serialization;

namespace Backend.Model
{
    public class DirectlineToken
    {
        [JsonPropertyName("conversationId")]
        public string ConversationId { get; set; } = string.Empty;

        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; } = 0;
    }
}