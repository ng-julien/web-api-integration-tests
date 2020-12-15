namespace Zoo.Api
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using Application;

    using Controllers;

    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.IdentityModel.Tokens;

    using Options;

    using Routing;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            
            app.UseAuthentication();

            app.UseAuthorization();
            
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                    .Services
                    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(
                        options =>
                            {
                                var opt = this.Configuration.Get<JwtBearerOptions>();
                                this.Configuration.GetSection("OidcOptions").Bind(options);
                                options.RequireHttpsMetadata = false;
                                options.TokenValidationParameters = new TokenValidationParameters
                                                                        {
                                                                            RoleClaimType = ClaimTypes.Role,
                                                                            ValidIssuer = options.Authority,
                                                                            ValidateAudience = true,
                                                                            ValidateIssuer = true,
                                                                            ValidateLifetime = true,
                                                                            RequireSignedTokens = true,
                                                                            NameClaimType = ClaimTypes.NameIdentifier
                                                                        };
                            })
                    .Services
                    .AddMvc(
                        options =>
                            {
                                options.Conventions.Add(
                                    new GenericControllerRouteConvention()
                                );
                            })
                    .ConfigureApplicationPartManager(
                        m =>
                            m.FeatureProviders.Add(
                                new GenericControllerFeatureProvider(typeof(AnimalsController<,,>))
                            ))
                    .Services
                    .AddApiVersioning(
                        config => { config.ReportApiVersions = true; })
                    .AddVersionedApiExplorer(
                        options =>
                            {
                                options.GroupNameFormat = "'v'VVV";
                                options.SubstituteApiVersionInUrl = true;
                            })
                    .AddApplication(
                        builder =>
                            {
                                builder.UseSqlServer(
                                    this.Configuration.GetConnectionString("ZooContext"),
                                    options => options.EnableRetryOnFailure());
                            });
        }
    }
}