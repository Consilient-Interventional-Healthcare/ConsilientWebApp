using Consilient.Api.Configuration;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Consilient.Api.Init
{
    internal static class ConfigureRateLimitingServiceCollectionExtensions
    {
        public static void ConfigureRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = RateLimitingConstants.GlobalPermitLimit,
                        Window = TimeSpan.FromSeconds(RateLimitingConstants.GlobalWindowSeconds),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
                });

                options.RejectionStatusCode = 429;

                options.AddPolicy(RateLimitingConstants.AuthenticatePolicy, context =>
                {
                    var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    return RateLimitPartition.GetTokenBucketLimiter(ip, _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = RateLimitingConstants.AuthenticateTokenLimit,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(RateLimitingConstants.AuthenticateWindowSeconds),
                        TokensPerPeriod = RateLimitingConstants.AuthenticateTokenLimit,
                        AutoReplenishment = true
                    });
                });

                options.AddPolicy(RateLimitingConstants.LinkExternalPolicy, context =>
                {
                    var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    return RateLimitPartition.GetTokenBucketLimiter(ip, _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = RateLimitingConstants.LinkExternalTokenLimit,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(RateLimitingConstants.LinkExternalWindowSeconds),
                        TokensPerPeriod = RateLimitingConstants.LinkExternalTokenLimit,
                        AutoReplenishment = true
                    });
                });
            });
        }
    }
}
