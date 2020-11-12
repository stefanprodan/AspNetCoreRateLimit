using AspNetCoreRateLimit.Tests.Enums;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace AspNetCoreRateLimit.Tests
{
    public class ClientRateLimitTests : BaseClassFixture
    {
        private const string apiPath = "/api/clients";
        private const string apiRateLimitPath = "/api/clientratelimit";
        private const string ip = "::1";

        public ClientRateLimitTests(RateLimitWebApplicationFactory factory) : base(factory)
        {
        }

        [Theory]
        [InlineData(ClientType.Wildcard, "GET")]
        [InlineData(ClientType.Wildcard, "PUT")]
        [InlineData(ClientType.Regex, "GET")]
        [InlineData(ClientType.Regex, "PUT")]
        public async Task SpecificClientRule(ClientType clientType, string verb)
        {
            // Arrange
            var clientId = "cl-key-1";

            int responseStatusCode = 0;

            // Act    
            for (int i = 0; i < 4; i++)
            {
                var request = new HttpRequestMessage(new HttpMethod(verb), apiPath);
                request.Headers.Add("X-ClientId", clientId);
                request.Headers.Add("X-Real-IP", ip);

                var response = await GetClient(clientType).SendAsync(request);
                responseStatusCode = (int)response.StatusCode;
            }

            // Assert
            Assert.Equal(429, responseStatusCode);
        }

        [Theory]
        [InlineData(ClientType.Wildcard)]
        [InlineData(ClientType.Regex)]
        public async Task SpecificPathRule(ClientType clientType)
        {
            // Arrange
            var clientId = "cl-key-3";
            int responseStatusCode = 0;
            var content = string.Empty;
            var keyword = "1s";

            // Act    
            for (int i = 0; i < 4; i++)
            {
                var request = new HttpRequestMessage(HttpMethod.Post, apiPath);
                request.Headers.Add("X-ClientId", clientId);
                request.Headers.Add("X-Real-IP", ip);

                var response = await GetClient(clientType).SendAsync(request);
                responseStatusCode = (int)response.StatusCode;
                content = await response.Content.ReadAsStringAsync();
            }

            // Assert
            Assert.Equal(429, responseStatusCode);
            Assert.Contains(keyword, content);
        }

        [Theory]
        [InlineData(ClientType.Wildcard)]
        [InlineData(ClientType.Regex)]
        public async Task GeneralRule(ClientType clientType)
        {
            // Arrange
            var clientId = "cl-key-1";
            int responseStatusCode = 0;
            var content = string.Empty;
            var keyword = "5m";

            // Act    
            for (int i = 0; i < 4; i++)
            {
                var request = new HttpRequestMessage(HttpMethod.Post, apiPath);
                request.Headers.Add("X-ClientId", clientId);
                request.Headers.Add("X-Real-IP", ip);

                var response = await GetClient(clientType).SendAsync(request);
                responseStatusCode = (int)response.StatusCode;
                content = await response.Content.ReadAsStringAsync();
            }

            // Assert
            Assert.Equal(429, responseStatusCode);
            Assert.Contains(keyword, content);
        }

        [Theory]
        [InlineData(ClientType.Wildcard)]
        [InlineData(ClientType.Regex)]
        public async Task OverrideGeneralRule(ClientType clientType)
        {
            // Arrange
            var clientId = "cl-key-2";
            int responseStatusCode = 0;
            

            // Act    
            for (int i = 0; i < 4; i++)
            {
                var request = new HttpRequestMessage(HttpMethod.Post, apiPath);
                request.Headers.Add("X-ClientId", clientId);
                request.Headers.Add("X-Real-IP", ip);

                var response = await GetClient(clientType).SendAsync(request);
                responseStatusCode = (int)response.StatusCode;
            }

            // Assert
            Assert.NotEqual(429, responseStatusCode);
            
        }

        [Theory]
        [InlineData(ClientType.Wildcard)]
        [InlineData(ClientType.Regex)]
        public async Task OverrideGeneralRuleAsLimitZero(ClientType clientType)
        {
            // Arrange
            var clientId = "cl-key-2";
            int responseStatusCode = 0;


            // Act    
            for (int i = 0; i < 4; i++)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, apiPath);
                request.Headers.Add("X-ClientId", clientId);
                request.Headers.Add("X-Real-IP", ip);

                var response = await GetClient(clientType).SendAsync(request);
                responseStatusCode = (int)response.StatusCode;
            }

            // Assert
            Assert.Equal(429, responseStatusCode);
        }

        [Theory]
        [InlineData(ClientType.Wildcard)]
        [InlineData(ClientType.Regex)]
        public async Task WhitelistPath(ClientType clientType)
        {
            // Arrange
            var clientId = "cl-key-x";
            int responseStatusCode = 0;

            // Act    
            for (int i = 0; i < 4; i++)
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, apiPath);
                request.Headers.Add("X-ClientId", clientId);
                request.Headers.Add("X-Real-IP", ip);

                var response = await GetClient(clientType).SendAsync(request);
                responseStatusCode = (int)response.StatusCode;
            }

            // Assert
            Assert.NotEqual(429, responseStatusCode);
        }

        [Theory]
        [InlineData(ClientType.Wildcard)]
        [InlineData(ClientType.Regex)]
        public async Task WhitelistClient(ClientType clientType)
        {
            // Arrange
            var clientId = "cl-key-b";
            int responseStatusCode = 0;

            // Act    
            for (int i = 0; i < 4; i++)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, apiPath);
                request.Headers.Add("X-ClientId", clientId);
                request.Headers.Add("X-Real-IP", ip);

                var response = await GetClient(clientType).SendAsync(request);
                responseStatusCode = (int)response.StatusCode;
            }

            // Assert
            Assert.Equal(200, responseStatusCode);
        }

        [Theory]
        [InlineData(ClientType.Wildcard)]
        [InlineData(ClientType.Regex)]
        public async Task UpdateOptions(ClientType clientType)
        {
            // Arrange
            var clientId = "cl-key-a";
            var keyword = "testpolicyupdate";

            // Act
            var updateRequest = new HttpRequestMessage(HttpMethod.Post, apiRateLimitPath);
            updateRequest.Headers.Add("X-ClientId", clientId);
            updateRequest.Headers.Add("X-Real-IP", ip);

            var updateResponse = await GetClient(clientType).SendAsync(updateRequest);
            Assert.True(updateResponse.IsSuccessStatusCode);

            var request = new HttpRequestMessage(HttpMethod.Get, apiRateLimitPath);
            request.Headers.Add("X-ClientId", clientId);
            request.Headers.Add("X-Real-IP", ip);

            var response = await GetClient(clientType).SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains(keyword, content);
        }
    }
}