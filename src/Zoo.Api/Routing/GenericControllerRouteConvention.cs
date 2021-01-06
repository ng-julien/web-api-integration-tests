namespace Zoo.Api.Routing
{
    using System;
    using System.Linq;

    using Humanizer;

    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.EntityFrameworkCore.Infrastructure;

    internal class GenericControllerRouteConvention : IControllerModelConvention
    {
        private readonly Type[] controllerTypes;

        public GenericControllerRouteConvention(params Type[] controllerTypes)
        {
            this.controllerTypes = controllerTypes;
        }
        public void Apply(ControllerModel controller)
        {
            if (!controller.ControllerType.IsGenericType)
            {
                return;
            }
            
            var className = controller.ControllerType.ShortDisplayName().Humanize();
            if (this.controllerTypes.Any(controllerType => controllerType.Name == controller.ControllerType.Name))
            { 
                var genericType = controller.ControllerType.GenericTypeArguments[0];
                className = genericType.ShortDisplayName().Humanize();
            }
            
            var controllerName = string.Concat(className.TakeWhile(c => c != ' '))
                                       .Transform(To.LowerCase)
                                       .Pluralize();

            controller.ControllerName = controllerName;
        }
    }
}