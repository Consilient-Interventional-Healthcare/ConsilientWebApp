using Consilient.Api.Init;
using Consilient.Data;
using Consilient.Patients.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;

namespace Consilient.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            const string version = "v1";
            var appId = typeof(Program).Namespace!;

            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddHealthChecks();

            builder.Services.AddDataProtection()
                .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration
                {
                    EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
                    ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
                });
            builder.Services.AddSwaggerGen(appId, version);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new NullReferenceException("connectionString");
            builder.Services.RegisterDataContext(connectionString);
            builder.Services.RegisterPatientServices();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger(appId, version);
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapHealthChecks("/health");
            app.MapControllers();
            app.Run();
        }
    }
}
