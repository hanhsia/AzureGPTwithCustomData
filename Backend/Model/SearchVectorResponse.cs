using Microsoft.Bot.Streaming.Payloads;
using Newtonsoft.Json;

namespace Backend.Model
{

    public class SearchVectorResponse
    {
        [JsonProperty("result")]
        public List<SearchResult> Result { get; set; } = new List<SearchResult>();

        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("time")]
        public double Time { get; set; }
    }

    public class SearchResult
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("score")]
        public double Score { get; set; }

        [JsonProperty("payload")]
        public Dictionary<string, string>? Payload { get; set; } = null;
    }

}
