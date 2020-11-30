using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HMACAuthentication.Tests.Integration
{
    internal static class HttpExtensions
    {
        internal static Task<HttpResponseMessage> PostAsJsonAsync<T>(this HttpClient httpClient, string url, T data, Tuple<string, string> header = null)
        {
            return httpClient.PostAsync(url, ContentAsJson(data, header));
        }

        internal static StringContent ContentAsJson<T>(T data, Tuple<string, string> header)
        {
            var dataAsString = JsonConvert.SerializeObject(data);
            var content = new StringContent(dataAsString);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            
            if (header != null)
            {
                content.Headers.Add(header.Item1, header.Item2);
            }
            
            return content;
        }
    }
}
