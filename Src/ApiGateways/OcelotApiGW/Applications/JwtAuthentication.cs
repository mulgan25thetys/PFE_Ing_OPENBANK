using Branch.API.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using OcelotApiGW.API.Middlewares;
using OcelotApiGW.API.Utils.Interfaces;
using OcelotApiGW.API.Utils.Models;
using System.Text;

namespace OcelotApiGW.Applications
{
    public static class JwtAuthentication
    {
        public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var section = configuration.GetSection("JWT");
            var options = section.Get<JwtOptions>();
            var key = Encoding.UTF8.GetBytes(options.Secret);
            section.Bind(options);
            services.Configure<JwtOptions>(section);

            services.AddSingleton<IJwtUtils, JwtUtils>();
            services.AddTransient<JwtMiddleware>();

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(x =>
                {
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });

            services.AddAuthorization(x =>
            {
                x.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();
            });
        }
    }
}
