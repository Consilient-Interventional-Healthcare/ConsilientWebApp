using Consilient.Data.Entities;
using Consilient.Data.GraphQL.Models;
using EntityGraphQL.Schema;
using Microsoft.EntityFrameworkCore;

namespace Consilient.Data.GraphQL;

public static partial class GraphQlSchemaConfigurator
{
    private static void AddDailyLogTypes(SchemaProvider<ConsilientDbContext> schema)
    {
        // DailyLogProvider type
        var dailyLogProviderType = schema.AddType<DailyLogProvider>(
            "dailyLogProvider",
            "Provider information for daily log");
        dailyLogProviderType.AddField(m => m.Id, nameof(DailyLogProvider.Id));
        dailyLogProviderType.AddField(m => m.FirstName, nameof(DailyLogProvider.FirstName));
        dailyLogProviderType.AddField(m => m.LastName, nameof(DailyLogProvider.LastName));
        dailyLogProviderType.AddField(m => m.Type, nameof(DailyLogProvider.Type));
        dailyLogProviderType.AddField(m => m.ProviderType, nameof(DailyLogProvider.ProviderType));

        // DailyLogHospitalization type
        var dailyLogHospitalizationType = schema.AddType<DailyLogHospitalization>(
            "dailyLogHospitalization",
            "Hospitalization information for daily log");
        dailyLogHospitalizationType.AddField(m => m.Id, nameof(DailyLogHospitalization.Id));
        dailyLogHospitalizationType.AddField(m => m.HospitalizationStatusId, nameof(DailyLogHospitalization.HospitalizationStatusId));
        dailyLogHospitalizationType.AddField(m => m.AdmissionDate, nameof(DailyLogHospitalization.AdmissionDate));
        dailyLogHospitalizationType.AddField(m => m.CaseId, nameof(DailyLogHospitalization.CaseId));

        // DailyLogVisit type
        var dailyLogVisitType = schema.AddType<DailyLogVisit>(
            "dailyLogVisit",
            "Visit information for daily log");
        dailyLogVisitType.AddField(m => m.Id, nameof(DailyLogVisit.Id));
        dailyLogVisitType.AddField(m => m.Room, nameof(DailyLogVisit.Room));
        dailyLogVisitType.AddField(m => m.Bed, nameof(DailyLogVisit.Bed));
        dailyLogVisitType.AddField(m => m.Hospitalization, nameof(DailyLogVisit.Hospitalization));
        dailyLogVisitType.AddField(m => m.Patient, nameof(DailyLogVisit.Patient));
        dailyLogVisitType.AddField(m => m.ProviderIds, nameof(DailyLogVisit.ProviderIds));

        // DailyLogVisitsResult type
        var dailyLogVisitsResultType = schema.AddType<DailyLogVisitsResult>(
            "dailyLogVisitsResult",
            "Result containing date, facility, providers and visits");
        dailyLogVisitsResultType.AddField("date", m => m.Date, "Service date");
        dailyLogVisitsResultType.AddField(m => m.FacilityId, nameof(DailyLogVisitsResult.FacilityId));
        dailyLogVisitsResultType.AddField(m => m.Providers, nameof(DailyLogVisitsResult.Providers));
        dailyLogVisitsResultType.AddField(m => m.Visits, nameof(DailyLogVisitsResult.Visits));
    }

    private static void AddDailyLogQuery(SchemaType<ConsilientDbContext> query)
    {
        query.AddField(
            "dailyLogVisits",
            new
            {
                dateServiced = ArgumentHelper.Required<string>(),
                facilityId = ArgumentHelper.Required<int>()
            },
            (ctx, args) => GetDailyLogVisits(ctx, args.dateServiced, args.facilityId),
            "Daily log visits with deduplicated providers for a specific date and facility");
    }

    private static DailyLogVisitsResult GetDailyLogVisits(ConsilientDbContext ctx, string dateServiced, int facilityId)
    {
        var date = DateOnly.ParseExact(dateServiced, "yyyy-MM-dd");

        // Single query with all necessary includes to prevent N+1
        var visits = ctx.Visits
            .AsNoTracking()
            .Include(v => v.Hospitalization)
                .ThenInclude(h => h.Patient)
                    .ThenInclude(p => p.PatientFacilities)
            .Include(v => v.VisitAttendants)
                .ThenInclude(va => va.Provider)
                    .ThenInclude(p => p.ProviderTypeNavigation)
            .Where(v => v.DateServiced == date && v.Hospitalization.FacilityId == facilityId)
            .OrderBy(v => v.Id)
            .ToList();

        // Extract unique providers and sort by LastName, FirstName
        var providers = visits
            .SelectMany(v => v.VisitAttendants)
            .Select(va => va.Provider)
            .DistinctBy(p => p.Id)
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .Select(p => new DailyLogProvider
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                Type = p.Type,
                ProviderType = p.ProviderTypeNavigation
            })
            .ToList();

        // Map visits to DailyLogVisit with reused VisitPatient
        var dailyLogVisits = visits.Select(v => new DailyLogVisit
        {
            Id = v.Id,
            Room = v.Room,
            Bed = v.Bed,
            Hospitalization = new DailyLogHospitalization
            {
                Id = v.Hospitalization.Id,
                HospitalizationStatusId = (int)v.Hospitalization.Status,
                AdmissionDate = v.Hospitalization.AdmissionDate,
                CaseId = v.Hospitalization.CaseId
            },
            Patient = new VisitPatient
            {
                Id = v.Hospitalization.Patient.Id,
                FirstName = v.Hospitalization.Patient.FirstName,
                LastName = v.Hospitalization.Patient.LastName,
                BirthDate = v.Hospitalization.Patient.BirthDate,
                Mrn = v.Hospitalization.Patient.PatientFacilities
                    .Where(pf => pf.FacilityId == facilityId)
                    .Select(pf => pf.Mrn)
                    .FirstOrDefault() ?? string.Empty
            },
            ProviderIds = [.. v.VisitAttendants.Select(va => va.ProviderId)]
        }).ToList();

        return new DailyLogVisitsResult
        {
            Date = date,
            FacilityId = facilityId,
            Providers = providers,
            Visits = dailyLogVisits
        };
    }
}
