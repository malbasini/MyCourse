using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyCourse.Models.Options;
using MyCourse.Models.Services.Application;
using MyCourse.Models.Services.Infrastructure;
using Microsoft.Extensions.Caching.Memory;

namespace MyCourse
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;

        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCaching();
            services.AddMvc(options=>{
                var homeProfile = new CacheProfile();
                homeProfile.Duration = configuration.GetValue<int>("ResponseCache:Home:Duration");
                homeProfile.Location = configuration.GetValue<ResponseCacheLocation>("ResponseCache:Home:Location");
                homeProfile.VaryByQueryKeys = new string[]{"page"};
                //Configuration.Bind("ResponseCache:Home", homeProfile);
                options.CacheProfiles.Add("Home", homeProfile);
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddTransient<ICourseService, AdoNetCourseService>();
            //services.AddTransient<ICourseService, EFCoreCourseService>();
            services.AddTransient<IDatabaseAccessor, SqliteDatabaseAccessor>();
            services.AddTransient<ICachedCourseService,MemoryCacheCourseService>();
            //services.AddTransient<ICachedCourseService, DistributedCacheCourseService>();
            services.AddDbContextPool<MyCourseDbContext>(optionsBuilder =>
            {
                string connectionString = configuration.GetSection("ConnectionStrings").GetValue<string>("Default");
                optionsBuilder.UseSqlite(connectionString);
            });
            #region Configurazione del servizio di cache distribuita

            //Se vogliamo usare Redis, ecco le istruzioni per installarlo: https://docs.microsoft.com/it-it/aspnet/core/performance/caching/distributed?view=aspnetcore-2.2#distributed-redis-cache
            //Bisogna anche installare il pacchetto NuGet: Microsoft.Extensions.Caching.StackExchangeRedis
            //services.AddStackExchangeRedisCache(options =>
            //{
            //    Configuration.Bind("DistributedCache:Redis", options);
            //});
            
            //Se vogliamo usare Sql Server, ecco le istruzioni per preparare la tabella usata per la cache: https://docs.microsoft.com/it-it/aspnet/core/performance/caching/distributed?view=aspnetcore-2.2#distributed-sql-server-cache
            /*services.AddDistributedSqlServerCache(options => 
            {
                Configuration.Bind("DistributedCache:SqlServer", options);
            });*/

            //Se vogliamo usare la memoria, mentre siamo in sviluppo
            //services.AddDistributedMemoryCache();
            
            #endregion
            //Options
            services.Configure<TimeFromSecondExpireCacheOptions>(configuration.GetSection("TimeExpireCacheFromSecond"));
            services.Configure<ConnectionStringOptions>(configuration.GetSection("ConnectionStrings"));
            services.Configure<CoursesOptions>(configuration.GetSection("Courses"));
            services.Configure<MemoryCacheOptions>(configuration.GetSection("MemoryCache"));
           
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //if (env.IsDevelopment())
            if (env.IsEnvironment("Development"))
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }
            app.UseStaticFiles();
            app.UseResponseCaching();
            //app.UseMvcWithDefaultRoute();
            app.UseMvc(routeBuilder =>
            {
                // /courses/detail/5
                routeBuilder.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
