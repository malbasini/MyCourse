using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;
using MyCourse.Models.Services.Applications;

namespace MyCourse
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options => options.EnableEndpointRouting = false);
            /*--Con questa istruzione stiamo dicendo ai servizi di ASP.NET Core che
                devono prepararsi alla gestione di oggetti di tipo ICourseService,
                quando si incontra un componente come il nostro controller che ha una
                dipendenza da ICourseService deve costruirlo e 
                passarlo al controller. Grazie alla dependency injection ASP.NET Core
                può creare istanze di componenti e iniettarle in questo caso nel
                controller.
            */
            services.AddTransient<ICourseService,CourseService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //if (env.IsDevelopment())
            if (env.IsEnvironment("Development"))
            {
                app.UseDeveloperExceptionPage();
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
