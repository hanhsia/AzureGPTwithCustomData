using Azure.AI.OpenAI;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Controllers;
using Backend.Model;
using Backend.Service;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Concurrent;
using System.Text;
using Microsoft.Graph;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Backend.Controllers
{
    [Route("api/openai")]
    [ApiController]
    public class OpenAIController : ControllerBase
    {

        private readonly ILogger<FilesController> _logger;
        private readonly AIService _aiService;
        private readonly HttpClient _qdratntClient;

        public OpenAIController(
            AIService aiService,
            ILogger<FilesController> logger,
            System.Net.Http.IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _aiService = aiService;
            _logger = logger;
            _qdratntClient = httpClientFactory.CreateClient("qdrant");
        }



        [HttpPost]
        [Route("answer")]
        public async Task<IActionResult> GetQuestionAnswer([FromBody] QuestionRequestModel questionRequestModel)
        {
            _logger.Enter();
            try
            {               
                var response = await _aiService.GetChatGPTAnswerAsync(questionRequestModel.Question, questionRequestModel.History, string.Empty, questionRequestModel.Style);

                return Ok(new
                {
                    isSuccess = true,
                    Content = response
                });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get Embedding failed");
                return StatusCode(500, "Internal server error");
            }
            finally
            {
                _logger.Exit();
            }
        }

 

        [HttpPost]
        [Route("streamAnswer")]
        public async Task GetQuestionStreamAnswer([FromBody] QuestionRequestModel questionRequestModel)
        {
            _logger.Enter();
            await Task.CompletedTask;
            try
            {
                
                // Set up the response headers for streaming
                Response.Headers.Add("Content-Type", "text/event-stream");
                Response.Headers.Add("Cache-Control", "no-cache");
                Response.Headers.Add("Connection", "keep-alive");

                var result = await _aiService.GetChatGPTStreamAnswerAsync(questionRequestModel.Question, questionRequestModel.History, string.Empty);
                if (result!=null)
                {
                    await foreach (var choice in result)
                    {
                        await foreach (var message in choice.GetMessageStreaming())
                        {
                            await Response.WriteAsync(message.Content);
                            await Response.Body.FlushAsync();
                        }
                    }
                }
               
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get Embedding failed");

                // Create an HttpResponseMessage with InternalServerError status code and error message
                HttpResponseMessage errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("Internal server error")
                };

                return ;
            }
            finally
            {
                _logger.Exit();
            }
        }


        [HttpPost, DisableRequestSizeLimit]
        [Route("compare")]
        public async Task<IActionResult> Compare([FromBody] CompareModel compareModel)
        {
            _logger.Enter();
            await Task.CompletedTask;
            try
            {
                var content1=compareModel.Content1;
                var response1 = await _aiService.GetEmbeddingAsync(content1);
                var content2 = compareModel.Content2;
                var response2 = await _aiService.GetEmbeddingAsync(content2);
                var result=_aiService.Calculate(response1!, response2!);

                return Ok(new
                {
                    isSuccess = true,
                    Content= result
                });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get Embedding failed");
                return StatusCode(500, "Internal server error");
            }
            finally
            {
                _logger.Exit();
            }
        }


        [HttpPost]
        [Route("search")]
        public async Task<IActionResult> Search([FromBody] string content)
        {
            _logger.Enter();
            await Task.CompletedTask;
            try
            {
                var result=await _aiService.SearchVectorAsync(content);

                return Ok(new
                {
                    isSuccess = true,
                    Content = result[0]?.Payload?["Content"]
                });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get Embedding failed");
                return StatusCode(500, "Internal server error");
            }
            finally
            {
                _logger.Exit();
            }
        }
    }
}
