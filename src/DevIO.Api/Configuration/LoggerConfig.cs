using Elmah.Io.AspNetCore;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using System;
using Elmah.Io.Extensions.Logging;
using Microsoft.Extensions.Logging;

namespace DevIO.Api.Configuration {
    public static class LoggerConfig {
        public static IServiceCollection AddLoggingConfiguration(this IServiceCollection services) {
            //Aqui os Elmah captura as excessões e os erros
            services.AddElmahIo(o =>
            {
                o.ApiKey = "644dc00b44d643b191ee7d7ce47c4a69";
                o.LogId = new Guid("b7389637-e4d4-4fb4-a265-45125727de8b");
            });

            //Repete as informações para que Elmah possa capturar os nossos logs
            //services.AddLogging(buider => {
            //    buider.AddElmahIo(o => {
            //        o.ApiKey = "644dc00b44d643b191ee7d7ce47c4a69";
            //        o.LogId = new Guid("b7389637-e4d4-4fb4-a265-45125727de8b");
            //    });
            //    buider.AddFilter<ElmahIoLoggerProvider>(null, LogLevel.Warning);
            //});

            return services;
        }

        public static IApplicationBuilder UseLoggingConfiguration(this IApplicationBuilder app) {
            app.UseElmahIo();

            return app;
        }
        
    }
}