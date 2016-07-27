KestrelRateLimit
==============

KestrelRateLimit is an ASP.NET Core rate limiting solution designed to control the rate of requests that clients can make to a Web API or MVC app based on IP address or client ID. The KestrelRateLimit package contains an IpRateLimitMiddleware and a ClientRateLimitMiddleware, with each middleware you can set multiple limits for different scenarios like allowing an IP or Client to make a maximum number of calls in a time interval like per second, 15 minutes, etc. You can define these limits to address all requests made to an API or you can scope the limits to each API URL or HTTP verb and path.

[![Build status](https://ci.appveyor.com/api/projects/status/48uf7t52n67dqd3b?svg=true)](https://ci.appveyor.com/project/stefanprodan/kestrelratelimit)
[![NuGet](https://img.shields.io/nuget/v/KestrelRateLimit.svg?maxAge=2592000)](https://www.nuget.org/packages/KestrelRateLimit/) 

**Documentation**

Rate limiting based on client IP

- [Setup and configuration](https://github.com/stefanprodan/KestrelRateLimit/wiki/IpRateLimitMiddleware#setup)
- [Defining rate limit rules](https://github.com/stefanprodan/KestrelRateLimit/wiki/IpRateLimitMiddleware#defining-rate-limit-rules)
- [Behavior](https://github.com/stefanprodan/KestrelRateLimit/wiki/IpRateLimitMiddleware#behavior)
- [Update rate limits at runtime](https://github.com/stefanprodan/KestrelRateLimit/wiki/IpRateLimitMiddleware#update-rate-limits-at-runtime)

Rate limiting based on client ID

- [Setup and configuration](https://github.com/stefanprodan/KestrelRateLimit/wiki/ClientRateLimitMiddleware#setup)
- [Defining rate limit rules](https://github.com/stefanprodan/KestrelRateLimit/wiki/ClientRateLimitMiddleware#defining-rate-limit-rules)
- [Behavior](https://github.com/stefanprodan/KestrelRateLimit/wiki/ClientRateLimitMiddleware#behavior)
- [Update rate limits at runtime](https://github.com/stefanprodan/KestrelRateLimit/wiki/ClientRateLimitMiddleware#update-rate-limits-at-runtime)
