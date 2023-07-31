using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace AspNetCoreRateLimit.Redis
{
    /// <summary>
    /// Configuration options for <see cref="RedisProcessingStrategy"/>.
    /// </summary>
    public class RedisRateLimitConfiguration : IOptions<RedisRateLimitConfiguration>
    {
        /// <summary>
        /// Gets or sets a delegate to create the ConnectionMultiplexer instance.
        /// </summary>
        public Func<Task<IConnectionMultiplexer>> ConnectionMultiplexerFactory { get; set; }

        public RedisRateLimitConfiguration Value => this;
    }
}
