﻿using ExternalDependencies.Calculator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MovieProject.Logic.Option;
using MovieProject.Logic.Proxy;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using System;
using System.Net.Http;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text.Json.Serialization;
using SystemTestingTools;
//using SystemTestingTools; // you only need this line if you are planning to use IServiceCollection.RecordHttpRequestsAndResponses()

namespace MovieProject.Web
{
    public class Startup
    {
        private readonly ILogger<Startup> _logger;
        private readonly IConfiguration _configuration;

        public static IEndpointBehavior wcfInterceptor = WcfHttpInterceptor.CreateRequestResponseRecorder("C:\\RecordedRequestAndResponses");

        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.Configure<Logic.Option.Caching>(_configuration.GetSection("caching"));
            AddHttpDependencies(services);

            services
                .Scan(scan => scan
                                .FromAssemblyOf<Logic.SearchService>()
                                .AddClasses()
                                .UsingRegistrationStrategy(Scrutor.RegistrationStrategy.Skip)
                                .AsImplementedInterfaces()
                                .WithScopedLifetime())
                .AddMvc()
                    .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddHealthChecks();
            services.RecordHttpClientRequestsAndResponses("C:\\RecordedRequestAndResponses");
        }

        private void AddHttpDependencies(IServiceCollection services)
        {
            services.Configure<Omdb>(_configuration.GetSection("Omdb"));
            var omdb = services.BuildServiceProvider().GetService<IOptions<Omdb>>();
            Logic.Constants.OmdbApiKey = omdb.Value?.ApiKey ?? throw new Exception("Omdb is not configured correctly in settings file");

            // setup copied from https://github.com/App-vNext/Polly/wiki/Polly-and-HttpClientFactory
            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutRejectedException>() // thrown by Polly's TimeoutPolicy if the inner call times out
                .RetryAsync();

            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromMilliseconds(800));  // Timeout for an individual try

            services
                .AddHttpClient<IMovieDatabaseProxy, MovieDatabaseProxy>()
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(timeoutPolicy) // We place the timeoutPolicy inside the retryPolicy, to make it time out each try
                .ConfigureHttpClient(c => {
                    c.BaseAddress = new Uri(omdb.Value.Url);
                    c.DefaultRequestHeaders.Add("Referer", Logic.Constants.Website);
                    c.Timeout = TimeSpan.FromMilliseconds(1500); // Overall timeout across all tries
                });

            services.Configure<User>(_configuration.GetSection("User"));
            var user = services.BuildServiceProvider().GetService<IOptions<User>>();

            services
                .AddHttpClient<IUserProxy, UserProxy>()
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(timeoutPolicy) // We place the timeoutPolicy inside the retryPolicy, to make it time out each try
                .ConfigureHttpClient(c => {
                    c.BaseAddress = new Uri(user.Value.Url);
                    c.DefaultRequestHeaders.Add("Referer", Logic.Constants.Website);
                    c.Timeout = TimeSpan.FromMilliseconds(1500); // Overall timeout across all tries
                });

            services.AddSingleton<ICalculatorSoap, CalculatorSoapClient>(factory => {
                var client = new CalculatorSoapClient(new CalculatorSoapClient.EndpointConfiguration());
                client.Endpoint.Address = new EndpointAddress(_configuration["Calculator:Url"]);
                if (wcfInterceptor != null) client.Endpoint.EndpointBehaviors.Add(wcfInterceptor);
                return client;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (!env.IsDevelopment())
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                app.UseHttpsRedirection();
            }
            
            app.UseMiddleware<ExceptionHandlerMiddleware>();
            app.UseHealthChecks("/healthcheck");
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/healthcheck");
                endpoints.MapControllers();
            });

            // useful for many purposes
            // - see cloud consumption and analyse bugs where application restarts unexpectely
            // - test logging is working correctly easily
            // -- check if component tests can intercept logs
            _logger.LogInformation("Application is starting"); 
        }
    }
}
