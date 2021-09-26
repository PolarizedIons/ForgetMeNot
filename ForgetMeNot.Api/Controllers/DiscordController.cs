using System.Collections.Generic;
using System.Threading.Tasks;
using ForgetMeNot.Api.Extentions;
using ForgetMeNot.Common.Transport;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ForgetMeNot.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("discord")]
    public class DiscordController : ControllerBase
    {
        private readonly IRequestClient<GetMutualGuildsRequest> _mutualGuildsClient;

        public DiscordController(IRequestClient<GetMutualGuildsRequest> mutualGuildsClient)
        {
            _mutualGuildsClient = mutualGuildsClient;
        }

        [HttpGet("guilds")]
        public async Task<ActionResult<IEnumerable<DiscordGuild>>> GetSharedGuilds()
        {
            var response = await _mutualGuildsClient.GetResponse<ListResponse<DiscordGuild>>(new GetMutualGuildsRequest
            {
                UserId = this.GetUserDiscordId(),
            });
            return Ok(response.Message.List);
        }
    }
}
