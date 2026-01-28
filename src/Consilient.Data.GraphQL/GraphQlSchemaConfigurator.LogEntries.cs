using Consilient.Data.Entities.Clinical;
using Consilient.Data.GraphQL.Models;
using EntityGraphQL.Schema;
using Microsoft.EntityFrameworkCore;

namespace Consilient.Data.GraphQL;

public static partial class GraphQlSchemaConfigurator
{
    private static void AddLogEntryTypes(SchemaProvider<ConsilientDbContext> schema)
    {
        // DailyLogEvent type
        var dailyLogEventType = schema.AddType<DailyLogEvent>(
            "dailyLogEvent",
            "Event information for log entry");
        dailyLogEventType.AddField(m => m.Id, nameof(DailyLogEvent.Id));
        dailyLogEventType.AddField(m => m.Description, nameof(DailyLogEvent.Description));
        dailyLogEventType.AddField(m => m.EnteredByUserId, nameof(DailyLogEvent.EnteredByUserId));
        dailyLogEventType.AddField(m => m.EventOccurredAt, nameof(DailyLogEvent.EventOccurredAt));
        dailyLogEventType.AddField(m => m.EventTypeId, nameof(DailyLogEvent.EventTypeId));
        dailyLogEventType.AddField(m => m.VisitId, nameof(DailyLogEvent.VisitId));

        // DailyLogUser type
        var dailyLogUserType = schema.AddType<DailyLogUser>(
            "dailyLogUser",
            "User information for log entry");
        dailyLogUserType.AddField(m => m.FirstName, nameof(DailyLogUser.FirstName));
        dailyLogUserType.AddField(m => m.LastName, nameof(DailyLogUser.LastName));
        dailyLogUserType.AddField(m => m.Role, nameof(DailyLogUser.Role));

        // DailyLogEventType type
        var dailyLogEventTypeType = schema.AddType<DailyLogEventType>(
            "dailyLogEventType",
            "Event type information for log entry");
        dailyLogEventTypeType.AddField(m => m.Id, nameof(DailyLogEventType.Id));
        dailyLogEventTypeType.AddField(m => m.Code, nameof(DailyLogEventType.Code));
        dailyLogEventTypeType.AddField(m => m.Name, nameof(DailyLogEventType.Name));

        // DailyLogLogEntryV2 type
        var dailyLogLogEntryV2Type = schema.AddType<DailyLogLogEntryV2>(
            "dailyLogLogEntryV2",
            "Log entry with event, user, and event type information");
        dailyLogLogEntryV2Type.AddField(m => m.Event, nameof(DailyLogLogEntryV2.Event));
        dailyLogLogEntryV2Type.AddField(m => m.User, nameof(DailyLogLogEntryV2.User));
        dailyLogLogEntryV2Type.AddField(m => m.EventType, nameof(DailyLogLogEntryV2.EventType));
    }

    private static void AddLogEntryQuery(SchemaType<ConsilientDbContext> query)
    {
        query.AddField(
            "getLogEntriesByVisitIdV2",
            new { visitId = ArgumentHelper.Required<int>() },
            (ctx, args) => GetLogEntriesByVisitIdV2(ctx, args.visitId),
            "Get log entries for a specific visit with event, user, and event type information");
    }

    private static List<DailyLogLogEntryV2> GetLogEntriesByVisitIdV2(ConsilientDbContext ctx, int visitId)
    {
        var visitEvents = ctx.VisitEvents
            .AsNoTracking()
            .Where(ve => ve.VisitId == visitId)
            .OrderBy(ve => ve.EventOccurredAt)
            .ToList();

        var userIds = visitEvents.Select(ve => ve.EnteredByUserId).Distinct().ToList();
        var eventTypeIds = visitEvents.Select(ve => (int)ve.EventType).Distinct().ToList();

        // Load employees for the users
        var employees = ctx.Employees
            .AsNoTracking()
            .Where(e => userIds.Contains(e.Id))
            .ToDictionary(e => e.Id);

        // Load event types
        var eventTypes = ctx.VisitEventTypes
            .AsNoTracking()
            .Where(et => eventTypeIds.Contains(et.Id))
            .ToDictionary(et => et.Id);

        return [.. visitEvents.Select(ve => new DailyLogLogEntryV2
        {
            Event = new DailyLogEvent
            {
                Id = ve.Id,
                Description = ve.Description,
                EnteredByUserId = ve.EnteredByUserId,
                EventOccurredAt = ve.EventOccurredAt.ToString("O"),
                EventTypeId = (int)ve.EventType,
                VisitId = ve.VisitId
            },
            User = employees.TryGetValue(ve.EnteredByUserId, out var employee)
                ? new DailyLogUser
                {
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Role = employee.Role.ToString()
                }
                : new DailyLogUser(),
            EventType = eventTypes.TryGetValue((int)ve.EventType, out var eventType)
                ? new DailyLogEventType
                {
                    Id = eventType.Id,
                    Code = eventType.Code,
                    Name = eventType.Name
                }
                : null
        })];
    }
}
