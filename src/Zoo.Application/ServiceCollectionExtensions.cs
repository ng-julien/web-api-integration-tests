namespace Zoo.Application
{
    using System;

    using AxaFrance.Extensions.ServiceModel.Settings;

    using Commands;

    using Infrastructure;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;

    using Queries;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services,
            Action<DbContextOptionsBuilder> dbContextConfiguration,
            Action<ServiceConfiguration<BasicAuth>> bookServiceBuilder)
        {
            return services.AddScoped<IGetRestrainedAnimalsQuery, GetRestrainedAnimalsQuery>()
                           .AddScoped<IAnimalRegistrationCommand, AnimalRegistrationCommand>()
                           .AddScoped<IGetVeterinariesQuery, GetVeterinariesQuery>()
                           .AddScoped<IGetBooksAboutQuery, GetBooksAboutQuery>()
                .AddInfrastructure(dbContextConfiguration, bookServiceBuilder);
        }
    }
}