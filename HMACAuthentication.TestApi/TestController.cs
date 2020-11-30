using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMACAuthentication.TestApi
{
    [ApiController]
    public class TestController : ControllerBase
    {
        [Authorize(AuthenticationSchemes = "Scheme1")]
        [HttpPost("api/scheme1")]
        public IActionResult Scheme1Endpoint()
        {
            return Ok();
        }

        [Authorize(AuthenticationSchemes = "Scheme2")]
        [HttpPost("api/scheme2")]
        public IActionResult Scheme2Endpoint()
        {
            return Ok();
        }
    }
}
