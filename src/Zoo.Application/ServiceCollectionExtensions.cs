namespace Zoo.Application
{
    using System;

    using Commands;

    using Infrastructure;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;

    using Queries;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services,
            Action<DbContextOptionsBuilder> dbContextConfiguration)
        {
            return services.AddScoped<IGetRestrainedAnimalsQuery, GetRestrainedAnimalsQuery>()
                           .AddScoped<IAnimalRegistrationCommand, AnimalRegistrationCommand>()
                           .AddScoped<IGetVeterinariesQuery, GetVeterinariesQuery>()
                .AddInfrastructure(dbContextConfiguration);
        }
    }
}