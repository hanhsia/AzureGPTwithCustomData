using System.Collections.Generic;
using Newtonsoft.Json;

namespace Backend.Model
{

    public class ScrollPointsRequest
    {
        [JsonProperty("filter")]
        public SearchFilter? Filter { get; set; } 

        [JsonProperty("limit")]
        public int Limit { get; set; } = 50;

        [JsonProperty("with_payload")]
        public bool WithPayload { get; set; } = true;

        [JsonProperty("with_vector")]
        public bool WithVector { get; set; } = false;
    }

    public class SearchFilter
    {
        [JsonProperty("must")]
        public List<Must> Must { get; set; } = new List<Must>();
    }

    public class Must
    {
        [JsonProperty("key")]
        public string Key { get; set; } = string.Empty;

        [JsonProperty("match")]
        public Match Match { get; set; } = new Match();
    }

    public class Match
    {
        [JsonProperty("value")]
        public string Value { get; set; } = string.Empty;
    }
}
