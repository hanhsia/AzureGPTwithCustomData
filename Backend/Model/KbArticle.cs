// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.18.1

using System.Text.Json.Serialization;

namespace Backend.Model
{
    public  class KbArticle
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        [JsonPropertyName("content_en")]
        public string Content { get; set; }=string.Empty;
        [JsonPropertyName("category")]
        public string  Category { get; set; }=string.Empty;
        [JsonPropertyName("sourcepage")]
        public string SourcePage { get; set; }=string.Empty;
        [JsonPropertyName("sourcefile")]
        public string SourceFile { get; set; }=string.Empty;

    }
    public class KbArticleCn
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        [JsonPropertyName("content_cn")]
        public string Content { get; set; } = string.Empty;
        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;
        [JsonPropertyName("sourcepage")]
        public string SourcePage { get; set; } = string.Empty;
        [JsonPropertyName("sourcefile")]
        public string SourceFile { get; set; } = string.Empty;

    }
}