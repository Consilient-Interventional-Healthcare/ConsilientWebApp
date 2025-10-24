using Consilient.Employees.Contracts;
using Consilient.Employees.Contracts.Dtos;

namespace Consilient.Employees.Services.Contracts
{
    /// <summary>
    /// Service contract for reading and managing employee records.
    /// </summary>
    public interface IEmployeeService
    {
        /// <summary>
        /// Gets all employees.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that resolves to an <see cref="IEnumerable{EmployeeDto}"/> containing all employees.
        /// </returns>
        Task<IEnumerable<EmployeeDto>> GetAllAsync();

        /// <summary>
        /// Finds an employee by email address.
        /// </summary>
        /// <param name="email">The email address to search for. Must not be <see langword="null"/> or empty.</param>
        /// <returns>
        /// A <see cref="Task"/> that resolves to the matching <see cref="EmployeeDto"/>, or <see langword="null"/> if no employee was found.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="email"/> is <see langword="null"/>.</exception>
        Task<EmployeeDto?> GetByEmail(string email);

        /// <summary>
        /// Gets an employee by identifier.
        /// </summary>
        /// <param name="id">The employee identifier.</param>
        /// <returns>
        /// A <see cref="Task"/> that resolves to the <see cref="EmployeeDto"/> if found, otherwise <see langword="null"/>.
        /// </returns>
        Task<EmployeeDto?> GetById(int id);

        /// <summary>
        /// Creates a new employee from the given request.
        /// </summary>
        /// <param name="request">The creation request. Must not be <see langword="null"/>.</param>
        /// <returns>
        /// A <see cref="Task"/> that resolves to the created <see cref="EmployeeDto"/> (including assigned id).
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">An employee with the specified email already exists.</exception>
        Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request);

        /// <summary>
        /// Updates an existing employee.
        /// </summary>
        /// <param name="id">The id of the employee to update.</param>
        /// <param name="employee">The updated employee values. Must not be <see langword="null"/>.</param>
        /// <returns>
        /// A <see cref="Task"/> that resolves to the updated <see cref="EmployeeDto"/> if the employee was found and updated, otherwise <see langword="null"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
        Task<EmployeeDto?> UpdateAsync(int id, UpdateEmployeeRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
