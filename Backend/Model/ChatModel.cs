namespace Backend.Model
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class AskRequestOverrides
    {
        [JsonPropertyName("semantic_ranker")]
        public bool SemanticRanker { get; set; }

        [JsonPropertyName("semantic_captions")]
        public bool SemanticCaptions { get; set; }

        [JsonPropertyName("exclude_category")]
        public string ExcludeCategory { get; set; } = string.Empty;

        [JsonPropertyName("top")]
        public int Top { get; set; } = 3;

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; } = 0.5;

        [JsonPropertyName("prompt_template")]
        public string PromptTemplate { get; set; } =string.Empty;

        [JsonPropertyName("prompt_template_prefix")]
        public string PromptTemplatePrefix { get; set; } = string.Empty;

        [JsonPropertyName("prompt_template_suffix")]
        public string PromptTemplateSuffix { get; set; } = string.Empty;

        [JsonPropertyName("suggest_followup_questions")]
        public bool SuggestFollowupQuestions { get; set; }
    }

    public class AskRequest
    {
        [JsonPropertyName("question")]
        public string Question { get; set; } = string.Empty;

        [JsonPropertyName("approach")]
        public string Approach { get; set; } = string.Empty;

        [JsonPropertyName("overrides")]
        public AskRequestOverrides Overrides { get; set; } = new AskRequestOverrides();
    }

    public class AskResponse
    {
        [JsonPropertyName("answer")]
        public string Answer { get; set; } = string.Empty;

        [JsonPropertyName("thoughts")]
        public string Thoughts { get; set; } = string.Empty;

        [JsonPropertyName("data_points")]
        public List<string> DataPoints { get; set; } = new List<string>();

        [JsonPropertyName("error")]
        public string Error { get; set; } = string.Empty;
    }

    public class ChatTurnFromClient
    {
        [JsonPropertyName("user")]
        public string User { get; set; } = string.Empty;

        [JsonPropertyName("bot")]
        public string Bot { get; set; } = string.Empty;
    }

    public class ChatRequest
    {
        [JsonPropertyName("history")]
        public List<ChatTurnFromClient> History { get; set; } = new List<ChatTurnFromClient>();

        [JsonPropertyName("approach")]
        public string Approach { get; set; } = string.Empty;

        [JsonPropertyName("overrides")]
        public AskRequestOverrides Overrides { get; set; } = new AskRequestOverrides();

        [JsonPropertyName("myindex_selectedValue")]
        public string MyindexSelectedValue { get; set; } = string.Empty;
    }
}
