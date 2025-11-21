using Consilient.Background.Workers;
using Consilient.Background.Workers.Contracts;
using Hangfire;
using Hangfire.Storage;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace Consilient.BackgroundHost
{
    internal class WorkerRegistration(IRecurringJobManager recurringJobManager, JobStorage jobStorage)
    {
        public void Register()
        {
            RegisterRecurringJobs();
            RegisterJobsThatMustRunInStartup();
        }

        private static void RegisterJobsThatMustRunInStartup()
        {
        }

        private void RegisterRecurringJob<TRecurringJob>(Expression<Action<TRecurringJob>> methodCall, string cronExpression, TimeZoneInfo? timeZoneInfo = null) where TRecurringJob : IRecurringWorker
        {
            recurringJobManager.AddOrUpdate(typeof(TRecurringJob).Name, methodCall, cronExpression, new RecurringJobOptions { TimeZone = timeZoneInfo ?? TimeZoneInfo.Local });
        }

        private void RegisterRecurringJobs()
        {
            using (var connection = jobStorage.GetConnection())
            {
                foreach (var recurringJob in connection.GetRecurringJobs())
                {
                    recurringJobManager.RemoveIfExists(recurringJob.Id);
                }
            }
            RegisterRecurringJob<EmailMonitorWorker>(m => m.Run(CancellationToken.None), "*/5 * * * *", GetEasternTimeZoneInfo());
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
