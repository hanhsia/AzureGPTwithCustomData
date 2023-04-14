using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Model
{
    public class CompareModel
    {
        [Required(ErrorMessage = "content1 is required")]
        [JsonPropertyName("content1")]
        public string Content1 { get; set; } = string.Empty;

        [Required(ErrorMessage = "content2 is required")]
        [JsonPropertyName("content2")]
        public string Content2 { get; set; } = string.Empty;
    }
}
