using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using DevIO.Api.Extensions;
using HealthChecks.UI.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DevIO.Api.Configuration {
    public static class HealthCheckConfig {
        //Pacote do HealthCheck para monitor diversos itens como, sqlserver, network, redis, dentre muitos outros
        //Esse pacote foi instalado
        //Install-Package AspNetCore.HealthChecks.SqlServer
        //Install-Package Elmah.Io.AspnetCore.HealthChecks
        //Install-Package AspNetCore.HealthChecks.UI
        //Install-Package AspNetCore.HealthChecks.UI.Client
        private static readonly Uri pacote1 = new Uri("https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks");
        
        public static IServiceCollection AddHealthChecksConfig(this IServiceCollection services, IConfiguration configuration) {
            //var strConn = configuration.GetConnectionString("DefaultConnection");
            //var strConn2 = configuration["Data:ConnectionStrings:Sample"];
            //var strConn3 = configuration["ConnectionStrings:DefaultConnection"];
            services.AddHealthChecks()
                .AddSqlServer(connectionString: configuration["ConnectionStrings:DefaultConnection"],
                    healthQuery: "select 1",
                    name: "BancoSQL",
                    failureStatus: HealthStatus.Degraded,
                    tags: new List<string>() { "db", "sql", "sqlserver" })
                .AddCheck("Produtos", new SqlServerHealthCheck(configuration["ConnectionStrings:DefaultConnection"]));

            services.AddHealthChecksUI()
                .AddInMemoryStorage();

            return services;
        }

        public static IApplicationBuilder UseHealthChecksConfig(this IApplicationBuilder app) {
            app.UseHealthChecks("/api/hc", new HealthCheckOptions {Predicate = _ => true})
                .UseHealthChecks("/api/hcz", new HealthCheckOptions {Predicate = _ => true, ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse })
                //.UseHealthChecksUI(config => config.UIPath="/api/hc-ui")
                .UseRouting()
                .UseEndpoints(config => {
                    //config.MapHealthChecksUI(); mapea o painel de health para o endereco abaixo
                    //https://localhost:5001/healthchecks-ui
                    config.MapHealthChecksUI();
                });

            return app;
        }
    }
}