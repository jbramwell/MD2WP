using System;
using System.Text;
using Newtonsoft.Json;

namespace MoonspaceLabs.Shared.BaseClasses
{
    public abstract class AuthenticationBase
    {
        public string Account { get; set; }
    }

    public class BasicAuthentication : AuthenticationBase
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public string BasicAuthHeader
        {
            get
            {
                return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{UserName}:{Password}"));
            }
        }

        public string AccountUrl
        {
            get
            {
                if (Account.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
                {
                    return Account.TrimEnd('/');
                }
                else
                {
                    return $"https://{Account}.visualstudio.com";
                }
            }
        }

        public BasicAuthentication(string account, string userName, string password)
        {
            Account = account;
            UserName = userName;
            Password = password;
        }
    }

    public class OAuthAuthorization : AuthenticationBase
    {
        [JsonProperty(PropertyName = "access_token")]
        public String accessToken { get; set; }

        [JsonProperty(PropertyName = "token_type")]
        public String tokenType { get; set; }

        [JsonProperty(PropertyName = "expires_in")]
        public String expiresIn { get; set; }

        [JsonProperty(PropertyName = "refresh_token")]
        public String refreshToken { get; set; }

        public String Error { get; set; }
    }
}