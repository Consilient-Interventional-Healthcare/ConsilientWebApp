using Consilient.Api.Configuration;
using System.Threading.RateLimiting;

namespace Consilient.Api.Init;

internal static class ConfigureRateLimitingServiceCollectionExtensions
{
    private const string UnknownIp = "unknown";

    public static void ConfigureRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetPartitionKey(context),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = RateLimitingConstants.GlobalPermitLimit,
                        Window = TimeSpan.FromSeconds(RateLimitingConstants.GlobalWindowSeconds),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            options.RejectionStatusCode = 429;

            options.AddPolicy(RateLimitingConstants.AuthenticatePolicy, context =>
                CreateTokenBucketPartition(
                    context,
                    RateLimitingConstants.AuthenticateTokenLimit,
                    RateLimitingConstants.AuthenticateWindowSeconds));

            options.AddPolicy(RateLimitingConstants.LinkExternalPolicy, context =>
                CreateTokenBucketPartition(
                    context,
                    RateLimitingConstants.LinkExternalTokenLimit,
                    RateLimitingConstants.LinkExternalWindowSeconds));

            options.AddPolicy(RateLimitingConstants.OAuthLoginPolicy, context =>
                CreateFixedWindowPartition(context, permitLimit: RateLimitingConstants.OAuthLoginPermitLimit, windowMinutes: RateLimitingConstants.OAuthLoginWindowMinutes));

            options.AddPolicy(RateLimitingConstants.OAuthCallbackPolicy, context =>
                CreateFixedWindowPartition(context, permitLimit: RateLimitingConstants.OAuthCallbackPermitLimit, windowMinutes: RateLimitingConstants.OAuthCallbackWindowMinutes));
        });
    }

    private static string GetPartitionKey(HttpContext context) =>
        context.Connection.RemoteIpAddress?.ToString() ?? UnknownIp;

    private static RateLimitPartition<string> CreateTokenBucketPartition(
        HttpContext context,
        int tokenLimit,
        int windowSeconds) =>
        RateLimitPartition.GetTokenBucketLimiter(
            GetPartitionKey(context),
            _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = tokenLimit,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
                ReplenishmentPeriod = TimeSpan.FromSeconds(windowSeconds),
                TokensPerPeriod = tokenLimit,
                AutoReplenishment = true
            });

    private static RateLimitPartition<string> CreateFixedWindowPartition(
        HttpContext context,
        int permitLimit,
        int windowMinutes) =>
        RateLimitPartition.GetFixedWindowLimiter(
            GetPartitionKey(context),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromMinutes(windowMinutes),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
}
