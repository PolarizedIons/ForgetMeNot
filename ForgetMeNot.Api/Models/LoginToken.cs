using ForgetMeNot.Common.Transport;

namespace ForgetMeNot.Api.Models
{
    public class LoginToken
    {
        public string AccessToken { get; set; }
        public DiscordUser User { get; set; }
    }
}
