using System;
using System.Net.Http;
using AspNetCoreRateLimit.Tests.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace AspNetCoreRateLimit.Tests
{
    public abstract class BaseClassFixture : IClassFixture<RateLimitWebApplicationFactory>
    {
        private readonly HttpClient _wildcardClient;
        private readonly HttpClient _regexClient;

        protected BaseClassFixture(RateLimitWebApplicationFactory factory)
        {
            _wildcardClient = factory.CreateClient(options: new WebApplicationFactoryClientOptions
            {
                BaseAddress = new System.Uri("https://localhost:44304")
            });

            _regexClient = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, conf) =>
                {
                    conf.AddJsonFile("appsettings.Regex.json");
                });
            }).CreateClient(options: new WebApplicationFactoryClientOptions
            {
                BaseAddress = new System.Uri("https://localhost:44304")
            });
        }

        /// <summary>
        /// Gets the <see cref="HttpClient"/> for the given <see cref="ClientType"/>.
        /// </summary>
        /// <param name="clientType">The type of client to return.</param>
        protected HttpClient GetClient(ClientType clientType)
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
    }
}