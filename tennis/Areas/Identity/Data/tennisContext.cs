using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using tennis.Areas.Identity.Data;
using tennis.Models;

namespace tennis.Data;

public class tennisContext : IdentityDbContext<tennisUser>
{
    public tennisContext(DbContextOptions<tennisContext> options)
        : base(options)
    {
    }

    public DbSet<Coach> Coaches { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<MemberSchedule> MemberSchedules { get; set; }
    public DbSet<Schedule> Schedules { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure the relationships
        builder.Entity<Member>()
            .HasOne(m => m.User)
            .WithOne(u => u.Member)
            .HasForeignKey<Member>(m => m.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Coach>()
            .HasOne(c => c.User)
            .WithOne(u => u.Coach)
            .HasForeignKey<Coach>(c => c.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }
}
