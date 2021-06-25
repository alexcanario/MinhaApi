using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using DevIO.Api.Extensions;
using Elmah.Io.AspNetCore.HealthChecks;
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
        //Install-Package Elmah.Io.AspNetCore.HealthChecks
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
                    tags: new List<string>() {"db", "sql", "sqlserver"})
                .AddCheck("Produtos", new SqlServerHealthCheck(configuration["ConnectionStrings:DefaultConnection"]))
                .AddElmahIoPublisher(opt => {
                    opt.ApiKey = LoggerConfig.ApiKey;
                    opt.LogId = new Guid("b7389637-e4d4-4fb4-a265-45125727de8b");
                    opt.HeartbeatId = "8f8aef187b2a4e0c851f6d1102b0a7cf";
                    opt.Application = "Minha Api - HealthCheak do Banco";
                });

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

    /*
        INSTRUÇÕES PARA ADICIONAR O HEALTHCHECKS NO ELMAH.IO

        Your applications/services need to send a POST request to the elmah.io API in the interval 
            configured on each heartbeat. When setting up the heartbeat you will need the following properties:
                Log ID:
                Heartbeat ID:

        For detailed instructions to how to do so from different tools and frameworks, select one of 
            the options below. For the full documentation, visit the Documentation site.
        C# cURL PowerShell ASP.NET Core More

        Install the Elmah.Io.Client NuGet package:

        Install-Package Elmah.Io.Client

        Create an elmah.io client and keep the instance:

        var apiKey = "API_KEY";
        var elmahIo = ElmahioAPI.Create(apiKey);

        Replace API_KEY with an API key with the Heartbeats - Write permission enabled (Where is my API key?).

        Create a new heartbeat using the client:

        var logId = new Guid("b7389637-e4d4-4fb4-a265-45125727de8b");
        var heartbeatId = "8f8aef187b2a4e0c851f6d1102b0a7cf";
        elmahIo.Heartbeats.Healthy(logId, heartbeatId);

        There are methods available for logging a degraded or unhealthy heartbeat too: Degraded(logId, heartbeatId) and Unhealthy(logId, heartbeatId).
     */
}