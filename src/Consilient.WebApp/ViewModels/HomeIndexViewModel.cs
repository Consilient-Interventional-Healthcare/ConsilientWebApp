namespace Consilient.WebApp.ViewModels
{
    public class HomeIndexViewModel
    {
        public int EncountersToday { get; set; }
        public int PendingApprovals { get; set; }
        public int ApprovedEncounters { get; set; }
        public int ErrorEncounters { get; set; }

        public int Last7DaysApproved { get; set; }
        public int Last7DaysPending { get; set; }

        public TimeframeOptions SelectedTimeframe { get; set; } = TimeframeOptions.Today;

        public enum TimeframeOptions
        {
            Today,
            Last7Days,
            Last30Days,
            AllTime
        }

        public DateOnly LowerDateRange
        {
            get
            {
                return SelectedTimeframe switch
                {
                    TimeframeOptions.Today => DateOnly.FromDateTime(DateTime.Today),
                    TimeframeOptions.Last7Days => DateOnly.FromDateTime(DateTime.Today.AddDays(-7)),
                    TimeframeOptions.Last30Days => DateOnly.FromDateTime(DateTime.Today.AddDays(-30)),
                    TimeframeOptions.AllTime => DateOnly.FromDateTime(DateTime.MinValue),
                    _ => DateOnly.FromDateTime(DateTime.Today)
                };
            }
        }
    }
}

