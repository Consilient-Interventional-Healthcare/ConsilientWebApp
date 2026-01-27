using Consilient.Infrastructure.ExcelImporter.Contracts;
using Consilient.ProviderAssignments.Contracts.Import;
using Consilient.ProviderAssignments.Services.Import;
using Consilient.ProviderAssignments.Services.Import.Validation;
using Consilient.ProviderAssignments.Services.Import.Validation.Validators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Consilient.Infrastructure.ExcelImporter.Tests.Helpers;

internal static class ImporterFactoryHelper
{

    public static IExcelImporter<ProcessedProviderAssignment> CreateImporter(ISinkProvider sinkProvider, int facilityId, DateOnly serviceDate)
    {
        // Create a test service provider with all validators registered
        var services = new ServiceCollection();
        services.AddScoped<IExcelRowValidator, NameRequiredValidator>();
        services.AddScoped<IExcelRowValidator, AgeRangeValidator>();
        services.AddScoped<IExcelRowValidator, HospitalNumberValidator>();
        services.AddScoped<IExcelRowValidator, DateFieldsValidator>();
        services.AddScoped<IExcelRowValidator, MrnValidator>();
        var serviceProvider = services.BuildServiceProvider();

        var validatorProvider = new ValidatorProvider(serviceProvider);
        var providerAssignmentsImportOptions = Options.Create(new ProviderAssignmentsImportOptions());

        var importerFactory = new ImporterFactory(NullLoggerFactory.Instance, sinkProvider, validatorProvider, providerAssignmentsImportOptions);
        var importer = importerFactory.Create(facilityId, serviceDate);
        return importer;
    }
}
