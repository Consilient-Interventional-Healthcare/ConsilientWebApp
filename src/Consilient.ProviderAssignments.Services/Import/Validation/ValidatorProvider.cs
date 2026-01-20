using Microsoft.Extensions.DependencyInjection;

namespace Consilient.ProviderAssignments.Services.Import.Validation
{
    /// <summary>
    /// Provides validator instances from the DI container.
    /// </summary>
    public class ValidatorProvider(IServiceProvider serviceProvider) : IValidatorProvider
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public IEnumerable<IExcelRowValidator> GetValidators() =>
            _serviceProvider.GetServices<IExcelRowValidator>();
    }
}
