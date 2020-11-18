namespace Zoo.Api.Tests.EndToEnd
{
    using System;

    using Core;

    using Infrastructure.Store;

    using Microsoft.Extensions.DependencyInjection;

    using TechTalk.SpecFlow;

    internal delegate void Assertion(IServiceProvider provider);

    internal delegate void Configure<in TDbContext>(TDbContext context);

    [Binding]
    public class StepsInitialize
    {
        private readonly ScenarioContext scenarioContext;

        public StepsInitialize(ScenarioContext scenarioContext)
        {
            this.scenarioContext = scenarioContext;
        }

        [AfterScenario]
        public void AfterScenario()
        {
            var webFactory = this.scenarioContext.Get<WebFactory<Startup, EndToEndDbContext>>();
            this.scenarioContext.Get<Assertion>()(webFactory.Server.Services);
            webFactory.Dispose();
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            this.scenarioContext.Set<Configure<IDbContext>>(context => { });
            this.scenarioContext.Set<Configure<IServiceCollection>>(services => { });
            this.scenarioContext.Set<Assertion>(context => { });
            this.scenarioContext.Set(
                new WebFactory<Startup, EndToEndDbContext>(
                    this.scenarioContext.Get<Configure<IServiceCollection>>,
                    this.scenarioContext.Get<Configure<IDbContext>>));
        }
    }
}