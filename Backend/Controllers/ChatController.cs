using Backend.Model;
using Backend.Service;
using Microsoft.AspNetCore.Mvc;
using WebApi.Controllers;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {

        private readonly ILogger<FilesController> _logger;
        private readonly AIService _aiService;


        public ChatController(
            AIService aiService,
            ILogger<FilesController> logger,
            IConfiguration configuration)
        {
            _aiService = aiService;
            _logger = logger;
        }


        [HttpPost]
        public async Task<ActionResult<AskResponse>> PostAync([FromBody] ChatRequest chatRequest)
        {
            var history = chatRequest.History.Select(x => new ChatTurn { User=x.User, Assistant=x.Bot }).ToList(); ;
            var question = history.Last().User;
            history.RemoveAt(history.Count - 1);
            (var internalEnglishQuestion, var internalChineseQuestion) = await _aiService.GetFullContextQuestionAsync(question, history);
            var results =_aiService.SearchVectorAsync(internalEnglishQuestion);
            var refContent = string.Join("\n", results.Result.Where(x => x.Score > 0.75).Select(x => x.Payload?["SourcePage"]+":"+ x.Payload?["Content"].Replace("\n", "").Replace("\r", "")).ToList());
            var answer=await _aiService.GetChatGPTAnswerAsync(question, history, refContent);

            AskResponse response = new AskResponse
            {
                Answer = answer,
                Thoughts = "",
                DataPoints = new List<string> (),
                Error = string.Empty
            };

            return Ok(response);
        }
    }
}
