using System.Threading.Tasks;
using ForgetMeNot.Api.Models;
using ForgetMeNot.Api.Services;
using ForgetMeNot.Common.Transport;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ForgetMeNot.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly DiscordOAuthService _oAuthService;
        private readonly IRequestClient<LoginUserRequest> _discordLoginClient;
        private readonly JwtService _jwtService;

        public AuthController(DiscordOAuthService oAuthService ,IRequestClient<LoginUserRequest> discordLoginClient, JwtService jwtService)
        {
            _oAuthService = oAuthService;
            _discordLoginClient = discordLoginClient;
            _jwtService = jwtService;
        }

        [AllowAnonymous]
        [HttpGet("oauth")]
        public ActionResult<string> GetOAuthUrl()
        {
            return Ok(_oAuthService.GetOAuthUrl());
        }

        [AllowAnonymous]
        [HttpGet("callback")]
        public async Task<ActionResult<LoginToken>> OAuthCallback([FromQuery] string code)
        {
            var discordAuthToken = await _oAuthService.ExchangeCodeForAccessToken(code);
            if (discordAuthToken == null)
            {
                return Unauthorized("Unable to get access token!");
            }

            var response = await _discordLoginClient.GetResponse<DiscordUser, NotFoundResponse>(new LoginUserRequest
            {
                AuthToken = discordAuthToken,
            });
            if (response.Is(out Response<NotFoundResponse> _))
            {
                return Unauthorized("Unable to find user!");
            }
            var user = (DiscordUser)response.Message;

            var accessToken = _jwtService.CreateAccessTokenFor(user);

            return Ok(new LoginToken
            {
                AccessToken = accessToken,
                User = user,
            });
        }
    }
}
