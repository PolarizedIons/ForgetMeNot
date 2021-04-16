using System;
using ForgetMeNot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ForgetMeNot.Database
{
    public sealed class DatabaseContext : DbContext
    {
        public DbSet<GuildSettings> GuildSettings { get; set; } = null!;
        public DbSet<Quote> Quotes { get; set; } = null!;

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
            ChangeTracker.StateChanged += OnEntityStateChanged;
            ChangeTracker.Tracked += OnEntityTracked;
        }

        private void OnEntityStateChanged(object? sender, EntityStateChangedEventArgs e)
        {
            if (e.NewState == EntityState.Modified && e.Entry.Entity is DbEntity entity)
            {
                entity.ModifiedAt = DateTime.UtcNow;
            }
        }

        private void OnEntityTracked(object? sender, EntityTrackedEventArgs e)
        {
            if (e.Entry.State == EntityState.Added && e.Entry.Entity is DbEntity entity)
            {
                entity.Id = Guid.NewGuid();
                entity.CreatedAt = DateTime.UtcNow;
                entity.ModifiedAt = DateTime.UtcNow;
            }
        }
    }
}
