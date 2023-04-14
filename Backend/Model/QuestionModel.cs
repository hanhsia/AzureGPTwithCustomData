using System.Text.Json.Serialization;

namespace Backend.Model
{
    public class ChatTurn
    {
        [JsonPropertyName("user")]
        public string User { get; set; } = string.Empty;
        [JsonPropertyName("assistant")]
        public string Assistant { get; set; } = string.Empty;
    }  

    public class QuestionRequestModel
    {
        [JsonPropertyName("style")]
        public string Style { get; set; } = string.Empty;

        [JsonPropertyName("question")]
        public string Question { get; set; } = string.Empty;

        [JsonPropertyName("history")]
        public List<ChatTurn> History { get; set; }=new List<ChatTurn>();
    }
   
}
