using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ForgetMeNot.Api.Models;
using ForgetMeNot.Common.Database.Models;
using ForgetMeNot.Common.Transport;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ForgetMeNot.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("quotes")]
    public class QuotesController : ControllerBase
    {
        private readonly IRequestClient<ListQuotesRequest> _listQuotesClient;
        private readonly IRequestClient<QueryUsersRequest> _queryUsersClient;

        public QuotesController(IRequestClient<ListQuotesRequest> listQuotesClient, IRequestClient<QueryUsersRequest> queryUsersClient)
        {
            _listQuotesClient = listQuotesClient;
            _queryUsersClient = queryUsersClient;
        }

        [HttpGet("{guildId}")]
        public async Task<ActionResult<IEnumerable<QuoteResponse>>> ListQuotes(string guildId, [FromQuery] ListQuotesFilter filter)
        {
            var response = await _listQuotesClient.GetResponse<ListResponse<Quote>>(new ListQuotesRequest
            {
                ChannelId = filter.ChannelId == null ? null : ulong.Parse(filter.ChannelId),
                GuildId = ulong.Parse(guildId),
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
            });

            var quotes = response.Message.List.ToArray();
            var userIds = quotes
                .Select(x => x.AuthorId)
                .Distinct();
            var usersResponse = await _queryUsersClient.GetResponse<ListResponse<DiscordUser>>(new QueryUsersRequest
            {
                UserIds = userIds,
            });
            var users = usersResponse.Message.List.Where(x => x != null).ToDictionary(x => x.Id, x => x);

            return Ok(quotes.Select(x => new QuoteResponse(x, users.ContainsKey(x.AuthorId.ToString()) ? users[x.AuthorId.ToString()] : null)));
        }
    }
}
