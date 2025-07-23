using Hangfire.Dashboard;
namespace Desafio_Kinetic.Dashboard
{
    public class AllowAllDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            return true; // permite acceso sin autenticación
        }
    }
}