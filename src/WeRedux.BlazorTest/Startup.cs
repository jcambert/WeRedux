using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WeReduxBlazor;
using WeRedux.BlazorTest.Data;
using Microsoft.Extensions.Logging;

namespace WeRedux.BlazorTest
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSingleton<WeatherForecastService>();
            services.AddStore<CounterState, IAction>("Counter 1").AddStore<CounterState2, IAction>("Counter 2");

            services.AddStorage();
            services.AddScoped<IRedux<CounterState, IAction>, Redux<CounterState, IAction>>(services =>
             {
                 var store = services.GetRequiredService<IStore<CounterState, IAction>>();
                 var storage = services.GetRequiredService<LocalStorage>();
                 //var js = services.GetRequiredService<IJSRuntime>();
                 var logger = services.GetRequiredService<ILogger<Redux<CounterState, IAction>>>();
                 var redux = new Redux<CounterState, IAction>(store, storage, logger, "Counter 1");

                 return redux;
             });
            services.AddScoped<IRedux<CounterState2, IAction>, Redux<CounterState2, IAction>>(services =>
            {
                var store = services.GetRequiredService<IStore<CounterState2, IAction>>();
                var storage = services.GetRequiredService<LocalStorage>();
                //var js = services.GetRequiredService<IJSRuntime>();
                var logger = services.GetRequiredService<ILogger<Redux<CounterState2, IAction>>>();
                var redux = new Redux<CounterState2, IAction>(store, storage, logger, "Counter 2");

                return redux;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
