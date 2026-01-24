using Consilient.Api.Infra.Authentication;
using Consilient.Api.Infra.Contracts;
using Consilient.Users.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Consilient.Api.Init
{

    public static class ConfigureAuthenticationBuilderExtensions
    {

        public static void ConfigureAuthentication(this WebApplicationBuilder builder, TokenGeneratorConfiguration tokenGeneratorConfiguration)
        {
            builder.Services.AddScoped<ICsrfTokenCookieService, CsrfTokenCookieService>();
            builder.Services.AddScoped<IJwtTokenCookieService, JwtTokenCookieService>();
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.TryGetValue(AuthenticationCookieNames.AuthToken, out var token) && !string.IsNullOrEmpty(token))
                        {
                            context.Token = token;
                        }
                        return Task.CompletedTask;
                    }
                };

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenGeneratorConfiguration.Secret)),
                    ValidateIssuer = !string.IsNullOrEmpty(tokenGeneratorConfiguration.Issuer),
                    ValidIssuer = tokenGeneratorConfiguration.Issuer,
                    ValidateAudience = !string.IsNullOrEmpty(tokenGeneratorConfiguration.Audience),
                    ValidAudience = tokenGeneratorConfiguration.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5)
                };
            });
        }
    }


}