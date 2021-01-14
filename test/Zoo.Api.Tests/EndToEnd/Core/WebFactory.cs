namespace Zoo.Api.Tests.EndToEnd.Core
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Security.Cryptography;

    using Infrastructure.Store;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Authorization.Policy;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Tokens;

    internal class WebFactory<TStartup, TContext>
        : WebApplicationFactory<TStartup>
        where TStartup : class
        where TContext : DbContext, IDbContext
    {
        private readonly Func<Configure<IDbContext>> getConfigureDbContext;

        private readonly Func<Configure<IServiceCollection>> getConfigureService;

        private readonly (SigningCredentials SigningCredentials, SymmetricSecurityKey symmetricSecurityKey, string
            Issuer, string Audience) tokenConfiguration;

        public WebFactory(
            Func<Configure<IServiceCollection>> getConfigureService,
            Func<Configure<IDbContext>> getConfigureDbContext)
        {
            this.getConfigureService = getConfigureService;
            this.getConfigureDbContext = getConfigureDbContext;
            var key = new byte[32];
            var randomGenerator = RandomNumberGenerator.Create();
            randomGenerator.GetBytes(key);
            var symmetricSecurityKey = new SymmetricSecurityKey(key) { KeyId = Guid.NewGuid().ToString() };
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
            this.tokenConfiguration = (signingCredentials, symmetricSecurityKey, Guid.NewGuid().ToString(),
                                          "fake-audience");
        }
        
        public string GetBearerToken(List<Claim> claims)
        {
            if (!claims.Any())
                return string.Empty;

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = new JwtSecurityToken(
                this.tokenConfiguration.Issuer,
                this.tokenConfiguration.Audience,
                claims,
                null,
                DateTime.UtcNow.AddMinutes(20),
                this.tokenConfiguration.SigningCredentials);
            return tokenHandler.WriteToken(jwtSecurityToken);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            // Definie l'environnement qui sera utilisé lors de l'execution des tests
            builder.UseEnvironment(Environments.Development);
            builder.UseTestServer()
                   .ConfigureAppConfiguration(
                       configurationBuilder =>
                           {
                               var settings = new Dictionary<string, string>
                                                  {
                                                      { "OidcOptions:Authority", string.Empty }
                                                  };
                               configurationBuilder.AddInMemoryCollection(settings);
                           })
                   .ConfigureTestServices(
                       services =>
                           {
                               services.AddSingleton<IPolicyEvaluator>(
                                   provider => new FakePolicyEvaluator(
                                       provider.GetService<IAuthorizationService>(),
                                       this.tokenConfiguration));
                               this.getConfigureService()(services);
                               var serviceCollection = new ServiceCollection();
                               var serviceProvider = serviceCollection.AddEntityFrameworkInMemoryDatabase()
                                                                      .AddDbContextPool<IDbContext, TContext>(
                                                                          (sp, options) =>
                                                                              {
                                                                                  options.UseInMemoryDatabase(
                                                                                      "InMemoryDbForTesting");
                                                                                  options
                                                                                      .UseInternalServiceProvider(
                                                                                          sp);
                                                                                  options
                                                                                      .EnableSensitiveDataLogging();
                                                                              })
                                                                      .AddLogging()
                                                                      .BuildServiceProvider();
                               var dbContext = this.ConfigureDbContext(serviceProvider);
                               services.AddSingleton<IDbContext>(dbContext);
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