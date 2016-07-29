using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace AspNetCoreRateLimit.Tests
{
    public class ClientRateLimitTests : IClassFixture<RateLimitFixture<Demo.Startup>>
    {
        private const string apiPath = "/api/clients";
        private const string apiRateLimitPath = "/api/ClientRateLimit";
        private const string ip = "::1";

        public ClientRateLimitTests(RateLimitFixture<Demo.Startup> fixture)
        {
            Client = fixture.Client;
        }

        public HttpClient Client { get; }

        [Theory]
        [InlineData("GET")]
        [InlineData("PUT")]
        public async Task SpecificRule(string verb)
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

                var response = await Client.SendAsync(request);
                responseStatusCode = (int)response.StatusCode;
            }

            // Assert
            Assert.Equal(429, responseStatusCode);
        }

        [Fact]
        public async Task GeneralRule()
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

                var response = await Client.SendAsync(request);
                responseStatusCode = (int)response.StatusCode;
                content = await response.Content.ReadAsStringAsync();
            }

            // Assert
            Assert.Equal(429, responseStatusCode);
            Assert.Contains(keyword, content);
        }

        [Fact]
        public async Task OverrideGeneralRule()
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

                var response = await Client.SendAsync(request);
                responseStatusCode = (int)response.StatusCode;
            }

            // Assert
            Assert.NotEqual(429, responseStatusCode);
            
        }

        [Fact]
        public async Task WhitelistPath()
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
            var clientId = "cl-key-b";
            int responseStatusCode = 0;

            // Act    
            for (int i = 0; i < 4; i++)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, apiPath);
                request.Headers.Add("X-ClientId", clientId);
                request.Headers.Add("X-Real-IP", ip);

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
            var clientId = "cl-key-a";
            var keyword = "testpolicyupdate";

            // Act
            var updateRequest = new HttpRequestMessage(HttpMethod.Post, apiRateLimitPath);
            updateRequest.Headers.Add("X-ClientId", clientId);
            updateRequest.Headers.Add("X-Real-IP", ip);

            var updateResponse = await Client.SendAsync(updateRequest);
            Assert.True(updateResponse.IsSuccessStatusCode);

            var request = new HttpRequestMessage(HttpMethod.Get, apiRateLimitPath);
            request.Headers.Add("X-ClientId", clientId);
            request.Headers.Add("X-Real-IP", ip);

            var response = await Client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains(keyword, content);
        }
    }
}
