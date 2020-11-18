namespace Zoo.Infrastructure
{
    using System;

    using Adapters;

    using Administration.AnimalsRegistrationAggregate;

    using AutoMapper;

    using Entities;

    using Microsoft.EntityFrameworkCore;
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
                           .AddScoped<IAnimalsRegistrationAdapter, AnimalsRegistrationAdapter>();
        }
    }
}