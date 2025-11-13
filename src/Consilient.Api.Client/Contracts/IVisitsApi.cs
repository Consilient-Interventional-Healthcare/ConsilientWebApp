using Consilient.Api.Client.Models;
using Consilient.Visits.Contracts.Dtos;
using Consilient.Visits.Contracts.Requests;

namespace Consilient.Api.Client.Contracts
{
    public interface IVisitsApi<TDto, in TCreate, in TUpdate> : IApi
        where TDto : class
        where TCreate : class
        where TUpdate : class
    {
        Task<ApiResponse<TDto?>> CreateAsync(TCreate request);

        Task<ApiResponse<bool>> DeleteAsync(int id);

        Task<ApiResponse<IEnumerable<TDto>>> GetByDateAsync(DateOnly date);
        Task<ApiResponse<IEnumerable<TDto>>> GetByEmployeeAsync(int employeeId);
        Task<ApiResponse<TDto?>> GetByIdAsync(int id);
        Task<ApiResponse<TDto?>> UpdateAsync(int id, TUpdate request);
    }

    public interface IVisitsApi : IVisitsApi<VisitDto, CreateVisitRequest, UpdateVisitRequest>;
}
