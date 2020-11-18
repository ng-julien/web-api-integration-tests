namespace Zoo.Api
{
    using Application;

    using Controllers;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

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

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
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