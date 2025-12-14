/*
*	<copyright file="AppDbContext">
*	</copyright>
* 	<author>Marco Macedo</author>
*   <date>2025 12/13/2025 9:05:36 PM</date>
*	<description></description>
**/

using Backend_sec_dev.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend_sec_dev.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<AuthSession> AuthSessions => Set<AuthSession>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }
}