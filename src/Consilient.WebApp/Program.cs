using Consilient.Api.Client;
using Consilient.Constants;
using Consilient.Data;
using Consilient.Infrastructure.Injection;
using Consilient.Infrastructure.Logging;
using Consilient.Infrastructure.Logging.Configuration;
using Consilient.WebApp.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;

namespace Consilient.WebApp
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
                .AddJsonFile(ApplicationConstants.ConfigurationFiles.AppSettings, optional: true, reloadOnChange: true)
                .AddJsonFile(string.Format(ApplicationConstants.ConfigurationFiles.EnvironmentAppSettings, builder.Environment.EnvironmentName), optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            var defaultConnectionString = builder.Configuration.GetConnectionString(ApplicationConstants.ConnectionStrings.Default) ?? throw new NullReferenceException($"{ApplicationConstants.ConnectionStrings.Default} missing");

            // Add services to the container.
            var applicationSettings = builder.Services.RegisterApplicationSettings<ApplicationSettings>(builder.Configuration);

            builder.Services.RegisterDataContext(defaultConnectionString);
            builder.Services.AddConsilientApiClient(applicationSettings.ApiClient);

            var loggingConfiguration = builder.Configuration.GetSection(ApplicationConstants.ConfigurationSections.Logging).Get<LoggingConfiguration>() ?? throw new NullReferenceException($"{ApplicationConstants.ConfigurationFiles.AppSettings} missing");
            var labels = new Dictionary<string, string>
            {
                { LabelConstants.App, builder.Environment.ApplicationName },
                { LabelConstants.Env, builder.Environment.EnvironmentName.ToLower() }
            };
            builder.Services.RegisterLogging(loggingConfiguration, labels);

            builder.Services.AddDataProtection()
                .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration
                {
                    EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
                    ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
                });
            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            builder.Services.AddSession();

            builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

            //// not needed
            //builder.Services.Configure<OpenIdConnectOptions>(
            //    OpenIdConnectDefaults.AuthenticationScheme, options =>
            //    {
            //        options.Events = new OpenIdConnectEvents
            //        {
            //            OnTokenValidated = ctx =>
            //            {
            //                Console.WriteLine("OIDC Token validated for: " + ctx.Principal.Identity?.Name);
            //                return Task.CompletedTask;
            //            },
            //            OnAuthenticationFailed = ctx =>
            //            {
            //                Console.WriteLine("OIDC Auth failed: " + ctx.Exception.Message);
            //                return Task.CompletedTask;
            //            }
            //        };
            //    });

            builder.Services.AddAuthorization();

            builder.Services.AddScoped<IClaimsTransformation, ClaimsTransformer>();

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
