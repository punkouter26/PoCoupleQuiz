using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

namespace PoCoupleQuiz.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiagnosticsController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public DiagnosticsController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet("internet")]
        public async Task<IActionResult> CheckInternetConnection()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://www.microsoft.com/");
                response.EnsureSuccessStatusCode();
                return Ok(true);
            }
            catch (HttpRequestException)
            {
                return Ok(false);
            }
        }
    }
}
