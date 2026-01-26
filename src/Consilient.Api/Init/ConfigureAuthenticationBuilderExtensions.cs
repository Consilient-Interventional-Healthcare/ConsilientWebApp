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

        public static void ConfigureAuthentication(this WebApplicationBuilder builder, TokenGeneratorOptions tokenGeneratorOptions)
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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenGeneratorOptions.Secret)),
                    ValidateIssuer = !string.IsNullOrEmpty(tokenGeneratorOptions.Issuer),
                    ValidIssuer = tokenGeneratorOptions.Issuer,
                    ValidateAudience = !string.IsNullOrEmpty(tokenGeneratorOptions.Audience),
                    ValidAudience = tokenGeneratorOptions.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5)
                };
            });
        }
    }


}