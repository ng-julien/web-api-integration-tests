namespace Zoo.Infrastructure
{
    using System;

    using Adapters;

    using Administration.AnimalsRegistrationAggregate;

    using AutoMapper;

    using Contracts;
    using Contracts.Veterinary;

    using Entities;

    using Infirmary.VeterinaryAggregate;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using Park.Common.Adapters;

    using Store;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            Action<DbContextOptionsBuilder> dbContextConfiguration)
        {
            return services.AddAutoMapper(dbContextConfiguration.Method.DeclaringType.Assembly, typeof(ServiceCollectionExtensions).Assembly)
                           .AddScoped<IReader, Reader>()
                           .AddScoped<IWriter, Writer>()
                           .AddDbContextPool<IDbContext, ZooContext>(dbContextConfiguration)
                           .AddScoped<IRestrainedAnimalAdapter, RestrainedAnimalAdapter>()
                           .AddScoped<IAnimalsRegistrationAdapter, AnimalsRegistrationAdapter>()
                           .AddScoped<IVeterinaryAdapter, VeterinaryAdapter>()
                           .AddHttpClient("veterinary-list",
                               (provider, options) =>
                                   {
                                       var configuration = provider.GetRequiredService<IConfiguration>();
                                       configuration.Bind("veterinary-list", options);
                                   })
                           .AddTypedClient(Refit.RestService.For<IVeterinaryClient>)
                           .Services;
        }
    }
}