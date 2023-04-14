using Newtonsoft.Json;

namespace Backend.Model
{
    public class DbCollection
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("vectors")]
        public VectorType Vectors { get; set; } = new VectorType() { Size=1536, Distance="Cosine" };
    }

    public class VectorType
    {
        [JsonProperty("size")]
        public int Size { get; set; }
        [JsonProperty("distance")]
        public string Distance { get; set; } = string.Empty;
    }

    public class CollectionCreateModel
    {
        [JsonProperty("vectors")]
        public VectorType Vectors { get; set; } = new VectorType() { Size=1536, Distance="Cosine" };
    }
}
