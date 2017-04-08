using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Model = Login.Core.Models;

namespace Login.Core.Data
{
    public class LoginContext : DbContext
    {
        public LoginContext(DbContextOptions<LoginContext> options) : base(options)
        {}

        public DbSet<Model.Login> Logins { get; set; }
        public DbSet<Model.UserSite> UserSites { get; set; }
        public DbSet<Model.User> Users { get; set; }


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

        public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
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

            modelBuilder.Entity<Model.Login>(e =>
            {
                e.ToTable("login");
                e.HasKey(b => b.Id);

                e.Property(b => b.Id).IsRequired().ValueGeneratedOnAdd();
                e.Property(b => b.UserDisplayName).IsRequired().HasMaxLength(128);
                e.Property(b => b.UserName).IsRequired().HasMaxLength(128);
            });

            modelBuilder.Entity<Model.UserSite>(e =>
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

            modelBuilder.Entity<Model.User>(e =>
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
            var entities = ChangeTracker.Entries().Where(x => x.Entity is Model.BaseModel && (x.State == EntityState.Added || x.State == EntityState.Modified));

            foreach (var entity in entities)
            {
                if (entity.State == EntityState.Added)
                {
                    ((Model.BaseModel)entity.Entity).Created = DateTime.UtcNow;
                }

                ((Model.BaseModel)entity.Entity).Modified = DateTime.UtcNow;
            }
        }

    }
}
