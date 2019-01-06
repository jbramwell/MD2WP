using MoonspaceLabs.Shared.BaseClasses;
using MoonspaceLabs.Shared.Interfaces;
using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MoonspaceLabs.Shared.Helpers
{
    public class RestHttpClient : IRestHttpClient
    {
        public async Task<T> GetAsync<T>(AuthenticationBase authentication, string address)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Authorization = GetAuthenticationHeaderValue(authentication);

                using (HttpResponseMessage response = client.GetAsync(address).Result)
                {
                    // will throw an exception if not successful
                    response.EnsureSuccessStatusCode();

                    var responseBody = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                        throw new Exception(responseBody);

                    try
                    {
                        return JsonConvert.DeserializeObject<T>(responseBody);
                    }
                    catch (Exception)
                    {
                        throw new Exception("Unable to convert results: " + responseBody);
                    }
                }
            }
        }

        public async Task PutAsync(AuthenticationBase authentication, string address, StringContent content)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Authorization = GetAuthenticationHeaderValue(authentication);

                using (HttpResponseMessage response = await client.PutAsync(address, content))
                {
                    // will throw an exception if not successful
                    response.EnsureSuccessStatusCode();

                    return;
                }
            }
        }

        public async Task<T> PutAsync<T>(AuthenticationBase authentication, string address, StringContent content)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Authorization = GetAuthenticationHeaderValue(authentication);

                using (HttpResponseMessage response = await client.PutAsync(address, content))
                {
                    // will throw an exception if not successful
                    response.EnsureSuccessStatusCode();

                    var responseBody = await response.Content.ReadAsStringAsync();
                    var resultObject = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(responseBody);

                    return resultObject;
                }
            }
        }

        private static string GetResponseText(HttpWebResponse response)
        {
            var responseText = string.Empty;

            using (var responseStream = response.GetResponseStream())
            {
                var streamToRead = responseStream;

                if (streamToRead != null)
                {
                    if (response.ContentEncoding.ToLower().Contains("gzip"))
                    {
                        streamToRead = new GZipStream(streamToRead, CompressionMode.Decompress);
                    }
                    else if (response.ContentEncoding.ToLower().Contains("deflate"))
                    {
                        streamToRead = new DeflateStream(streamToRead, CompressionMode.Decompress);
                    }

                    using (var streamReader = new StreamReader(streamToRead, Encoding.UTF8))
                    {
                        responseText = streamReader.ReadToEnd();

                        streamToRead.Close();
                    }
                }
            }

            return responseText;
        }

        public async Task<T> PostAsync<T>(AuthenticationBase authentication, string address, T model)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(address);

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = GetAuthenticationHeaderValue(authentication);

                using (var response = client.PostAsJsonAsync<T>(client.BaseAddress, model))
                {
                    var responseBody = await response.Result.Content.ReadAsStringAsync();

                    return JsonConvert.DeserializeObject<T>(responseBody);
                }
            }
        }

        public async Task<T> PatchAsync<T, M>(AuthenticationBase authentication, string address, M model)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = GetAuthenticationHeaderValue(authentication);

                var request = new HttpRequestMessage(new HttpMethod("PATCH"), address);
                var jsonModel = JsonConvert.SerializeObject(model);

                request.Content = new StringContent(jsonModel, Encoding.UTF8, "application/json");

                using (HttpResponseMessage response = await client.SendAsync(request))
                {
                    // will throw an exception if not successful
                    response.EnsureSuccessStatusCode();

                    var responseBody = await response.Content.ReadAsStringAsync();
                    var resultObject = JsonConvert.DeserializeObject<T>(responseBody);

                    return resultObject;
                }
            }
        }

        public async Task DeleteAsync(AuthenticationBase authentication, string address)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = GetAuthenticationHeaderValue(authentication);

                using (HttpResponseMessage response = await client.DeleteAsync(address))
                {
                    // will throw an exception if not successful
                    response.EnsureSuccessStatusCode();

                    return;
                }
            }
        }

        public HttpWebResponse RequestFile(AuthenticationBase authentication, string address, string httpMethod = "GET")
        {
            HttpWebResponse response = null;

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(address);

                request.UserAgent = "Markdown-to-WordPress";
                request.Headers.Set(HttpRequestHeader.Authorization, GetAuthenticationHeaderValue(authentication).ToString());
                request.ContentType = "application/json";
                request.Method = httpMethod;

                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    response = (HttpWebResponse)e.Response;
                }
            }
            catch (Exception)
            {
                response?.Close();
            }

            return response;
        }

        private string GetBasicAuthenticationHeaderValue(AuthenticationBase authentication)
        {
            var headerValue = string.Empty;

            if (authentication is BasicAuthentication basicAuthentication)
            {
                headerValue =
                    $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{basicAuthentication.UserName}:{basicAuthentication.Password}"))}";
            }

            return headerValue;
        }

        private AuthenticationHeaderValue GetAuthenticationHeaderValue(AuthenticationBase authentication)
        {
            if (authentication is BasicAuthentication basicAuthentication)
            {
                return new AuthenticationHeaderValue("Basic",
                            Convert.ToBase64String(
                                UTF8Encoding.UTF8.GetBytes(
                                    string.Format("{0}:{1}", basicAuthentication.UserName, basicAuthentication.Password))));

            }

            if (authentication is OAuthAuthorization oauthAuthorization)
            {
                return new AuthenticationHeaderValue("Bearer", oauthAuthorization.accessToken);
            }

            throw new InvalidCastException("Authentication type is unknown.");
        }
    }
}