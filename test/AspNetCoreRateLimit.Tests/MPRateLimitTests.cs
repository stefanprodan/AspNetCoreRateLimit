using System;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AspNetCoreRateLimit.Tests
{
    public class MPRateLimitTests: IClassFixture<RateLimitWebApplicationFactory>
    {
        private const string apiPath = "/api/mpvalues";
        private const string apiRateLimitPath = "/api/mpratelimit";
        private const string ip = "::1";

        private readonly HttpClient _client;

        public MPRateLimitTests(RateLimitWebApplicationFactory factory)
        {
            _client = factory.CreateClient(options: new WebApplicationFactoryClientOptions
            {
                BaseAddress = new System.Uri("https://localhost:44304")
            });
        }


        [Theory]
        [InlineData("20")]
        public async Task SpecificMPRule(string val)
        {
            // Arrange
            int responseStatusCode = 0;

            // Act    
            for (int i = 0; i < 3; i++)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, apiPath);
                request.Headers.Add("X-Real-IP", ip);
                request.Headers.Add("X-UIPATH-Metadata", val);

                var response = await _client.SendAsync(request);
                responseStatusCode = (int)response.StatusCode;
            }

            // Assert
            Assert.Equal(429, responseStatusCode);
        }

        [Fact]
        public async Task GeneralMPLimitRule()
        {
            // Arrange
            //var ip = "84.247.85.210";
            var clientId = "cl-key-1";
            int responseStatusCode = 0;
            var MPVal = "20";
            // Act    
            for (int i = 0; i < 3; i++)
            {
                var request = new HttpRequestMessage(HttpMethod.Post,apiRateLimitPath);
                request.Headers.Add("X-UIPATH-Metadata", MPVal);
                request.Headers.Add("X-Real-IP", ip);
                request.Headers.Add("X-ClientId", clientId);

                var response = await _client.SendAsync(request);
                responseStatusCode = (int)response.StatusCode;
                
            }

            // Assert
            Assert.Equal(429, responseStatusCode);

        }


    }
}
