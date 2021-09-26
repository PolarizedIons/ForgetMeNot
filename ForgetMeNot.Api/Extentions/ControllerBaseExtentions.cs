using System.Linq;
using ForgetMeNot.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ForgetMeNot.Api.Extentions
{
    public static class ControllerBaseExtentions
    {
        public static ulong GetUserDiscordId(this ControllerBase controller)
        {
            return ulong.Parse(controller.User.Claims.First(claim => claim.Type == JwtService.DiscordIdField).Value);
        }
    }
}
