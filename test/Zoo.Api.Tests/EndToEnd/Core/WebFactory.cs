namespace Zoo.Api.Tests.EndToEnd.Core
{
    using System;

    using Infrastructure.Store;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    internal class WebFactory<TStartup, TContext>
        : WebApplicationFactory<TStartup>
        where TStartup : class
        where TContext : DbContext, IDbContext
    {
        private readonly Func<Configure<IDbContext>> getConfigureDbContext;

        private readonly Func<Configure<IServiceCollection>> getConfigureService;

        public WebFactory(
            Func<Configure<IServiceCollection>> getConfigureService,
            Func<Configure<IDbContext>> getConfigureDbContext)
        {
            this.getConfigureService = getConfigureService;
            this.getConfigureDbContext = getConfigureDbContext;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.UseEnvironment("Development");
            builder.UseTestServer()
                   .UseDefaultServiceProvider(
                       options =>
                           options.ValidateScopes = false)
                   .ConfigureTestServices(
                       services =>
                           {
                               this.getConfigureService()(services);
                               var serviceProvider = services.AddEntityFrameworkInMemoryDatabase()
                                                             .AddDbContextPool<IDbContext, TContext>(
                                                                 (sp, options) =>
                                                                     {
                                                                         options.UseInMemoryDatabase(
                                                                             "InMemoryDbForTesting");
                                                                         options.UseInternalServiceProvider(sp);
                                                                         options.EnableSensitiveDataLogging();
                                                                     }).BuildServiceProvider();
                               var dbContext = this.ConfigureDbContext(serviceProvider);
                               services.AddSingleton<IDbContext>(_ => dbContext);
                           });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
            }
        }

        private TContext ConfigureDbContext(IServiceProvider serviceProvider)
        {
            var dbContext = (TContext)serviceProvider.GetRequiredService<IDbContext>();
            dbContext.Database.EnsureCreated();
            var logging = serviceProvider
                .GetRequiredService<ILogger<WebFactory<TStartup, TContext>>>();

            try
            {
                this.getConfigureDbContext()(dbContext);
                dbContext.SaveChanges();
                return dbContext;
            }
            catch (Exception ex)
            {
                logging.LogError(ex, "Configure DbContext error");
                throw;
            }
        }
    }
}