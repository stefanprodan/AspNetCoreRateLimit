using AspNetCoreRateLimit.Tests.Enums;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace AspNetCoreRateLimit.Tests
{
    public class IpRateLimitTests : BaseClassFixture
    {
        private const string apiValuesPath = "/api/values/";
        private const string apiRateLimitPath = "/api/ipratelimit";
        
        public IpRateLimitTests(RateLimitWebApplicationFactory factory) : base(factory)
        {
        }

        [Theory]
        [InlineData(ClientType.Wildcard, "84.247.85.224")]
        [InlineData(ClientType.Wildcard, "84.247.85.225")]
        [InlineData(ClientType.Wildcard, "84.247.85.226:6555")]
        [InlineData(ClientType.Wildcard, "205.156.136.211, 192.168.29.47:54610")]
        [InlineData(ClientType.Regex, "84.247.85.224")]
        [InlineData(ClientType.Regex, "84.247.85.225")]
        [InlineData(ClientType.Regex, "84.247.85.226:6555")]
        [InlineData(ClientType.Regex, "205.156.136.211, 192.168.29.47:54610")]
        public async Task SpecificIpRule(ClientType clientType, string ip)
        {
            // Arrange
            int responseStatusCode = 0;

            // Act    
            for (int i = 0; i < 3; i++)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, apiValuesPath);
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
        public async Task GlobalIpRule(ClientType clientType)
        {
            // Arrange
            var ip = "84.247.85.228";

            int responseStatusCode = 0;
            string content = null;

            // Act    
            for (int i = 0; i < 4; i++)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, apiValuesPath);
                request.Headers.Add("X-Real-IP", ip);

                var response = await GetClient(clientType).SendAsync(request);
                responseStatusCode = (int)response.StatusCode;
                content = await response.Content.ReadAsStringAsync();
            }

            // Assert
            Assert.Equal(429, responseStatusCode);
            Assert.Equal(
                "{ \"message\": \"Whoa! Calm down, cowboy!\", \"details\": \"Quota exceeded. Maximum allowed: 2 per 1s. Please try again in 1 second(s).\" }",
                content);
        }

        [Theory]
        [InlineData(ClientType.Wildcard)]
        [InlineData(ClientType.Regex)]
        public async Task GlobalIpLimitZeroRule(ClientType clientType)
        {
            // Arrange
            var ip = "84.247.85.231";

            int responseStatusCode = 0;

            // Act    
            for (int i = 0; i < 4; i++)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, apiValuesPath);
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
        public async Task WhitelistIp(ClientType clientType)
        {
            // Arrange
            var ip = "::1";

            int responseStatusCode = 0;

            // Act    
            for (int i = 0; i < 4; i++)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, apiValuesPath);
                request.Headers.Add("X-Real-IP", ip);

                var response = await GetClient(clientType).SendAsync(request);
                responseStatusCode = (int)response.StatusCode;
            }

            // Assert
            Assert.Equal(200, responseStatusCode);
        }

        [Theory]
        [InlineData(ClientType.Wildcard, "GET")]
        [InlineData(ClientType.Wildcard, "POST")]
        [InlineData(ClientType.Wildcard, "PUT")]
        [InlineData(ClientType.Regex, "GET")]
        [InlineData(ClientType.Regex, "POST")]
        [InlineData(ClientType.Regex, "PUT")]
        public async Task SpecificPathRule(ClientType clientType, string verb)
        {
            // Arrange
            var ip = "84.247.85.227";

            int responseStatusCode = 0;

            // Act    
            for (int i = 0; i < 4; i++)
            {
                var request = new HttpRequestMessage(new HttpMethod(verb), apiValuesPath);
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
            var ip = "84.247.85.229";
            int responseStatusCode = 0;

            // Act    
            for (int i = 0; i < 4; i++)
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, apiValuesPath);
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
        public async Task WhitelistRootPath(ClientType clientType)
        {
            // Arrange
            var ip = "84.247.85.229";
            int responseStatusCode = 0;

            // Act    
            for (int i = 0; i < 4; i++)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "/");
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
            var ip = "84.247.85.230";
            var clientId = "cl-key-1";
            int responseStatusCode = 0;

            // Act    
            for (int i = 0; i < 4; i++)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, apiValuesPath);
                request.Headers.Add("X-Real-IP", ip);
                request.Headers.Add("X-ClientId", clientId);

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
            var ip = "::1";
            var keyword = "testupdate";

            // Act
            var updateRequest = new HttpRequestMessage(HttpMethod.Post, apiRateLimitPath);
            updateRequest.Headers.Add("X-Real-IP", ip);

            var updateResponse = await GetClient(clientType).SendAsync(updateRequest);
            Assert.True(updateResponse.IsSuccessStatusCode);

            var request = new HttpRequestMessage(HttpMethod.Get, apiRateLimitPath);
            request.Headers.Add("X-Real-IP", ip);

            var response = await GetClient(clientType).SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains(keyword, content);
        }

        [Theory]
        [InlineData(ClientType.Wildcard, "84.247.85.232")]
        [InlineData(ClientType.Regex, "84.247.85.232")]
        public async Task SpecificIpRuleMonitorActive(ClientType clientType, string ip)
        {
            // Arrange
            int responseStatusCode = 0;

            // Act    
            for (int i = 0; i < 2; i++)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, apiValuesPath);
                request.Headers.Add("X-Real-IP", ip);

                var response = await GetClient(clientType).SendAsync(request);
                responseStatusCode = (int)response.StatusCode;
            }

            // Assert
            Assert.Equal(200, responseStatusCode);
        }
    }
}