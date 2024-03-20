using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyCourse.Models.Services.Application;
using MyCourse.Models.Services.Infrastructure;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.Extensions.Configuration;
using MyCourse.Models.Options;
using MyCourse.Models.Enums;
namespace MyCourse
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options => options.EnableEndpointRouting = false);
            //Usiamo ADO.NET o Entity Framework Core per l'accesso ai dati?
            var persistence = Persistence.AdoNet;
            switch (persistence)
            {
                case Persistence.AdoNet:
                    services.AddTransient<ICourseService, AdoNetCourseService>();
                    services.AddTransient<IDatabaseAccessor, SqlliteDatabaseAccessor>();
                    break;
                case Persistence.EfCore:
                    services.AddTransient<ICourseService, EfCoreCourseService>();
                    // Usando AddDbContextPool, il DbContext verrà implicitamente registrato con il ciclo di vita Scoped
                    services.AddDbContextPool<MyCourseDbContext>(optionsBuilder =>
                    {
                        string connectionString = Configuration.GetSection("ConnectionStrings").GetValue<string>("Default");
                        optionsBuilder.UseSqlite(connectionString);
                    });
                    break;
            }
            //Options
            services.Configure<ConnectionStringsOptions>(Configuration.GetSection("ConnectionStrings"));
            services.Configure<CoursesOptions>(Configuration.GetSection("Courses"));
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
            //Questo è già sufficente tuttavia usiamo
            //un metodo che mostra un Default Route.
            //app.UseMvcWithDefaultRoute()
            app.UseMvc(routeBuilder =>
            {
                //Sto definendo una route
                /*Abbiamo definito una route con tre frammenti,
                 controller,action e id. Grazie a questa il 
                 Middleware di routing è in grado di estrarre
                 delle informazioni dalla richiesta. Supponiamo
                 che arrivi la seguente richiesta:
                 
                 /courses/detail/5
                 
                 Il Middleware di routing dovrà andarsi a trovare
                 una richiesta il cui Controller è courses, la sua
                 Action è detail passando al controller il 5 come id.
                 Per non avere un errore HTTP 500 possiamo impostare
                 dei valori di default. Creiamo l'HomeController e la
                 sua action Index, la route può essere configurata 
                 assegnando valori opzionali per il controller, e l'action.
                 il ? sull'ultimo frammento, l'id indica che è opzionale.*/
                routeBuilder.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
