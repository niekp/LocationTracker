using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Storage.SQLite;
using LocationTracker.Data;
using LocationTracker.Jobs;
using LocationTracker.Repositories.Core;
using LocationTracker.Repositories.Persistence;
using LocationTracker.Utils;
using LocationTracker.Utils.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LocationTracker
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.KnownProxies.Add(IPAddress.Parse("127.0.0.1"));
                options.KnownProxies.Add(IPAddress.Parse("51.68.123.74"));
            });

            // Databases
            services.AddDbContext<LocationContext>(options => options.UseMySQL(Configuration.GetConnectionString("DefaultConnection")));

            // Tables
            services.AddTransient<IPingRepository, PingRepository>();
            services.AddTransient<ILocationRepository, LocationRepository>();
            services.AddTransient<IRideRepository, RideRepository>();
            services.AddTransient<IDayRepository, DayRepository>();
            services.AddTransient<ITagRepository, TagRepository>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IUserSessionRepository, UserSessionRepository>();
            services.AddTransient<INoteRepository, NoteRepository>();

            // Helper repos
            services.AddTransient<IStatsRepository, StatsRepository>();
            services.AddTransient<IRunRepository, RunRepository>();

            // Cache
            services.AddSession(options => {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddTransient<ICache, Cache>();

            // Hangfire
            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSQLiteStorage());

            // Add the processing server as IHostedService
            services.AddHangfireServer();

            // App settings
            services.Configure<AppSettings>(Configuration);

            // Authorization
            services.AddAuthentication(a => {
                a.DefaultAuthenticateScheme = "Cookie";
                a.DefaultChallengeScheme = "Cookie";
            }).AddCookieAuth(o => { });

            // MVC
            services.AddControllersWithViews();
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
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseSession();

            app.UseStatusCodePages(async context => {
                if (context.HttpContext.Response.StatusCode == 401
                    && !context.HttpContext.Request.Path.Value.StartsWith("/api"))
                {
                    context.HttpContext.Response.Redirect("/login");
                }
            });

            app.UseRouting();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorization() }
            });

            RecurringJob.AddOrUpdate<ProcessPings>("ProcessPings", x => x.Process(), Cron.Daily);
            RecurringJob.AddOrUpdate<HistoryMap>("DrawMap", x => x.DrawMap(), Cron.Daily);

#if !DEBUG
            BackgroundJob.Enqueue<HistoryMap>(x => x.DrawMap());
#endif

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
