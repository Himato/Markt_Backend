using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using Markt.Helpers;
using Markt.Models;
using Markt.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Markt
{
    public static class StringExtenstion
    {
        public static string GetUniqueUri(this string str, bool withKey)
        {
            if (!withKey)
            {
                return str.ExcludeNonAlpha();
            }

            var date = DateTime.Now.Date;
            var key = $"{date.Year:X}{date.Month:X}{date.Day:X}".ToLower();

            return $"{str.ExcludeNonAlpha()}-{key}";
        }

        private static string ExcludeNonAlpha(this string str)
        {
            return str.ToLower().Replace(' ', '-').Where(c => c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c >= '0' && c <= '9' || c == '-')
                .Aggregate("", (current, next) => current + next);
        }

        public static string ToCamelCase(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;

            var first = str.Substring(0, 1).ToLower();
            if (str.Length == 1) return first;

            return first + str.Substring(1);
        }
    }

    public static class DbUpdateExceptionHandler
    {
        public static string GetExceptionMessage(this DbUpdateException exception)
        {
            return exception.InnerException?.Message ?? exception.Message;
        }
    }

    public static class IdentityExtenstion
    {
        public static string GetUserId(this IIdentity identity)
        {
            return ((ClaimsIdentity)identity)?.Claims.FirstOrDefault(c => c.Type.Equals(nameof(ClaimTypes.NameIdentifier).ToCamelCase()))?.Value;
        }

        public static string GetUsername(this IIdentity identity)
        {
            return ((ClaimsIdentity)identity)?.Claims.FirstOrDefault(c => c.Type.Equals(nameof(ClaimTypes.Name).ToCamelCase()))?.Value;
        }
    }

    public static class StartupExtenstion
    {
        public static void AddServices(this IServiceCollection services)
        {
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IBrandService, BrandService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<ICartService, CartService>();
        }

        public static void ConfigureMvcApi(this IServiceCollection services, CompatibilityVersion version)
        {
            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                })
                .SetCompatibilityVersion(version);

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errorResponse = new ApiError(context.ModelState);
                    return new BadRequestObjectResult(errorResponse);
                };
            });

            services.AddResponseCaching();
        }

        public static void ConfigureEntityFramework(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("LocalDb")));
        }

        public static void ConfigureIdentity(this IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, UserRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options => 
            { 
                options.User.RequireUniqueEmail = true;
                // Default Password settings.
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 0;
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(nameof(UserRole.Seller), policy => policy.RequireClaim(nameof(ClaimTypes.Role), UserRole.Seller, UserRole.Admin));
                options.AddPolicy(nameof(UserRole.Admin), policy => policy.RequireClaim(nameof(ClaimTypes.Role), UserRole.Admin));
            });
        }

        public static void ConfigureJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // => remove default claims
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(cfg =>
                {
                    cfg.RequireHttpsMetadata = true;
                    cfg.SaveToken = true;
                    cfg.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"])),
                        ValidateIssuer = false,
                        ValidIssuer = configuration["Jwt:Issuer"],
                        ValidateAudience = false,
                        ValidAudience = configuration["Jwt:Issuer"],
                        ClockSkew = TimeSpan.Zero // remove delay of token when expire
                    };
                });
        }

        public static void AddCors(this IServiceCollection services)
        {
            services.AddCors(options => options.AddPolicy("CorsPolicy",
                builder =>
                {
                    builder.AllowAnyMethod().AllowAnyHeader()
                        .WithOrigins("http://markt.mesawer.com")
                        .AllowCredentials();
                }));
        }
    }
}
