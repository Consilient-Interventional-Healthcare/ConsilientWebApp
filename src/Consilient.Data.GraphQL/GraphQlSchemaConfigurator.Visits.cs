using Consilient.Data.Entities;
using Consilient.Data.Entities.Clinical;
using Consilient.Data.GraphQL.Models;
using EntityGraphQL.Schema;
using EntityGraphQL.Schema.FieldExtensions;
using Microsoft.EntityFrameworkCore;

namespace Consilient.Data.GraphQL;

public static partial class GraphQlSchemaConfigurator
{
    private static void AddVisitTypes(SchemaProvider<ConsilientDbContext> schema)
    {
        var visitType = schema.AddType<Visit>(ToGraphQlName(nameof(Visit)), $"{ToGraphQlName(nameof(Visit))} object");
        visitType.AddField(m => m.DateServiced, nameof(Visit.DateServiced));
        visitType.AddField(m => m.Hospitalization, nameof(Visit.Hospitalization));
        visitType.AddField(m => m.Id, nameof(Visit.Id));
        visitType.AddField(m => m.IsScribeServiceOnly, nameof(Visit.IsScribeServiceOnly));
        visitType.AddField(m => m.Room, nameof(Visit.Room));
        visitType.AddField(m => m.Bed, nameof(Visit.Bed));
        visitType.AddField("patient", m => new VisitPatient
        {
            Id = m.Hospitalization.Patient.Id,
            FirstName = m.Hospitalization.Patient.FirstName,
            LastName = m.Hospitalization.Patient.LastName,
            BirthDate = m.Hospitalization.Patient.BirthDate,
            Mrn = m.Hospitalization.Patient.PatientFacilities
                .Where(pf => pf.FacilityId == m.Hospitalization.FacilityId)
                .Select(pf => pf.Mrn)
                .FirstOrDefault() ?? string.Empty
        }, "Patient with MRN at facility");
        visitType.AddField(m => m.VisitAttendants, nameof(Visit.VisitAttendants));

        var visitPatientType = schema.AddType<VisitPatient>("visitPatient", "Patient object with facility-specific MRN");
        visitPatientType.AddField(m => m.Id, nameof(VisitPatient.Id));
        visitPatientType.AddField(m => m.FirstName, nameof(VisitPatient.FirstName));
        visitPatientType.AddField(m => m.LastName, nameof(VisitPatient.LastName));
        visitPatientType.AddField(m => m.BirthDate, nameof(VisitPatient.BirthDate));
        visitPatientType.AddField(m => m.Mrn, nameof(VisitPatient.Mrn));

        var visitAttendantType = schema.AddType<VisitAttendant>(ToGraphQlName(nameof(VisitAttendant)), $"{ToGraphQlName(nameof(VisitAttendant))} object");
        visitAttendantType.AddField(m => m.Id, nameof(VisitAttendant.Id));
        visitAttendantType.AddField(m => m.VisitId, nameof(VisitAttendant.VisitId));
        visitAttendantType.AddField(m => m.ProviderId, nameof(VisitAttendant.ProviderId));
        visitAttendantType.AddField(m => m.Provider, nameof(VisitAttendant.Provider));
    }

    private static void AddVisitQuery(SchemaType<ConsilientDbContext> query)
    {
        query
            .AddField(
                "visits",
                new
                {
                    dateServiced = ArgumentHelper.Required<string>(),
                    facilityId = ArgumentHelper.Required<int>()
                },
                (ctx, args) => ctx.Visits
                    .AsNoTracking()
                    .Where(v => v.DateServiced == DateOnly.ParseExact(args.dateServiced, "yyyy-MM-dd") && v.Hospitalization.FacilityId == args.facilityId)
                    .OrderBy(p => p.Id),
                "List of visits for a specific date and facility")
            .UseSort();
    }
}
