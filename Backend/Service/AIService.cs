using AI.Dev.OpenAI.GPT;
using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Backend.Model;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace Backend.Service
{
    public class AIService
    {
        private const string PromptTemplateFile = "Prompt.txt";
        private readonly ILogger<AIService> _logger;
        private readonly OpenAIClient _azureOpenAIClient;
        private readonly OpenAIClient _openAIClient;
        private readonly SearchClient _searchClient;
        private readonly SearchIndexClient _searchIndexClient;
        private readonly string _indexName = "kbindex";
        private readonly HttpClient _qdrantClient;
        private readonly string _collectionName = "kbindex";


        public AIService(
            IOptions<AzureOpenAIClientSettings> aoiOptions,
            IOptions<OpenAIClientSettings> oiOptions,
            SearchClient searchClient,
            SearchIndexClient searchIndexClient,
            System.Net.Http.IHttpClientFactory httpClientFactory,
            ILogger<AIService> logger)
        {
            _logger = logger;
            var oiSettings = oiOptions.Value;
            var aoiSettings = aoiOptions.Value;

            _openAIClient = new OpenAIClient(
                oiSettings.ApiKey,
                new OpenAIClientOptions());

            _azureOpenAIClient = new OpenAIClient(
                new Uri(aoiSettings.Endpoint),
                new Azure.AzureKeyCredential(aoiSettings.ApiKey),
                new OpenAIClientOptions());
            _searchClient=searchClient;
            _searchIndexClient=searchIndexClient;
            _indexName=_searchClient.IndexName;
            _collectionName=_searchClient.IndexName;
            _qdrantClient = httpClientFactory.CreateClient("qdrant");
        }

        public async Task<SearchIndex?> CreateSearchIndexAsync()
        {
            if (!_searchIndexClient.GetIndexNames().Contains(_indexName))
            {
                var index = new SearchIndex(_indexName)
                {
                    Fields =
                    {
                        new SimpleField("id", SearchFieldDataType.String)
                        {
                            IsKey = true
                        },
                        new SearchableField("content_en", false)
                        {
                            AnalyzerName = LexicalAnalyzerName.EnMicrosoft
                        },
                        new SearchableField("content_cn",false)
                        {
                            AnalyzerName =LexicalAnalyzerName.ZhHansMicrosoft
                        },
                        new SimpleField("category", SearchFieldDataType.String)
                        {
                            IsFilterable = true,
                            IsFacetable = true
                        },
                        new SimpleField("sourcepage", SearchFieldDataType.String)
                        {
                            IsFilterable = true,
                            IsFacetable = true
                        },
                        new SimpleField("sourcefile", SearchFieldDataType.String)
                        {
                            IsFilterable = true,
                            IsFacetable = true
                        }
                    },

                    SemanticSettings= new SemanticSettings()
                    {
                        Configurations =
                        {
                            new SemanticConfiguration(
                                name:"default",
                                prioritizedFields: new PrioritizedFields()
                                {
                                    ContentFields={
                                        new SemanticField() { FieldName="content_en" },
                                        new SemanticField() { FieldName="content_cn" }
                                    }
                                }
                            )
                        }
                    }
                };
                var result = await _searchIndexClient.CreateIndexAsync(index);
                return result.Value;
            }
            return null;
        }

        public async Task<Response<IndexDocumentsResult>> IndexDocumentsAsync(IEnumerable<Dictionary<string, string>> sections, IndexDocumentsOptions? options = null)
        {
            var batch = IndexDocumentsBatch.Upload(sections);
            var client = _searchIndexClient.GetSearchClient(_indexName);
            var result = await client.IndexDocumentsAsync(batch, options);
            return result;
        }

        /// <summary>
        /// generate a full context question according to the conversation history.
        /// </summary>
        /// <param name="question"></param>
        /// <param name="history"></param>
        /// <returns></returns>
        public async Task<(string englishQuestion, string chineseQuestion)> GetFullContextQuestionAsync(string question, IList<ChatTurn> history)
        {
            string? internalChineseQuestion = string.Empty;
            string? internalEnglishQuestion = string.Empty;
            if (IsChinese(question))
            {
                internalChineseQuestion=question;
            }
            else
            {
                internalEnglishQuestion=question;
            }

            string chinesePromptTemplate = string.Empty;
            string englishPromptTemplate = string.Empty;

            string chinesePrompt = string.Empty;
            string englishPrompt = string.Empty;
            if (history==null || history.Count==0)
            {
                chinesePromptTemplate = await GetTemplateContent("TranslateToChinesePrompt");
                chinesePrompt= string.Format(chinesePromptTemplate, question);
                englishPromptTemplate = await GetTemplateContent("TranslateToEnglishPrompt");
                englishPrompt= string.Format(englishPromptTemplate, question);
            }
            else
            {
                chinesePromptTemplate = await GetTemplateContent("ChineseSummaryPrompt");
                chinesePrompt=string.Format(chinesePromptTemplate, string.Join("\n", history.Select(x => x.User + "\n" + x.Assistant)), question);
                englishPromptTemplate = await GetTemplateContent("EnglishSummaryPrompt");
                englishPrompt=string.Format(englishPromptTemplate, string.Join("\n", history.Select(x => x.User + "\n" + x.Assistant)), question);
            }

            var count = Math.Max(GetTokenCount(chinesePrompt), GetTokenCount(englishPrompt));
            if (count>3000)
            {
                return (internalEnglishQuestion, internalChineseQuestion);
            }


            CompletionsOptions completionsOptions = new CompletionsOptions()
            {
                Prompts  =
                        {
                            englishPrompt,
                            chinesePrompt
                        },
                Temperature=0.2f,
                MaxTokens=256,
                FrequencyPenalty=0,
                PresencePenalty=0,
            };
            var deploymentOrModelName = "text-davinci-003";
            var result = _azureOpenAIClient.GetCompletions(deploymentOrModelName, completionsOptions);
            internalEnglishQuestion = result?.Value?.Choices?[0]?.Text??internalEnglishQuestion;
            internalChineseQuestion = result?.Value?.Choices?[1]?.Text??internalChineseQuestion;

            _logger.LogInformation($"internalEnglishQuestion:{internalEnglishQuestion},internalChineseQuestion:{internalChineseQuestion}");
            return (internalEnglishQuestion.Trim(), internalChineseQuestion.Trim());
        }

        /// <summary>
        /// search index data to get reference context
        /// </summary>
        /// <param name="question">question</param>
        /// <param name="language">language</param>
        /// <returns></returns>
        public async Task<string> GetReferenceContentAsync(string question, QueryLanguage language)
        {
            var options = new SearchOptions
            {
                IncludeTotalCount = false,
                QueryType=SearchQueryType.Semantic,
                QueryLanguage=language,
                SemanticConfigurationName="default",
                Size=3
            };
            var results = new List<string>();
            if (language==QueryLanguage.ZhCn)
            {
                var searchResponse = await _searchClient.SearchAsync<KbArticleCn>(question, options);
                foreach (var item in searchResponse.Value.GetResults())
                {
                    if (string.IsNullOrEmpty(item.Document?.Content))
                    {
                        break;
                    }
                    if (item.RerankerScore<0.8)
                    {
                        continue;
                    }
                    results.Add(item.Document?.SourcePage+":"+item.Document?.Content?.Replace("\n", "").Replace("\r", ""));
                }
            }
            else
            {
                var searchResponse = await _searchClient.SearchAsync<KbArticle>(question, options);
                foreach (var item in searchResponse.Value.GetResults())
                {
                    if (string.IsNullOrEmpty(item.Document?.Content))
                    {
                        break;
                    }
                    if (item.RerankerScore<0.8)
                    {
                        continue;
                    }
                    results.Add(item.Document?.SourcePage+":"+item.Document?.Content?.Replace("\n", "").Replace("\r", ""));
                }
            }

            if (results.Count>0)
            {
                var content = string.Join("\n", results);
                return content;
            }
            return string.Empty;
        }

        public async Task RemoveFileFromIndex(string fileName)
        {
            var language = QueryLanguage.EnUs;
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            if (fileNameWithoutExt.Substring(fileNameWithoutExt.Length - 3)=="-cn")
            {
                language=QueryLanguage.ZhCn;
            }

            var filter = string.IsNullOrEmpty(fileName) ? null : $"sourcefile eq '{Path.GetFileName(fileName)}'";

            var options = new SearchOptions
            {
                IncludeTotalCount = true,
                QueryLanguage=language,
                Filter=filter
            };
            var client = _searchIndexClient.GetSearchClient(_indexName);
            if (language==QueryLanguage.ZhCn)
            {
                var results = await _searchClient.SearchAsync<KbArticleCn>("*", options);
                var documents = results.Value.GetResultsAsync();
                await foreach (var page in documents.AsPages())
                {
                    var pagedIds = page.Values.Select(x => x.Document.Id).ToList();
                    var result = await client.DeleteDocumentsAsync("id", pagedIds);
                }
            }
            else
            {
                var results = await _searchClient.SearchAsync<KbArticle>("*", options);
                var documents = results.Value.GetResultsAsync();
                await foreach (var page in documents.AsPages())
                {
                    var pagedDocuments = page.Values.Select(x => x.Document).ToList();
                    var result = await client.DeleteDocumentsAsync(pagedDocuments);
                }
            }
        }

        /// <summary>
        /// Get ChatGPT's answer for a question.
        /// </summary>
        /// <param name="question"></param>
        /// <param name="history"></param>
        /// <param name="refContent"></param>
        /// <returns></returns>
        public async Task<string> GetChatGPTAnswerAsync(string question, IList<ChatTurn> history, string refContent, string? style = null)
        {
            var answer = string.Empty;

            var needRefContent = !string.IsNullOrEmpty(refContent);
            var temperature = 0.5f;
            int count = 0;
            var messages = new List<ChatMessage>();
            var prefix = string.Empty;
            if (style==null)
            {
                prefix = needRefContent ? await GetTemplateContent("ChatGPTLocalContent") : await GetTemplateContent("ChatGPTGeneral");

                if (string.IsNullOrEmpty(prefix))
                {
                    return answer;
                }
                prefix=needRefContent ? string.Format(prefix+"\n", refContent) : prefix;
            }
            else
            {
                prefix=await GetTemplateContent(style);
                if (style=="Coder")
                {
                    temperature=0.2f;
                }
                else if (style=="Sales")
                {
                    temperature=1.0f;
                }
                else if (style=="Translator")
                {
                    temperature=0.2f;
                }
            }

            messages.Add(new Azure.AI.OpenAI.ChatMessage(ChatRole.System, prefix));
            var historyString = string.Empty;
            for (var i = history.Count-1; i>=0; i--)
            {
                count = GetTokenCount(prefix+historyString+history[i].Assistant+history[i].User+question);

                if (count>3000)
                {
                    break;
                }
                messages.Insert(1, new Azure.AI.OpenAI.ChatMessage(ChatRole.Assistant, history[i].Assistant));
                messages.Insert(1, new Azure.AI.OpenAI.ChatMessage(ChatRole.User, history[i].User));
                historyString+=history[i].User+history[i].Assistant;
            }
            messages.Add(new ChatMessage(ChatRole.User, question));
            count= GetTokenCount(string.Join("\n", messages.SelectMany(x => new[] { x.Role+x.Content })));
            if (count>4000)
            {
                return "What you said is too long, please take your time.";
            }
            var deploymentOrModelName = "gpt-35-turbo";
            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                Temperature=temperature,
                MaxTokens=4020-count,
                FrequencyPenalty=0,
                PresencePenalty=0
            };
            foreach (var message in messages)
            {
                chatCompletionsOptions.Messages.Add(message);
            }

            var completionResult = await _azureOpenAIClient.GetChatCompletionsAsync(deploymentOrModelName, chatCompletionsOptions);

            answer = completionResult?.Value?.Choices?.FirstOrDefault()?.Message.Content??string.Empty;

            return answer;
        }

        /// <summary>
        /// Get ChatGPT's answer for a question.
        /// </summary>
        /// <param name="question"></param>
        /// <param name="history"></param>
        /// <param name="refContent"></param>
        /// <returns></returns>
        public async Task<IAsyncEnumerable<StreamingChatChoice>?> GetChatGPTStreamAnswerAsync(string question, IList<ChatTurn> history, string refContent)
        {
            var answer = string.Empty;

            var needRefContent = !string.IsNullOrEmpty(refContent);

            var prefix = needRefContent ? await GetTemplateContent("ChatGPTLocalContent") : await GetTemplateContent("ChatGPTGeneral");

            if (string.IsNullOrEmpty(prefix))
            {
                return null;
            }
            int count = 0;
            var messages = new List<ChatMessage>();
            prefix=needRefContent ? string.Format(prefix+"\n", refContent) : prefix;
            messages.Add(new Azure.AI.OpenAI.ChatMessage(ChatRole.System, prefix));
            var historyString = string.Empty;
            for (var i = history.Count-1; i>=0; i--)
            {
                count = GetTokenCount(prefix+historyString+history[i].Assistant+history[i].User+question);

                if (count>3000)
                {
                    break;
                }
                messages.Insert(1, new Azure.AI.OpenAI.ChatMessage(ChatRole.Assistant, history[i].Assistant));
                messages.Insert(1, new Azure.AI.OpenAI.ChatMessage(ChatRole.User, history[i].User));
                historyString+=history[i].User+history[i].Assistant;
            }
            messages.Add(new ChatMessage(ChatRole.User, question));
            count= GetTokenCount(string.Join("\n", messages.SelectMany(x => new[] { x.Role+x.Content })));
            if (count>4000)
            {
                return null;
            }
            var deploymentOrModelName = "gpt-35-turbo";
            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                Temperature=0.5f,
                MaxTokens=4020-count,
                FrequencyPenalty=0,
                PresencePenalty=0
            };
            foreach (var message in messages)
            {
                chatCompletionsOptions.Messages.Add(message);
            }

            var completionResult = _azureOpenAIClient.GetChatCompletionsStreaming(deploymentOrModelName, chatCompletionsOptions);
            return completionResult.Value.GetChoicesStreaming();
        }

        /// <summary>
        /// Get input string's token count.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public int GetTokenCount(string content)
        {
            var count = GPT3Tokenizer.Encode(content).Count;
            return count;
        }

        /// <summary>
        /// Get context embedding
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task<IReadOnlyList<float>> GetEmbeddingAsync(string content)
        {
            var pattern = "[\r\n,.!?，。！？]";
            string replacement = "";
            string newContent = Regex.Replace(content, pattern, replacement);

            var embeddingsRequest = new EmbeddingsOptions(newContent);
            var deploymentOrModelName = "text-embedding-ada-002";

            Response<Embeddings> response = await _azureOpenAIClient.GetEmbeddingsAsync(deploymentOrModelName, embeddingsRequest);
            return response.Value.Data[0].Embedding;
        }

        /// <summary>
        /// Caculate Cosine similarity
        /// </summary>
        /// <param name="vectorA"></param>
        /// <param name="vectorB"></param>
        /// <returns>similarity score</returns>
        /// <exception cref="ArgumentException"></exception>
        public double Calculate(IReadOnlyList<float> vectorA, IReadOnlyList<float> vectorB)
        {
            if (vectorA == null || vectorB == null || vectorA.Count != vectorB.Count)
            {
                throw new ArgumentException("Input vectors must be non-null and have the same length");
            }

            var dotProduct = vectorA.Zip(vectorB, (a, b) => a * b).Sum();
            var magnitudeA = Math.Sqrt(vectorA.Sum(a => a * a));
            var magnitudeB = Math.Sqrt(vectorB.Sum(b => b * b));
            return dotProduct / (magnitudeA * magnitudeB);
        }

        private bool IsChinese(string content)
        {
            //byte[] bytes = System.Text.Encoding.Default.GetBytes(question);

            //if (bytes.Length > question.Length)
            //{
            //    return true;
            //}

            Regex regex = new Regex(@"[\u4E00-\u9FA5]");
            if (regex.IsMatch(content))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// get prompt content from the template.
        /// </summary>
        /// <param name="templateName"></param>
        /// <param name="fileName"></param>
        /// <returns>content string,null if not found</returns>
        private async Task<string> GetTemplateContent(string templateName, string fileName = PromptTemplateFile)
        {
            var templateResourcePath = GetType().Assembly.GetManifestResourceNames().First(name => name.EndsWith(fileName));

            using (var stream = GetType().Assembly.GetManifestResourceStream(templateResourcePath))
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var fileContent = await reader.ReadToEndAsync();

                        // Define the pattern to match the segments
                        string pattern = @"#(\w+)(?:(?:\r?\n)|$)((?:(?!(?:\n)*#\w+(?:\r?\n)|$)(?:.|\n))*)";

                        // Find all matches
                        MatchCollection matches = Regex.Matches(fileContent, pattern);

                        // Loop through the matches and find the segment with the desired name
                        foreach (System.Text.RegularExpressions.Match match in matches)
                        {
                            string name = match.Groups[1].Value.Trim();
                            string content = match.Groups[2].Value.Trim();

                            if (name == templateName)
                            {
                                return content+"\n";
                            }
                        }
                    }
                }
            }
            return string.Empty;
        }

        private (string? englishQuery, string? chineseQuery) ParseQueryContent(string content)
        {
            string? englishQuery = null;
            string? chineseQuery = null;
            // Define the pattern to match the segments
            string pattern = @"\r*\n*^English\:([\S ]+?)\r?\n*Chinese\:([\S ]+?)$";

            // Find all matches
            var matches = Regex.Matches(content, pattern);

            // Loop through the matches and find the segment with the desired name
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                englishQuery = match.Groups[1].Value.Trim();
                chineseQuery = match.Groups[2].Value.Trim();
                break;
            }

            return (englishQuery, chineseQuery);
        }

        public async Task<bool> CreateCollectionAsync()
        {
            return await CreateCollectionAsync(
                _collectionName, 
                new VectorType() { Size=1536, Distance="Cosine" });
            
        }


        private async Task<bool> CreateCollectionAsync(string collectionName, VectorType vector)
        {
            var collectionInfo = await GetCollectionInfoAsync(collectionName);
            if(collectionInfo == null )
            {
                var model= new CollectionCreateModel() 
                {
                    Vectors = vector
                };

                var result = await _qdrantClient.PutAsync($"/collections/{collectionName}",
                        new StringContent(JsonConvert.SerializeObject(model),Encoding.UTF8, "application/json"));
                if (result.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<DbCollectionInfo?> GetCollectionInfoAsync(string collectionName)
        {
            var result = await _qdrantClient.GetAsync($"/collections/{collectionName}");
            if (result.IsSuccessStatusCode)
            {
                var content = await result.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<DbCollectionInfo>(content);
            }
            return null;
        }


        public async Task<bool> CreatePoints(string collectionName, List<DbPoint> points, bool wait = true)
        {
            try
            {
                var model = new DbPointsCreateModel()
                {
                    Points=points
                };
                var result = await _qdrantClient.PutAsync($"collections/{collectionName}/points?wait={wait.ToString().ToLower()}",
                    new StringContent(JsonConvert.SerializeObject(model),Encoding.UTF8, "application/json"));
                if (result.IsSuccessStatusCode)
                {
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<ScrollPointsResponse?> ScrollPointsAsync(ScrollPointsRequest request)
        {
            
            var response = await _qdrantClient.PostAsync($"collections/{_collectionName}/points/scroll",
                new StringContent(JsonConvert.SerializeObject(request),Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ScrollPointsResponse>(responseContent);
            }
            return null;
        }

        public async Task DeletePoints(string fileName)
        {
            var scrollRequest = new ScrollPointsRequest
            {
                Filter = new Model.SearchFilter
                {
                    Must = new List<Must>
                {
                    new Must
                    {
                        Key = "SourceFile",
                        Match = new Model.Match { Value = fileName }
                    }
                }
                },
                Limit = 50,
                WithPayload = true,
                WithVector = false
            };
            int? nextPageOffset = null;

        try
        {
            var ids=new List<string>();
            while (true)
            {
                if (nextPageOffset.HasValue)
                {
                    scrollRequest.Limit = nextPageOffset.Value;
                }

                var scrollResponse = await ScrollPointsAsync(scrollRequest);
                if(scrollResponse?.Result!=null && scrollResponse.Result.Points.Count!=0)
                {
                    ids.AddRange(scrollResponse.Result.Points.Select(x => x.Id));
                }
                if (scrollResponse?.Result?.NextPageOffset == null)
                {
                    break;
                }

                nextPageOffset = scrollResponse.Result.NextPageOffset;
            }
            await DeletePoints(_collectionName, ids);
        }
        catch (Exception)
        {
            
        }
        }
        private async Task<bool> DeletePoints(string collectionName, List<string> ids)
        {
            try
            {
                var model = new DbPointsDeleteModel()
                {
                    Points=ids
                };
                var result = await _qdrantClient.PostAsync($"collections/{collectionName}/points/delete",
                    new StringContent(JsonConvert.SerializeObject(model),Encoding.UTF8, "application/json"));
                if (result.IsSuccessStatusCode)
                {
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> EmbeddingDocumentAsync(IEnumerable<Dictionary<string, string>> sections)
        {
            var points = new List<DbPoint>();
            //get section's content and source file. calulate embedding of content and generate 
            //a vector as point. batch upload the points to qdrant db.
            foreach(var section in sections)
            {
                var content = string.Empty;
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(section["sourcefile"]);
                if (fileNameWithoutExt.EndsWith("-cn"))
                {
                    content=section["content_cn"];
                }
                else
                {
                    content=section["content_en"];
                }
                
                var embedding= await GetEmbeddingAsync(content);
                List<double> doubleList = embedding.Select(f => (double)f).ToList();
                points.Add(new DbPoint()
                {
                    Vector=(List<double>)doubleList,
                    Payload=new Dictionary<string, string>()
                    {
                        { "Id", section["id"] },
                        { "SourceFile",section["sourcefile"] },
                        { "SourcePage",section["sourcepage"] },
                        { "Category", section["category"] },
                        { "Content",content }

                    }
                });
            }
            if(points.Count>0)
            {
                return await CreatePoints(_collectionName, points);
            }
            return false;
        }



        public async Task<List<SearchResult>> SearchVectorAsync( string content)
        {
            var embeddingResult = await GetEmbeddingAsync(content);
            List<double> doubleList = embeddingResult.Select(f => (double)f).ToList();
            var request = new SearchVectorRequest
            {
                Vector = doubleList,
                Limit = 3,
                Params = new SearchParams
                {
                    HnswEf = 256,
                    Exact = false
                },
                WithVectors = false,
                WithPayload = true
            };
            return await SearchVectorAsync(_collectionName, request);
        }

        private async Task<List<SearchResult>> SearchVectorAsync(string collectionName, SearchVectorRequest request)
        {
            var content = new StringContent(JsonConvert.SerializeObject(request), System.Text.Encoding.UTF8, "application/json");
            var response = await _qdrantClient.PostAsync($"collections/{collectionName}/points/search", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result=JsonConvert.DeserializeObject<SearchVectorResponse>(responseContent);
                if(result!=null && result.Result!=null)
                {
                    return result.Result??new List<SearchResult>();

                }
            }
            
            return new List<SearchResult>();
        }

    }
}