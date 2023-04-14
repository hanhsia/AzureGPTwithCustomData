using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Model
{
    public class ListModel
    {
        [JsonPropertyName("prefix")]
        public string Prefix { get; set; } = string.Empty;
    }
}
