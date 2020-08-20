using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using FluentValidation;
using FluentValidation.AspNetCore;
using Godwit.Common.Data;
using Godwit.Common.Data.Model;
using Godwit.HandleRegistrationAction.Model;
using Godwit.HandleRegistrationAction.Model.Validators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using StackExchange.Redis;

namespace Godwit.HandleRegistrationAction {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            var dbConn =
                $"Server={Configuration["DB_HOST"]};Database={Configuration["HASURA_DB"]};User Id={Configuration["DB_USERNAME"]};Password={Configuration["DB_PASSWORD"]};";

            var redisConn =
                $"{Configuration["REDIS_HOST"] ?? "localhost:6379"},allowAdmin=true,password={Configuration["REDIS_PASSWORD"]}";
            var certificate = new X509Certificate2(
                Path.Combine(Configuration["CERT_PATH"], Configuration["CERT_FILENAME"]),
                Configuration["CERT_PASSWORD"], X509KeyStorageFlags.Exportable);
            var redis = ConnectionMultiplexer.Connect(redisConn);

            services.AddControllers()
                .AddFluentValidation(cfg => cfg.AutomaticValidationEnabled = false);
            services.AddSingleton<IValidator<ActionData>, ActionDataValidator>();
            services
                .AddDbContextPool<KetoDbContext>(opt => {
                    opt.UseNpgsql(dbConn, builder => {
                            builder.EnableRetryOnFailure(10, TimeSpan.FromMilliseconds(100), null!);
                            builder.CommandTimeout(60);
                            builder.UseAdminDatabase("postgres");
                            builder.UseNodaTime();
                        })
                        .UseSnakeCaseNamingConvention();
                });
            services
                .AddIdentity<User, IdentityRole<string>>()
                .AddEntityFrameworkStores<KetoDbContext>()
                .AddDefaultTokenProviders();
            services.AddSingleton<IConnectionMultiplexer>(c => redis);
            services.Configure<DataProtectionTokenProviderOptions>(o => { o.TokenLifespan = TimeSpan.FromHours(24); });
            services
                .AddDataProtection(cfg => cfg.ApplicationDiscriminator = "id.godwit")
                .PersistKeysToStackExchangeRedis(redis)
                .ProtectKeysWithCertificate(certificate);

            services.AddHealthChecks()
                .AddNpgSql(dbConn);
            services.AddCors();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            app.UseForwardedHeaders(new ForwardedHeadersOptions {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();
            app.UseCors(opt => {
                opt.AllowAnyHeader();
                opt.WithMethods("POST");
                opt.AllowAnyOrigin();
            });
            app.UseRouting();

            app.UseAuthorization();
            app.UseSerilogRequestLogging();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/hc");
            });
        }
    }
}