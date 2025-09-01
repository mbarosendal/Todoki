using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System;
using System.Text;
using TickTask.Server.Data;
using TickTask.Server.Data.Models;
using TickTask.Server.Services;

namespace TickTask
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddLogging();
            builder.Logging.AddConsole();

            builder.Services.AddScoped<IProjectService, ProjectService>();

            var jwtSecret = builder.Configuration["JWT:SecretKey"];
            var adminPassword = builder.Configuration["Admin:Password"];
            var adminEmail = builder.Configuration["Admin:Email"];

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithEnvironmentName()
                //.MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // filters out debug level logs, less noise, but bad practice
                //.MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning) // filters out debug level logs, less noise, but bad practice
                .MinimumLevel.Information()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties}{NewLine}{Exception}")
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            builder.Host.UseSerilog();

            // Paraameters for AddJwtBearer token
            var tokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"])),

                ValidateIssuer = true,
                ValidIssuer = builder.Configuration["JWT:Issuer"],

                ValidateAudience = true,
                ValidAudience = builder.Configuration["JWT:Audience"],

                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1),

                RequireExpirationTime = true,
                RequireSignedTokens = true,
                RequireAudience = true
            };
            builder.Services.AddSingleton(tokenValidationParameters);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password settings
                //options.Password.RequireDigit = false;
                options.Password.RequiredLength = 8;
                //options.Password.RequireNonAlphanumeric = false;
                //options.Password.RequireUppercase = false;
                //options.Password.RequireLowercase = false;

                // Lockout settings for wrong password attempts
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;

                // User settings
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment(); // true if not development
                    options.TokenValidationParameters = tokenValidationParameters; // https://www.linkedin.com/learning/asp-dot-net-core-token-based-authentication/injecting-tokenvalidationparameters?autoSkip=true&resume=false&u=57075649
                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async context =>
                        {
                            var dbContext = context.HttpContext.RequestServices
                                .GetRequiredService<ApplicationDbContext>();
                            var jti = context.Principal.FindFirst("jti")?.Value;

                            var isRevoked = await dbContext.RefreshTokens
                                .AnyAsync(rt => rt.JwtId == jti && rt.IsRevoked);

                            if (isRevoked)
                            {
                                context.Fail("Token has been revoked");
                            }
                        }
                    };
                });

            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Swagger with authorization", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Name = "Authorization",
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    Description = "Please enter JWT token:"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            builder.Services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("AuthPolicy", config =>
                {
                    config.Window = TimeSpan.FromMinutes(1);
                    config.PermitLimit = 5; // 5 login attempts per minute
                    config.QueueLimit = 0;
                });

                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.HttpContext.Response.WriteAsync("Too many login attempts. Please try again later", token);
                };
            });

            var app = builder.Build();

            app.MapGet("/checkin", () => "Check in confirmed");                

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseMigrationsEndPoint();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRateLimiter();

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();

                try
                {
                    await AppDbInitializer.SeedRolesAsync(services);

                    await AppDbInitializer.SeedAdminAsync(services, adminEmail, adminPassword, logger);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }

            app.MapRazorPages();
            app.MapControllers();
            app.MapFallbackToFile("index.html");

            await app.RunAsync();
        }
    }
}
