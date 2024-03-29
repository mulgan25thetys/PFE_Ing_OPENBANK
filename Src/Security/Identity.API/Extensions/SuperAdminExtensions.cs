using Identity.API.Applications.Data;
using Identity.API.Applications.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Data;
using Identity.API.Utils;

namespace Identity.API.Extensions
{
    public static class SuperAdminExtensions
    {
        public async static Task<IHost> CreateSuperAdmin<TContext>(this IHost host,
           Action<TContext, IServiceProvider> seeder, int? retry = 0) where TContext : IdentityContext
        {
            int retryForAvailability = retry.Value;
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<TContext>>();
                var config = services.GetRequiredService<IConfiguration>();
                var context = services.GetService<TContext>();
                var userManager = services.GetService<UserManager<UserModel>>();
                var roleManager = services.GetService<RoleManager<Entitlement>>();
                var hostUrl = services.GetService<HttpRequest>();

                try
                {
                    logger.LogInformation("Creating super admin associated with context {DbContextName}", typeof(TContext).Name);

                    await InvokeSeeder(seeder, context, services, config, userManager, roleManager,hostUrl);

                    logger.LogInformation("Creating super admin associated with context {DbContextName}", typeof(TContext).Name);

                }
                catch (SqlException ex)
                {
                    logger.LogError(ex, "An error occurred while creating super admin used on context {DbContextName}", typeof(TContext).Name);

                    if (retryForAvailability < 50)
                    {
                        retryForAvailability++;
                        System.Threading.Thread.Sleep(2000);
                        await CreateSuperAdmin<TContext>(host, seeder, retryForAvailability);
                    }
                }
            }
            return host;
        }

        private async static Task InvokeSeeder<TContext>(Action<TContext, IServiceProvider> seeder,
                                                   TContext _identityContext,
                                                   IServiceProvider services,
                                                   IConfiguration _configuration,
                                                   UserManager<UserModel> userManager,
                                                   RoleManager<Entitlement> roleManager,
                                                   HttpRequest request)
                                                   where TContext : IdentityContext
        {
            var adminUser = await userManager.FindByNameAsync(_configuration.GetValue<string>("AppRoles:AdminRole:name"));

            if (adminUser == null)
            {
                UserModel admin = new UserModel()
                {
                    Email = _configuration.GetValue<string>("AppRoles:AdminRole:email"),
                    EmailConfirmed = true,
                    PhoneNumber = _configuration.GetValue<string>("AppRoles:AdminRole:phoneNumber"),
                    PhoneNumberConfirmed = true,
                    AccessFailedCount = _configuration.GetValue<int>("AppRoles:AdminRole:AccessFailedCount"),
                    LockoutEnabled = _configuration.GetValue<bool>("AppRoles:AdminRole:LockoutEnabled"),
                    TwoFactorEnabled = true,
                    UserName = _configuration.GetValue<string>("AppRoles:AdminRole:name"),
                
                };
                admin = UserProviderDetails.GetUriProviderDetails(request, admin);
                admin.NormalizedEmail = admin.Email;
                var success = await userManager.CreateAsync(admin, _configuration.GetValue<string>("AppRoles:AdminRole:password"));

                if (success.Succeeded)
                {
                    Entitlement adminRole = new Entitlement { Bank_id = "", Name = _configuration.GetValue<string>("AppRoles:AdminRole:name") };
                    adminRole.NormalizedName = adminRole.Name;

                    if (!await roleManager.RoleExistsAsync(adminRole.Name))
                    {
                        await roleManager.CreateAsync(adminRole);
                    }

                    IEnumerable<string> roles = new List<string>() { adminRole.Name };
                    admin = await userManager.FindByNameAsync(admin.UserName);

                    if (admin != null)
                    {
                        await userManager.AddToRolesAsync(admin, roles);
                    }
                }
            }
            seeder(_identityContext, services);
        }
    }
}
