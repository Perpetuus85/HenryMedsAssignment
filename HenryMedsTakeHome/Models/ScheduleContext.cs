using Microsoft.EntityFrameworkCore;

namespace HenryMedsTakeHome.Models
{
    public class ScheduleContext : DbContext
    {
        public ScheduleContext(DbContextOptions<ScheduleContext> options) : base(options) { }

        public DbSet<Reservation> Reservations { get; set; } = null!;

        public DbSet<Client> Clients { get; set; } = null!;

        public DbSet<Provider> Providers { get; set; } = null!;

        public DbSet<RequestReservation> RequestReservations { get; set; }
    }
}
