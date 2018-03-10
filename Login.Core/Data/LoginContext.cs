using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Login.Core.Data
{
    public class LoginContext : DbContext
    {
        public LoginContext(DbContextOptions<LoginContext> options) : base(options)
        {}

        public DbSet<Models.Login> Logins { get; set; }
        public DbSet<Models.UserSite> UserSites { get; set; }
        public DbSet<Models.User> Users { get; set; }


        public override int SaveChanges()
        {
            AddTimestamps();
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            AddTimestamps();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            AddTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            AddTimestamps();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Models.Login>(e =>
            {
                e.ToTable("login");
                e.HasKey(b => b.Id);

                e.Property(b => b.Id).IsRequired().ValueGeneratedOnAdd();
                e.Property(b => b.UserDisplayName).IsRequired().HasMaxLength(128);
                e.Property(b => b.UserName).IsRequired().HasMaxLength(128);
            });

            modelBuilder.Entity<Models.UserSite>(e =>
            {
                e.ToTable("usersite");
                e.HasKey(b => b.Name);
                e.Property(b => b.Name).IsRequired().HasMaxLength(128);
                e.Property(b => b.Url).IsRequired().HasMaxLength(255);
                e.Property(b => b.PermissionList).IsRequired();
                e.Ignore(b => b.Permissions);
                e.Property(b => b.Timestamp).ValueGeneratedOnAddOrUpdate().IsConcurrencyToken();

                e.HasOne(b => b.User).WithMany(b => b.Sites).HasForeignKey(b => b.UserEmail);
            });

            modelBuilder.Entity<Models.User>(e =>
            {
                e.ToTable("user");
                e.HasKey(b => b.Email);
                e.Property(b => b.Email).IsRequired().HasMaxLength(128);
                e.Property(b => b.Name).IsRequired().HasMaxLength(128);
                e.Property(b => b.DisplayName).IsRequired().HasMaxLength(255);
                e.Property(b => b.Timestamp).ValueGeneratedOnAddOrUpdate().IsConcurrencyToken();
            });
        }

        private void AddTimestamps()
        {
            var entities = ChangeTracker.Entries().Where(x => x.Entity is Models.BaseModel && (x.State == EntityState.Added || x.State == EntityState.Modified));

            foreach (var entity in entities)
            {
                if (entity.State == EntityState.Added)
                {
                    ((Models.BaseModel)entity.Entity).Created = DateTime.UtcNow;
                }

                ((Models.BaseModel)entity.Entity).Modified = DateTime.UtcNow;
            }
        }

    }
}
