using Microsoft.EntityFrameworkCore;

namespace CustomerBookingService.Data
{
    public class CustomerBookingDbContext : DbContext
    {
        public CustomerBookingDbContext(
            DbContextOptions<CustomerBookingDbContext> options)
            : base(options)
        {
        }
    }
}