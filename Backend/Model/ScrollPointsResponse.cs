namespace Backend.Model
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class ScrollPointsResponse
    {
        [JsonProperty("result")]
        public ScrollResult Result { get; set; } = new ScrollResult();

        [JsonProperty("status")]
        public string Status { get; set; } =string.Empty;

        [JsonProperty("time")]
        public double Time { get; set; }
    }

    public class ScrollResult
    {
        [JsonProperty("next_page_offset")]
        public int? NextPageOffset { get; set; }

        [JsonProperty("points")]
        public List<DbPoint> Points { get; set; } = new List<DbPoint>();
    }
}
