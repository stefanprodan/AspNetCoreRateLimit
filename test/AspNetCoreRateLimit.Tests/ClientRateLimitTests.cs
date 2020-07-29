using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration.Memory;
using Xunit;

namespace AspNetCoreRateLimit.Tests
{
    public class ClientRateLimitTests : IClassFixture<RateLimitWebApplicationFactory>
    {
        private const string apiPath = "/api/clients";
        private const string apiRateLimitPath = "/api/clientratelimit";
        private const string ip = "::1";

        private readonly HttpClient _wildcardClient;
        private readonly HttpClient _regexClient;

        public ClientRateLimitTests(RateLimitWebApplicationFactory factory)
        {
            _wildcardClient = factory.CreateClient(options: new WebApplicationFactoryClientOptions
            {
                BaseAddress = new System.Uri("https://localhost:44304")
            });


            _regexClient = CreateRegexClient(factory);
        }

        private HttpClient CreateRegexClient(RateLimitWebApplicationFactory factory)
        {
            return factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, conf) =>
                {
                    conf.Add(new MemoryConfigurationSource
                    {
                        InitialData = new List<KeyValuePair<string, string>>()
                        {
                            new KeyValuePair<string, string>("ClientRateLimiting:EnableRegexRuleMatching", "true"),
                            new KeyValuePair<string, string>("ClientRateLimiting:EndpointWhitelist:0", "[a-zA-Z]+:/api/values"),

                            new KeyValuePair<string, string>("ClientRateLimiting:GeneralRules:0:Endpoint", ".+"),
                            new KeyValuePair<string, string>("ClientRateLimiting:GeneralRules:0:Period", "1s"),
                            new KeyValuePair<string, string>("ClientRateLimiting:GeneralRules:0:Limit", "2"),
                            new KeyValuePair<string, string>("ClientRateLimiting:GeneralRules:1:Endpoint", ".+"),
                            new KeyValuePair<string, string>("ClientRateLimiting:GeneralRules:1:Period", "1m"),
                            new KeyValuePair<string, string>("ClientRateLimiting:GeneralRules:1:Limit", "5"),
                            new KeyValuePair<string, string>("ClientRateLimiting:GeneralRules:2:Endpoint", "post:/api/cli[a-z]+"),
                            new KeyValuePair<string, string>("ClientRateLimiting:GeneralRules:2:Period", "5m"),
                            new KeyValuePair<string, string>("ClientRateLimiting:GeneralRules:2:Limit", "3"),

                            new KeyValuePair<string, string>("ClientRateLimitPolicies:ClientRules:0:Rules:0:Endpoint", ".+"),
                            new KeyValuePair<string, string>("ClientRateLimitPolicies:ClientRules:0:Rules:0:Period", "1s"),
                            new KeyValuePair<string, string>("ClientRateLimitPolicies:ClientRules:0:Rules:0:Limit", "10"),

                            new KeyValuePair<string, string>("ClientRateLimitPolicies:ClientRules:1:Rules:0:Endpoint", ".+"),
                            new KeyValuePair<string, string>("ClientRateLimitPolicies:ClientRules:1:Rules:0:Period", "1s"),
                            new KeyValuePair<string, string>("ClientRateLimitPolicies:ClientRules:1:Rules:0:Limit", "10"),
                        }
                    });
                });
            }).CreateClient(options: new WebApplicationFactoryClientOptions
            {
                BaseAddress = new System.Uri("https://localhost:44304")
            });
        }

        private HttpClient GetClient(ClientType clientType)
        {
            switch (clientType)
            {
                case ClientType.Wildcard:
                    return _wildcardClient;
                case ClientType.Regex:
                    return _regexClient;
                default:
                    throw new ArgumentOutOfRangeException(nameof(clientType), clientType, "Unexpected client type.");
            }
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