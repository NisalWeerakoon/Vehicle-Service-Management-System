using Microsoft.EntityFrameworkCore;

namespace JobMaintenanceService.Data
{
    public class JobMaintenanceDbContext : DbContext
    {
        public JobMaintenanceDbContext(
            DbContextOptions<JobMaintenanceDbContext> options)
            : base(options)
        {
        }
    }
}
