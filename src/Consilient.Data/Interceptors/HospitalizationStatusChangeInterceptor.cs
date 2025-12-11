using Consilient.Common.Services;
using Consilient.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Consilient.Data.Interceptors
{
    public class HospitalizationStatusChangeInterceptor(ICurrentUserService currentUserService) : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            TrackStatusChanges(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            TrackStatusChanges(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void TrackStatusChanges(DbContext? context)
        {
            if (context == null)
            {
                return;
            }

            var changedHospitalizations = context.ChangeTracker.Entries<Hospitalization>()
                .Where(e => e.State == EntityState.Modified)
                .ToList();

            var currentUserId = currentUserService.UserId;

            foreach (var entry in changedHospitalizations)
            {
                var statusProperty = entry.Property(h => h.HospitalizationStatusId);

                if (statusProperty.IsModified)
                {
                    var oldStatusId = (int)statusProperty.OriginalValue!;
                    var newStatusId = (int)statusProperty.CurrentValue!;

                    if (oldStatusId != newStatusId)
                    {
                        var history = new HospitalizationStatusHistory
                        {
                            HospitalizationId = entry.Entity.Id,
                            NewStatusId = newStatusId,
                            ChangedAt = DateTime.UtcNow,
                            ChangedByUserId = currentUserId
                        };

                        context.Set<HospitalizationStatusHistory>().Add(history);
                    }
                }
            }
        }
    }
}