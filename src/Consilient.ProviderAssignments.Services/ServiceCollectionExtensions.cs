using Consilient.Infrastructure.ExcelImporter.Contracts;
using Consilient.ProviderAssignments.Contracts.Import;
using Consilient.ProviderAssignments.Contracts.Processing;
using Consilient.ProviderAssignments.Contracts.Resolution;
using Consilient.ProviderAssignments.Services.Import;
using Consilient.ProviderAssignments.Services.Import.Sinks;
using Consilient.ProviderAssignments.Services.Import.Validation;
using Consilient.ProviderAssignments.Services.Import.Validation.Validators;
using Consilient.ProviderAssignments.Services.Processing;
using Consilient.ProviderAssignments.Services.Resolution;
using Consilient.ProviderAssignments.Services.Resolution.Resolvers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.ProviderAssignments.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddProviderAssignmentsServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ImportSettings>(configuration.GetSection(ImportSettings.SectionName));
            AddImportServices(services);
            AddResolutionServices(services);
            AddProcessingServices(services);
            return services;
        }

        private static void AddImportServices(IServiceCollection services)
        {
            services.AddScoped<IDataSink, EFCoreStagingProviderAssignmentSink>();
            services.AddScoped<ISinkProvider, TrivialSinkProvider>();

            // Register individual validators (validate raw Excel data)
            services.AddScoped<IExcelRowValidator, NameRequiredValidator>();
            services.AddScoped<IExcelRowValidator, AgeRangeValidator>();
            services.AddScoped<IExcelRowValidator, HospitalNumberValidator>();
            services.AddScoped<IExcelRowValidator, DateFieldsValidator>();
            services.AddScoped<IExcelRowValidator, MrnValidator>();

            // Register validator provider
            services.AddScoped<IValidatorProvider, ValidatorProvider>();

            services.AddScoped<IImporterFactory, ImporterFactory>();
        }

        private static void AddResolutionServices(IServiceCollection services)
        {
            // Register resolution cache
            services.AddScoped<IResolutionCache, ResolutionCache>();

            // Register individual resolvers with their marker interfaces
            services.AddScoped<IPhysicianResolver, PhysicianResolver>();
            services.AddScoped<INursePractitionerResolver, NursePractitionerResolver>();
            services.AddScoped<IPatientResolver, PatientResolver>();
            services.AddScoped<IHospitalizationResolver, HospitalizationResolver>();
            services.AddScoped<IHospitalizationStatusResolver, HospitalizationStatusResolver>();
            services.AddScoped<IVisitResolver, VisitResolver>();

            // Register resolver provider (creates resolvers with explicit cache/dbContext)
            services.AddScoped<IResolverProvider, ResolverProvider>();

            // Register main resolver
            services.AddScoped<IProviderAssignmentsResolver, ProviderAssignmentsResolver>();
        }

        private static void AddProcessingServices(IServiceCollection services)
        {
            services.AddScoped<IProviderAssignmentsProcessor, ProviderAssignmentsProcessor>();
        }
    }
}
