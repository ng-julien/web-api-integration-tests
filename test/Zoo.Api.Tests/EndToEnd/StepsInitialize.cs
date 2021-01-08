namespace Zoo.Api.Tests.EndToEnd
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;

    using Core;

    using Infrastructure.Store;

    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.Extensions.DependencyInjection;

    using Newtonsoft.Json.Linq;

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
            this.scenarioContext.Get<Assertion>()(webFactory.Server.Services.CreateScope().ServiceProvider);
            webFactory.Dispose();
        }

        [Given(@"I'm (.*)")]
        public void Im(string role)
        {
            var claims = this.scenarioContext.Get<List<Claim>>();
            claims.Add(new Claim(ClaimTypes.Role, role));
            this.scenarioContext.Set(claims);
        }
        
        [When(@"i call the http resource '(.*)' with (.*) http method")]
        public async Task WhenICallTheHttpResourceWithHttpMethod(string resource, HttpMethod httpMethod)
        {
            this.scenarioContext.TryGetValue("Post", out var postingValue);
            HttpContent httpContent = null;
            if (postingValue != null)
            {
                httpContent = new StringContent(
                    JObject.FromObject(postingValue).ToString(),
                    Encoding.UTF8,
                    "application/json");
            }

            var webFactory = this.scenarioContext.Get<WebFactory<Startup, EndToEndDbContext>>();
            using var client = webFactory.CreateClient();
            var token = webFactory.GetBearerToken(this.scenarioContext.Get<List<Claim>>());
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Add("Authorization", $"{JwtBearerDefaults.AuthenticationScheme} {token}");
            }
            
            var response = await client.SendAsync(
                               new HttpRequestMessage(httpMethod, $"{resource}")
                                   {
                                       Content = httpContent
                                   });
            this.scenarioContext.Set(response);
        }
        
        [BeforeScenario]
        public void BeforeScenario()
        {
            this.scenarioContext.Set(new List<Claim>());
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