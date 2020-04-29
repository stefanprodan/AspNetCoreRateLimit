using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCoreRateLimit.Tests
{
    // https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-2.2
    // TestServer.cs https://github.com/aspnet/AspNetCore/blob/93a24b03bbda1aa0ab9b553a50b70dd36d554934/src/Hosting/TestHost/src/TestServer.cs
    // WebApplicationFactory.cs https://github.com/aspnet/AspNetCore/blob/c565386a3ed135560bc2e9017aa54a950b4e35dd/src/Mvc/Mvc.Testing/src/WebApplicationFactory.cs
    public class RateLimitWebApplicationFactory : WebApplicationFactory<Demo.Startup>
    {
        protected override TestServer CreateServer(IWebHostBuilder builder)
        {
            var server = base.CreateServer(builder);

            using (var scope = server.Host.Services.CreateScope())
            {
                // get the ClientPolicyStore instance
                var clientPolicyStore = scope.ServiceProvider.GetRequiredService<IClientPolicyStore>();

                // seed Client data from appsettings
                clientPolicyStore.SeedAsync().Wait();

                // get the IpPolicyStore instance
                var ipPolicyStore = scope.ServiceProvider.GetRequiredService<IIpPolicyStore>();

                // seed IP data from appsettings
                ipPolicyStore.SeedAsync().Wait();
            }

            return server;
        }
    }
}