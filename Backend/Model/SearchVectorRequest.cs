using Newtonsoft.Json;

namespace Backend.Model
{
    public class SearchVectorRequest
    {
        [JsonProperty("vector")]
        public List<double> Vector { get; set; } = new List<double>();

        [JsonProperty("limit")]
        public int Limit { get; set; }

        [JsonProperty("filter", NullValueHandling = NullValueHandling.Ignore)]
        public SearchFilter? Filter { get; set; } 

        [JsonProperty("params", NullValueHandling = NullValueHandling.Ignore)]
        public SearchParams? Params { get; set; } = new SearchParams();

        [JsonProperty("with_vectors", NullValueHandling = NullValueHandling.Ignore)]
        public bool? WithVectors { get; set; } = false;

        [JsonProperty("with_payload", NullValueHandling = NullValueHandling.Ignore)]
        public bool? WithPayload { get; set; } = true;
    }


    public class SearchParams
    {
        [JsonProperty("hnsw_ef", NullValueHandling = NullValueHandling.Ignore)]
        public int? HnswEf { get; set; } = 256;

        [JsonProperty("exact", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Exact { get; set; } = false;
    }

}
