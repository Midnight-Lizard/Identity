using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MidnightLizard.Web.Identity.Models;

namespace MidnightLizard.Web.Identity.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            modelBuilder.Entity(typeof(ApplicationUser).FullName, b =>
                {
                    b.Property<string>(nameof(ApplicationUser.Id));

                    b.Property<int>(nameof(ApplicationUser.AccessFailedCount));

                    b.Property<string>(nameof(ApplicationUser.ConcurrencyStamp))
                        .IsConcurrencyToken();

                    b.Property<string>(nameof(ApplicationUser.Email))
                        .HasAnnotation("MaxLength", 256);

                    b.Property<bool>(nameof(ApplicationUser.EmailConfirmed));

                    b.Property<bool>(nameof(ApplicationUser.LockoutEnabled));

                    b.Property<DateTimeOffset?>(nameof(ApplicationUser.LockoutEnd));

                    b.Property<string>(nameof(ApplicationUser.NormalizedEmail))
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>(nameof(ApplicationUser.NormalizedUserName))
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>(nameof(ApplicationUser.PasswordHash));

                    b.Property<string>(nameof(ApplicationUser.PhoneNumber));

                    b.Property<bool>(nameof(ApplicationUser.PhoneNumberConfirmed));

                    b.Property<string>(nameof(ApplicationUser.SecurityStamp));

                    b.Property<bool>(nameof(ApplicationUser.TwoFactorEnabled));

                    b.Property<string>(nameof(ApplicationUser.UserName))
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>(nameof(ApplicationUser.DisplayName))
                        .HasAnnotation("MaxLength", 256);

                    b.HasKey(nameof(ApplicationUser.Id));

                    b.HasIndex(nameof(ApplicationUser.NormalizedEmail))
                        .HasName("EmailIndex");

                    b.HasIndex(nameof(ApplicationUser.NormalizedUserName))
                        .IsUnique()
                        .HasName("UserNameIndex");

                    b.ToTable("AspNetUsers");
                });
        }
    }
}
