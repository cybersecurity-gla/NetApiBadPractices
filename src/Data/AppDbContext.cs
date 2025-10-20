using Microsoft.EntityFrameworkCore;
using BadApiExample.Models;

namespace BadApiExample.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Person> Persons { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Person entity
        modelBuilder.Entity<Person>(entity =>
        {
            // Primary key
            entity.HasKey(e => e.Id);

            // Indexes for performance
            entity.HasIndex(e => e.Email)
                  .IsUnique()
                  .HasDatabaseName("IX_Persons_Email");

            entity.HasIndex(e => e.Name)
                  .HasDatabaseName("IX_Persons_Name");

            entity.HasIndex(e => e.IsActive)
                  .HasDatabaseName("IX_Persons_IsActive");

            entity.HasIndex(e => e.IsDeleted)
                  .HasDatabaseName("IX_Persons_IsDeleted");

            entity.HasIndex(e => e.CreatedDate)
                  .HasDatabaseName("IX_Persons_CreatedDate");

            // Composite index for soft delete and active status
            entity.HasIndex(e => new { e.IsDeleted, e.IsActive })
                  .HasDatabaseName("IX_Persons_IsDeleted_IsActive");

            // Property configurations
            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.Email)
                  .IsRequired()
                  .HasMaxLength(255);

            entity.Property(e => e.Phone)
                  .HasMaxLength(20);

            entity.Property(e => e.Address)
                  .HasMaxLength(500);

            entity.Property(e => e.CreatedDate)
                  .IsRequired()
                  .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.IsActive)
                  .IsRequired()
                  .HasDefaultValue(true);

            entity.Property(e => e.IsDeleted)
                  .IsRequired()
                  .HasDefaultValue(false);

            // Configure table with check constraints
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Persons_Age", "[Age] >= 1 AND [Age] <= 120");
                t.HasCheckConstraint("CK_Persons_Email", "LEN([Email]) > 0");
                t.HasCheckConstraint("CK_Persons_Name", "LEN([Name]) >= 2");
            });

            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<Person>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted);

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedDate = DateTime.UtcNow;
                    entry.Entity.IsActive = true;
                    entry.Entity.IsDeleted = false;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedDate = DateTime.UtcNow;
                    break;

                case EntityState.Deleted:
                    // Implement soft delete
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedDate = DateTime.UtcNow;
                    entry.Entity.IsActive = false;
                    break;
            }
        }
    }
}