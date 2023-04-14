using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Model
{
    public class DeleteModel
    {
        [Required(ErrorMessage = "name is required")]
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }
}

