using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace DevIO.Api.Configuration {
    public static class ApiConfig {

        public static IServiceCollection WebApiConfig(this IServiceCollection services) {
            //Versionamento da API
            services.AddApiVersioning(opt => {
                opt.ReportApiVersions = true; //Quando a api for consumida, será reportado no
                //header que a versão esta obsoleta e que uma nova está disponível
                opt.AssumeDefaultVersionWhenUnspecified = true; //Assume a versão padrao quando nao for especificado
                opt.DefaultApiVersion = new ApiVersion(1, 0);
            });

            services.AddVersionedApiExplorer(opt => {
                opt.GroupNameFormat = "'v'VVV";
                opt.SubstituteApiVersionInUrl = true;
            });
            services.Configure<ApiBehaviorOptions>(options => {
                options.SuppressModelStateInvalidFilter = true;
            });

            //Necessario para consumir a API pelo Angular
            //Facilita o acesso a outras origens
            // O Cors é implementado pelo browser
            services.AddCors(option => {
                option.AddPolicy("Development",
                    builder => builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());

                option.AddPolicy("Production",
                    builder => builder
                        .WithMethods("GET")
                        .WithOrigins("http://desenvolvedor.io")
                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                        .WithHeaders(HeaderNames.ContentType, "x-custom-header")
                        .AllowAnyHeader());
                        //Nessa politica o Cors libera o verbo get para http://desenvolvedor.io
                        //Outros enderecos não terao nenhuma permissão, porem o Cors so funciona para browsers


            });

            services.AddControllers();
            services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Trace));

            return services;
        }

        public static IApplicationBuilder UseMvcConfig(this IApplicationBuilder app) {
            app.UseHttpsRedirection();

            //Transferido para o startup.cs
            //app.UseCors("Development");

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });

            return app;
        }
    }
}
