using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MovieProject.Logic.Proxy;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using System;
using System.Linq;
using System.Net.Http;

namespace MovieProject.Web
{
    public class Startup
    {
        private readonly ILogger<Startup> _logger;
        private readonly IConfiguration _configuration;

        //public static Func<DelegatingHandler> GlobalLastHandlerFactory = () => new SystemTestingTools.RequestResponseRecorder("C:\\temp");
        public static Func<DelegatingHandler> GlobalLastHandlerFactory = null;

        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private void AddHttpClient(IServiceCollection services)
        {
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
                .ConfigureHttpMessageHandlerBuilder((c) => {
                    if (GlobalLastHandlerFactory != null) c.AdditionalHandlers.Add(GlobalLastHandlerFactory());
                });

            services
                .AddHttpClient<IUserProxy, UserProxy>()
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(timeoutPolicy) // We place the timeoutPolicy inside the retryPolicy, to make it time out each try
                .ConfigureHttpMessageHandlerBuilder((c) => {
                    if (GlobalLastHandlerFactory != null) c.AdditionalHandlers.Add(GlobalLastHandlerFactory());
                });
        }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            services
                .Scan(scan => scan
                                .FromAssemblyOf<Logic.SearchService>()
                                .AddClasses()
                                .UsingRegistrationStrategy(Scrutor.RegistrationStrategy.Skip)
                                .AsImplementedInterfaces()
                                .WithScopedLifetime())
                .AddMvc()                
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddHealthChecks();

            services.Configure<Logic.Option.Caching>(_configuration.GetSection("caching"));
            services.Configure<Logic.Option.Omdb>(_configuration.GetSection("Omdb"));
            services.Configure<Logic.Option.User>(_configuration.GetSection("User"));

            AddHttpClient(services);
        }



        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (!env.IsDevelopment())
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                app.UseHttpsRedirection();
            }
            
            app.UseMiddleware<ExceptionHandlerMiddleware>();
            app.UseMvc();
            app.UseHealthChecks("/healthcheck");

            // useful for many purposes
            // - see cloud consumption and analyse bugs where application restarts unexpectely
            // - test logging is working correctly easily
            // -- check if system tests can intercept logs
            _logger.LogInformation("Application is starting"); 
        }
    }
}
