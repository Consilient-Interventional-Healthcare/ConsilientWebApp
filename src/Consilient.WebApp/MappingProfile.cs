using AutoMapper;
using Consilient.WebApp.Models;
using Consilient.WebApp.ViewModels;

namespace Consilient.WebApp
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Contract, ContractViewModel>();
            CreateMap<ContractViewModel, Contract>()
                .ForMember(dest => dest.Employee, opt => opt.Ignore())
                .ForMember(dest => dest.Facility, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceType, opt => opt.Ignore())
                .ForMember(dest => dest.ProviderContracts, opt => opt.Ignore());

            CreateMap<Employee, EmployeeViewModel>();
            CreateMap<EmployeeViewModel, Employee>()
                .ForMember(dest => dest.Contracts, opt => opt.Ignore())
                .ForMember(dest => dest.PatientVisitCosigningPhysicianEmployees, opt => opt.Ignore())
                .ForMember(dest => dest.PatientVisitNursePractitionerEmployees, opt => opt.Ignore())
                .ForMember(dest => dest.PatientVisitPhysicianEmployees, opt => opt.Ignore())
                .ForMember(dest => dest.PatientVisitScribeEmployees, opt => opt.Ignore())
                .ForMember(dest => dest.PatientVisitsStagingCosigningPhysicianEmployees, opt => opt.Ignore())
                .ForMember(dest => dest.PatientVisitsStagingNursePractitionerEmployees, opt => opt.Ignore())
                .ForMember(dest => dest.PatientVisitsStagingPhysicianEmployees, opt => opt.Ignore())
                .ForMember(dest => dest.PatientVisitsStagingScribeEmployees, opt => opt.Ignore())
                .ForMember(dest => dest.ProviderContracts, opt => opt.Ignore())
                .ForMember(dest => dest.ProviderPays, opt => opt.Ignore());

            CreateMap<Facility, FacilityViewModel>();
            CreateMap<FacilityViewModel, Facility>()
                .ForMember(dest => dest.PatientVisits, opt => opt.Ignore())
                .ForMember(dest => dest.PatientVisitsStagings, opt => opt.Ignore())
                .ForMember(dest => dest.ProviderPays, opt => opt.Ignore());

            CreateMap<FacilityPay, FacilityPayViewModel>();
            CreateMap<FacilityPayViewModel, FacilityPay>()
                .ForMember(dest => dest.Facility, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceType, opt => opt.Ignore());

            CreateMap<Insurance, InsuranceViewModel>();
            CreateMap<InsuranceViewModel, Insurance>()
                .ForMember(dest => dest.PatientVisits, opt => opt.Ignore())
                .ForMember(dest => dest.PatientVisitsStagings, opt => opt.Ignore());

            CreateMap<Patient, PatientViewModel>();
            CreateMap<PatientViewModel, Patient>()
                .ForMember(dest => dest.PatientVisits, opt => opt.Ignore())
                .ForMember(dest => dest.PatientVisitsStagings, opt => opt.Ignore());

            CreateMap<PatientVisit, PatientVisitViewModel>();
            CreateMap<PatientVisitViewModel, PatientVisit>()
                .ForMember(dest => dest.CosigningPhysicianEmployee, opt => opt.Ignore())
                .ForMember(dest => dest.Facility, opt => opt.Ignore())
                .ForMember(dest => dest.Insurance, opt => opt.Ignore())
                .ForMember(dest => dest.NursePractitionerEmployee, opt => opt.Ignore())
                .ForMember(dest => dest.Patient, opt => opt.Ignore())
                .ForMember(dest => dest.PhysicianEmployee, opt => opt.Ignore())
                .ForMember(dest => dest.ScribeEmployee, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceType, opt => opt.Ignore());

            CreateMap<PatientVisitsStaging, PatientVisitsStagingViewModel>();
            CreateMap<PatientVisitsStagingViewModel, PatientVisitsStaging>()
                .ForMember(dest => dest.CosigningPhysicianEmployee, opt => opt.Ignore())
                .ForMember(dest => dest.Facility, opt => opt.Ignore())
                .ForMember(dest => dest.Insurance, opt => opt.Ignore())
                .ForMember(dest => dest.NursePractitionerEmployee, opt => opt.Ignore())
                .ForMember(dest => dest.Patient, opt => opt.Ignore())
                .ForMember(dest => dest.PhysicianEmployee, opt => opt.Ignore())
                .ForMember(dest => dest.ScribeEmployee, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceType, opt => opt.Ignore());

            CreateMap<PayrollDatum, PayrollDatumViewModel>();
            CreateMap<PayrollDatumViewModel, PayrollDatum>()
                .ForMember(dest => dest.PayrollPeriod, opt => opt.Ignore())
                .ForMember(dest => dest.ProviderPay, opt => opt.Ignore());

            CreateMap<PayrollPeriod, PayrollPeriodViewModel>();
            CreateMap<PayrollPeriodViewModel, PayrollPeriod>()
                .ForMember(dest => dest.PayrollData, opt => opt.Ignore());

            CreateMap<ProviderContract, ProviderContractViewModel>();
            CreateMap<ProviderContractViewModel, ProviderContract>()
                .ForMember(dest => dest.Employee, opt => opt.Ignore())
                .ForMember(dest => dest.Contract, opt => opt.Ignore());

            CreateMap<ProviderPay, ProviderPayViewModel>();
            CreateMap<ProviderPayViewModel, ProviderPay>()
                .ForMember(dest => dest.Employee, opt => opt.Ignore())
                .ForMember(dest => dest.Facility, opt => opt.Ignore())
                .ForMember(dest => dest.PayrollData, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceType, opt => opt.Ignore());

            CreateMap<ServiceType, ServiceTypeViewModel>();
            CreateMap<ServiceTypeViewModel, ServiceType>()
                .ForMember(dest => dest.PatientVisits, opt => opt.Ignore())
                .ForMember(dest => dest.PatientVisitsStagings, opt => opt.Ignore())
                .ForMember(dest => dest.ProviderPays, opt => opt.Ignore());

            CreateMap<PatientVisitsStaging, PatientVisit>();
        }
    }
}
