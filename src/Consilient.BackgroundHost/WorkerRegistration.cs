using Consilient.Background.Workers;
using Hangfire;
using Hangfire.Storage;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace Consilient.BackgroundHost
{
    internal class WorkerRegistration(IBackgroundJobClient backgroundJobClient, IRecurringJobManager recurringJobManager, JobStorage jobStorage)
    {
        private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;
        private readonly JobStorage _jobStorage = jobStorage;
        private readonly IRecurringJobManager _recurringJobManager = recurringJobManager;

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
            _recurringJobManager.AddOrUpdate(typeof(TRecurringJob).Name, methodCall, cronExpression, new RecurringJobOptions { TimeZone = timeZoneInfo ?? TimeZoneInfo.Local });
        }

        private void RegisterRecurringJobs()
        {
            using (var connection = _jobStorage.GetConnection())
            {
                foreach (var recurringJob in connection.GetRecurringJobs())
                {
                    _recurringJobManager.RemoveIfExists(recurringJob.Id);
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
