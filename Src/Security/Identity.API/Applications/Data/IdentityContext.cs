using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Applications.Data
{
    public class IdentityContext : IdentityDbContext
    {
        public IdentityContext(DbContextOptions<IdentityContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IdentityUserLogin<string>>(id => id.HasKey(k => k.UserId));
            modelBuilder.Entity<IdentityUserRole<string>>(ur => ur.HasKey(k => new
            {
                k.RoleId,
                k.UserId
            }));
            modelBuilder.Entity<IdentityUserToken<string>>(ut => ut.HasKey(k => new
            {
                k.UserId,
                k.LoginProvider,
                k.Name
            }));
        }
    }
}
