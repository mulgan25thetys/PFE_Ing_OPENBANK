using Branch.API.Utils;
using Identity.API.Utils.Interfaces;
using Identity.API.Utils.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Identity.API.Applications.Security
{
    public static class SecurityCors
    {
        public static void AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwt = new JwtOptions();
            var section = configuration.GetSection("jwt");
            section.Bind(jwt);
            services.Configure<JwtOptions>(section);
            services.AddScoped<IJwtUtils, JwtUtils>();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddCookie()
            .AddJwtBearer(option =>
            {
                option.SaveToken = true;
                option.Authority = jwt.ValidIssuer;
                option.RequireHttpsMetadata = false;
                option.TokenValidationParameters = new TokenValidationParameters()
                {
                    //ValidateIssuer = false,
                    ValidateAudience = false,
                    //ValidAudience = configuration["JWT:ValidAudience"],
                    //ValidIssuer = configuration["JWT:ValidIssuer"],
                    //ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret))
                };
            });
        }
    }
}
