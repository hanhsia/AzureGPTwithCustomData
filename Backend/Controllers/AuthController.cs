using Backend.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace DemoPro.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _httpClientFactory=httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost]
    [Route("generateDirectlineToken")]
    public async Task<IActionResult> GenerateDirectlineToken()
    {
        _logger.Enter();
        try
        {
            var client = _httpClientFactory.CreateClient("botframework");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _configuration["MicrosoftDirectlineSecret"]);
            var result = await client.PostAsync("/v3/directline/tokens/generate", new StringContent(string.Empty));
            if (result.IsSuccessStatusCode)
            {
                var content = await result.Content.ReadFromJsonAsync<DirectlineToken>();
                if (content != null)
                {
                    return Ok(new
                    {
                        isSuccess = true,
                        Content = new
                        {
                            ConversationId = content.ConversationId,
                            Token = content.Token,
                            ExpireIn = content.ExpiresIn
                        }
                    });
                }
                return Ok(new
                {
                    isSuccess = false,
                    Message = "No Content!"
                });
            }
            return StatusCode(((int)result.StatusCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GenerateDirectlineToken failed");
            return StatusCode(500, "Internal server error");
        }
        finally
        {
            _logger.Exit();
        }
    }
}