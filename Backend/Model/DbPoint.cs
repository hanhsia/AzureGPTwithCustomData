using Newtonsoft.Json;

namespace Backend.Model
{
    public class DbPointsCreateModel
    {
        [JsonProperty("points")]
        public List<DbPoint> Points { get; set; } = new List<DbPoint>();
    }

    public class DbPointsDeleteModel
    {
        [JsonProperty("points")]
        public List<string> Points { get; set; } = new List<string>();
    }

    public class DbPoint
    {
        [JsonProperty("id")]
        public string Id { get; set; }=Guid.NewGuid().ToString();
        [JsonProperty("vector")]
        public List<double> Vector { get; set; }=new List<double>();

        [JsonProperty("payload")]
        public Dictionary<string,string>? Payload { get; set; } = null;
    }
}
