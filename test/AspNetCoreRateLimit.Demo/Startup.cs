using AspNetCoreRateLimit.Redis;
using Ben.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;

namespace AspNetCoreRateLimit.Demo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // configure ip rate limiting middleware
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));
            services.Configure<IpRateLimitPolicies>(Configuration.GetSection("IpRateLimitPolicies"));

            // configure client rate limiting middleware
            services.Configure<ClientRateLimitOptions>(Configuration.GetSection("ClientRateLimiting"));
            services.Configure<ClientRateLimitPolicies>(Configuration.GetSection("ClientRateLimitPolicies"));
            
            // configure Redis rate limiting strategy
            services.Configure<RedisRateLimitOptions>(Configuration.GetSection("RedisRateLimitPolicies"));

            // register stores
            services.AddInMemoryRateLimiting();
            // services.AddDistributedRateLimiting<AsyncKeyLockProcessingStrategy>();
            // services.AddDistributedRateLimiting<RedisProcessingStrategy>();
            // services.AddRedisRateLimiting();

            services.AddMvc((options) =>
            {
                options.EnableEndpointRouting = false;

            }).AddNewtonsoftJson();


            // configure the resolvers
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseBlockingDetection();

            app.UseIpRateLimiting();
            app.UseClientRateLimiting();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseDefaultFiles(new DefaultFilesOptions 
            { 
                DefaultFileNames = new List<string> { "index.html" } 
            });
            app.UseStaticFiles();

            app.UseMvc();
        }
    }
}