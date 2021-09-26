using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
using ForgetMeNot.Common.Extentions;
using Microsoft.Extensions.Configuration;

namespace ForgetMeNot.Api.Services
{
    public class DiscordOAuthService : IScopedDiService
    {
        private static readonly string[] RequestScopes = {"identify"};
        private const string OAuthRequestUrl = "https://discord.com/oauth2/authorize?client_id={client_id}&redirect_uri={redirect_url}&response_type=code&scope={scopes}";
        private const string OAuthExchangeUrl = "https://discord.com/api/oauth2/token";
        
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public DiscordOAuthService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public string GetOAuthUrl()
        {
            var clientId = _configuration["Discord:ClientId"];
            var redirectUrl = _configuration["Discord:RedirectUrl"];
            var scopes = string.Join(" ", RequestScopes);

            return OAuthRequestUrl
                .Replace("{client_id}", clientId)
                .Replace("{redirect_url}", HttpUtility.UrlEncode(redirectUrl))
                .Replace("{scopes}", HttpUtility.UrlEncode(scopes))
            ;
        }

        public async Task<string> ExchangeCodeForAccessToken(string code)
        {
            var data = new Dictionary<string, string>
            {
                {"client_id", _configuration["Discord:ClientId"]},
                {"client_secret", _configuration["Discord:ClientSecret"]},
                {"grant_type", "authorization_code"},
                {"code", code},
                {"redirect_uri", _configuration["Discord:RedirectUrl"]},
                {"scope", string.Join(" ", RequestScopes)}
            };

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(OAuthExchangeUrl),
                Headers =
                {
                    {HttpRequestHeader.ContentType.ToString(), "application/x-www-form-urlencoded"}
                },
                Content = new FormUrlEncodedContent(data),
            };

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var responseJson = JsonSerializer.Deserialize<TokenExchangeResponse>(responseString);
            return responseJson.AccessToken;
        }
    }

    internal struct TokenExchangeResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }
        
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
        
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
        
        [JsonPropertyName("scope")]
        public string Scope { get; set; }
    }
}
