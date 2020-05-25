using Authenticator.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Mime;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Authenticator.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    public class AuthController : ControllerBase
    {
        readonly ISpatialOSPlatform spatialOSPlatformService;
        readonly XsollaTokenService xsollaTokenService;
        readonly ILogger<AuthController> logger;

        public AuthController(ISpatialOSPlatform spatialOSPlatformService, XsollaTokenService xsollaTokenService, ILogger<AuthController> logger)
        {
            this.spatialOSPlatformService = spatialOSPlatformService;
            this.xsollaTokenService = xsollaTokenService;
            this.logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SpatialOSTokens>> SpatialOSTokens()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);//weird mapping from firebase token, should be equal to "sub" value aka user id

            if (userId == null)
            {
                logger.LogDebug("User Id is Null");
                return BadRequest();
            }

            var playerIdentityToken = await spatialOSPlatformService.GetPlayerIdentityToken(userId);
            var loginToken = await spatialOSPlatformService.GetLoginToken(playerIdentityToken);

            var tokens = new SpatialOSTokens
            {
                PlayerIdentityToken = playerIdentityToken,
                LoginToken = loginToken
            };

            return Ok(tokens);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TokenResponse>> XsollaToken()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);//weird mapping from firebase token, should be equal to "sub" value aka user id

            if (userId == null)
            {
                logger.LogDebug("User Id is Null");
                return BadRequest();
            }

            var token = await xsollaTokenService.GetXsollaToken(userId);

            return Ok(token);
        }
    }
}
