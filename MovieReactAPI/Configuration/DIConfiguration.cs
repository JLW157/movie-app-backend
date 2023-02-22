using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using MovieReactAPI.APIBehavior;
using MovieReactAPI.Filters;
using MovieReactAPI.Helpers;
using NetTopologySuite.Geometries;
using NetTopologySuite;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace MovieReactAPI.Configuration
{
    public static class DIConfiguration
    {
        public static void RegisterCoreDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IFileStorageService, AzureStorageService>();

            services.AddDbContext<ApplicationDbContext>(opts =>
            {
                opts.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions.UseNetTopologySuite());
            });

            services.AddCors(opt =>
            {
                var frontUrl = configuration.GetValue<string>("front-url");

                opt.AddDefaultPolicy(builderCors =>
                {
                    builderCors.WithOrigins(frontUrl).AllowAnyMethod().AllowAnyHeader().
                    WithExposedHeaders(new string[] { "totalAmountOfRecords" });
                });
            });

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddSingleton<GeometryFactory>(NtsGeometryServices.
                Instance.CreateGeometryFactory(srid: 4326));

            services.AddSingleton(provider => new MapperConfiguration(config =>
            {
                var geometryFactory = provider.GetRequiredService<GeometryFactory>();

                config.AddProfile(new AutoMapperProfiles(geometryFactory));
            }).CreateMapper());

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opts =>
                {
                    JWTConfiguration jwtConfiguration = new JWTConfiguration();
                    configuration.Bind("JWT", jwtConfiguration);
                    opts.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtConfiguration.key)),
                        ClockSkew = TimeSpan.Zero
                    };
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(PolicyConstants.AdminPolicy, policy => policy.RequireClaim("role", "admin"));
            });
        }

        public static void RegisterCoreConfiguration(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.Configure<JWTConfiguration>(configuration.GetSection("JWT"));
        }
    }
}
