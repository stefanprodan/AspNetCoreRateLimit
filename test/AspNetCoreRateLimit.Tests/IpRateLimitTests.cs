using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace AspNetCoreRateLimit.Tests
{
    public class IpRateLimitTests: IClassFixture<RateLimitFixture<Demo.Startup>>
    {
        private const string apiValuesPath = "/api/values";
        private const string apiRateLimitPath = "/api/ipratelimit";

        public IpRateLimitTests(RateLimitFixture<Demo.Startup> fixture)
        {
            Client = fixture.Client;
        }

        public HttpClient Client { get; }

        [Theory]
        [InlineData("84.247.85.224")]
        [InlineData("84.247.85.225")]
        [InlineData("84.247.85.226")]
        public async Task SpecificIpRule(string ip)
        {
            // Arrange
            int responseStatusCode = 0;

            // Act    
            for (int i = 0; i < 3; i++)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, apiValuesPath);
                request.Headers.Add("X-Real-IP", ip);

                var response = await Client.SendAsync(request);
                responseStatusCode = (int)response.StatusCode;
            }

            // Assert
            Assert.Equal(429, responseStatusCode);
        }

        [Fact]
        public async Task GlobalIpRule()
        {
            // Arrange
            var ip = "84.247.85.228";

            int responseStatusCode = 0;

            // Act    
            for (int i = 0; i < 4; i++)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, apiValuesPath);
                request.Headers.Add("X-Real-IP", ip);

                var response = await Client.SendAsync(request);
                responseStatusCode = (int)response.StatusCode;
            }

            // Assert
            Assert.Equal(429, responseStatusCode);
        }

        [Fact]
        public async Task WhitelistIp()
        {
            // Arrange
            var ip = "::1";

            int responseStatusCode = 0;

            // Act    
            for (int i = 0; i < 4; i++)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, apiValuesPath);
                request.Headers.Add("X-Real-IP", ip);

                var response = await Client.SendAsync(request);
                responseStatusCode = (int)response.StatusCode;
            }

            // Assert
            Assert.Equal(200, responseStatusCode);
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("POST")]
        [InlineData("PUT")]
        public async Task SpecificPathRule(string verb)
        {
            // Arrange
            var ip = "84.247.85.227";

            int responseStatusCode = 0;

            // Act    
            for (int i = 0; i < 4; i++)
            {
                var request = new HttpRequestMessage(new HttpMethod(verb), apiValuesPath);
                request.Headers.Add("X-Real-IP", ip);

                var response = await Client.SendAsync(request);
                responseStatusCode = (int)response.StatusCode;
            }

            // Assert
            Assert.Equal(429, responseStatusCode);
        }

        [Fact]
        public async Task WhitelistPath()
        {
            // Arrange
            var ip = "84.247.85.229";
            int responseStatusCode = 0;

            // Act    
            for (int i = 0; i < 4; i++)
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, apiValuesPath);
                request.Headers.Add("X-Real-IP", ip);

                var response = await Client.SendAsync(request);
                responseStatusCode = (int)response.StatusCode;
            }

            // Assert
            Assert.NotEqual(429, responseStatusCode);
        }

        [Fact]
        public async Task WhitelistClient()
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

                var response = await Client.SendAsync(request);
                responseStatusCode = (int)response.StatusCode;
            }

            // Assert
            Assert.Equal(200, responseStatusCode);
        }

        [Fact]
        public async Task UpdateOptions()
        {
            // Arrange
            var ip = "::1";
            var keyword = "testupdate";

            // Act
            var updateRequest = new HttpRequestMessage(HttpMethod.Post, apiRateLimitPath);
            updateRequest.Headers.Add("X-Real-IP", ip);

            var updateResponse = await Client.SendAsync(updateRequest);
            Assert.True(updateResponse.IsSuccessStatusCode);

            var request = new HttpRequestMessage(HttpMethod.Get, apiRateLimitPath);
            request.Headers.Add("X-Real-IP", ip);

            var response = await Client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains(keyword, content);
        }
    }
}
