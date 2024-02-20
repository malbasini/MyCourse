using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace MyCourse
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options => options.EnableEndpointRouting = false);
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
                 Action è detail passando al controller il 5 come id.*/
                routeBuilder.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
