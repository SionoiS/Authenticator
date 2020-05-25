using Authenticator.Models;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Authenticator
{
    public class XsollaTokenService
    {
        readonly HttpClient client;
        readonly HttpRequestMessage httpRequestMessage;
        readonly long projectId;

        public XsollaTokenService(HttpClient client)
        {
            projectId = long.Parse(Environment.GetEnvironmentVariable("XSOLLA_PROJECT_ID"));
            var merchantId = Environment.GetEnvironmentVariable("XSOLLA_MERCHANT_ID");
            var apiKey = Environment.GetEnvironmentVariable("XSOLLA_API_KEY");

            var xsollaTokenURI = new Uri("https://api.xsolla.com/merchant/v2/merchants/" + merchantId + "/token");

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, xsollaTokenURI);

            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(merchantId + ":" + apiKey)));

            this.client = client;
            this.httpRequestMessage = httpRequestMessage;
        }

        public async Task<TokenResponse> GetXsollaToken(string userId)
        {
            var request = new TokenRequest
            {
                user = new User { id = new Id { value = userId } },
                settings = new Settings { project_id = projectId },
            };

            httpRequestMessage.Content = new ByteArrayContent(JsonSerializer.SerializeToUtf8Bytes(request));
            httpRequestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Json);

            var response = await client.SendAsync(httpRequestMessage);
            var contentByteArray = await response.Content.ReadAsByteArrayAsync();

            return JsonSerializer.Deserialize<TokenResponse>(contentByteArray);
        }
    }
}
