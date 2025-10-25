using Consilient.Employees.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.Employees.Services
{
    public static class EmployeeRegistrationExtension
    {
        public static void RegisterEmployeeServices(this IServiceCollection services)
        {
            services.AddScoped<IEmployeeService, EmployeeService>();
        }
    }
}
