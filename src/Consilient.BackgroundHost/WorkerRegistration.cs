using Consilient.Background.Workers;
using Hangfire;
using Hangfire.Storage;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace Consilient.BackgroundHost
{
    internal class WorkerRegistration(IBackgroundJobClient backgroundJobClient, IRecurringJobManager recurringJobManager, JobStorage jobStorage)
    {
        private readonly IBackgroundJobClient BackgroundJobClient = backgroundJobClient;
        private readonly JobStorage JobStorage = jobStorage;
        private readonly IRecurringJobManager RecurringJobManager = recurringJobManager;

        public void Register()
        {
            RegisterRecurringJobs();
            RegisterJobsThatMustRunInStartup();
        }

        private static void RegisterJobsThatMustRunInStartup()
        {
        }

        private void RegisterRecurringJob<TRecurringJob>(Expression<Action<TRecurringJob>> methodCall, string cronExpression, TimeZoneInfo? timeZoneInfo = null) where TRecurringJob : BaseRecurringWorker
        {
            RecurringJobManager.AddOrUpdate(typeof(TRecurringJob).Name, methodCall, cronExpression, new RecurringJobOptions { TimeZone = timeZoneInfo ?? TimeZoneInfo.Local });
        }

        private void RegisterRecurringJobs()
        {
            using (var connection = JobStorage.GetConnection())
            {
                foreach (var recurringJob in connection.GetRecurringJobs())
                {
                    RecurringJobManager.RemoveIfExists(recurringJob.Id);
                }
            }
            RegisterRecurringJob<EmailMonitorWorker>(m => m.Run(), "*/5 * * * *", GetEasternTimeZoneInfo());
        }

        private static TimeZoneInfo GetEasternTimeZoneInfo()
        {
            try
            {
                var tz = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                   ? TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")
                   : TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
                return tz;
            }
            catch
            {
                return TimeZoneInfo.Local;
            }
        }
    }
}
