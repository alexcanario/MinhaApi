using DevIO.Api.Configuration;
using DevIO.Api.Extensions;
using DevIO.Data.Context;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DevIO.Api {
    public class Startup {
        private readonly string _connString;
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
            _connString = Configuration.GetConnectionString("DefaultConnection");
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddDbContext<MeuDbContext>(options => {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddIdentityConfiguration(Configuration);

            services.AddAutoMapper(typeof(Startup));

            services.WebApiConfig();
            
            services.ResolveDependencies();

            services.AddSwaggerConfig();

            services.AddHealthChecksConfig(Configuration);

            services.AddLoggingConfiguration();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider) {
            if (env.IsDevelopment()) {
                app.UseCors("Development"); // sempre antes do UseMvcConfiguration
                app.UseDeveloperExceptionPage();
            } 
            else 
            {
                app.UseHsts();
                app.UseCors("Productions"); // sempre antes do UseMvcConfiguration
            }

            app.UseAuthentication(); //Sempre antes da declação do MVC Configuration

            //Esse middleware habilita o elmah a capturar qualquer exception que ocorrra.
            //Sem essa middleware nAo seria possivel o elmah capturar um simples new Exception("Erro")
            app.UseMiddleware<ExceptionMiddleware>();

            app.UseMvcConfig();

            app.UseSwaggerConfig(provider);

            app.UseHealthChecksConfig();

            app.UseLoggingConfiguration();
        }
    }
}
