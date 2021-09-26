using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ForgetMeNot.Common.Extentions;
using ForgetMeNot.Common.Transport;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ForgetMeNot.Api.Services
{
    public class JwtService : ISingletonDiService
    {
        public const string DiscordIdField = "Discord-ID";

        private static readonly TimeSpan AccessTokenValidFor = TimeSpan.FromHours(6);

        private readonly JwtSecurityTokenHandler _tokenHandler;
        private readonly byte[] _key;
        private readonly string _audience;
        private readonly string _issuer;

        public JwtService(IConfiguration config)
        {
            _key = Encoding.UTF8.GetBytes(config["Jwt:Secret"]);
            _issuer = config["Jwt:Issuer"];
            _audience = config["Jwt:Audience"];
            _tokenHandler = new JwtSecurityTokenHandler();
        }

        public string CreateAccessTokenFor(DiscordUser user)
        {
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] 
                {
                    new Claim(DiscordIdField, user.Id.ToString()), 
                }),
                Issuer = _issuer,
                Audience = _audience,
                Expires = DateTime.UtcNow.Add(AccessTokenValidFor),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = _tokenHandler.CreateToken(tokenDescriptor);
            return _tokenHandler.WriteToken(token);
        }
    }
}
