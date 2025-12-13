using Hangfire.Dashboard;

namespace MeetLines.API.Filters
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            // WARNING: This allows unauthenticated access to the Hangfire Dashboard.
            // Use only for development or behind a firewall/reverse proxy with auth.
            return true;
        }
    }
}
