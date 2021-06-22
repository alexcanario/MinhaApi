using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace DevIO.Api.Configuration {
    //OK Revisado
    public static class SwaggerConfig {
        //OK Revisado
        public static IServiceCollection AddSwaggerConfig(this IServiceCollection services) {
            services.AddSwaggerGen(c => {
                c.OperationFilter<SwaggerDefaultValues>();

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme() {
                    Description = "Insira o token JWT - Jason Web Token deta maneira: Bearer {seu token}",
                    Name = "Authorization",
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });

                
                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme {
                            Reference = new OpenApiReference {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        }, 
                        new string[]{}
                    }
                });
            });

            return services;
        }

        public static IApplicationBuilder UseSwaggerConfig(this IApplicationBuilder app, IApiVersionDescriptionProvider provider) {
            //Os middlewares sao chamados na ordem em que são declarados, portando devemos colocar essa declaração de segurança antes de chamar o swagger
            //Usar o middleware de segurança para permitir acesso ao swagger, usuários autenticados
            //app.UseMiddleware<SwaggerAuthorizedMiddleware>();

            app.UseSwagger();
            app.UseSwaggerUI(opt => {
                foreach (var description in provider.ApiVersionDescriptions) {
                    opt.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                }
            });

            
            
            return app;
        }
    }

    //OK Revisada
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions> {
        private readonly IApiVersionDescriptionProvider _provider;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) => _provider = provider;

        public void Configure(SwaggerGenOptions options) {
            foreach (var description in _provider.ApiVersionDescriptions) {
                options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
            }
        }

        private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description) {
            var info = new OpenApiInfo() {
                Title = "API - desenvolvedor.io",
                Version = description.ApiVersion.ToString(),
                Description = "Esta APi faz parte do curso REST com ASP.NET Core WebAPI",
                Contact = new OpenApiContact() { Name = "Alex Canario", Email = "alexcanario@gmail.com" },
                License = new OpenApiLicense() { Name = "MIT", Url = new Uri("https://opensource.org/licenses/MIT") },
                TermsOfService = new Uri("https://opensource.org/licenses/MIT")
            };

            if (description.IsDeprecated) {
                info.Description += " - Esta versão está obsoleta!";
            }

            return info;
        }
    }

    //OK Revisada
    public class SwaggerDefaultValues : IOperationFilter {
        public void Apply(OpenApiOperation operation, OperationFilterContext context) {
            if (operation.Parameters is null) {
                return;
            }

            foreach (var parameter in operation.Parameters) {
                var description =
                    context.ApiDescription.ParameterDescriptions.First(p => p.Name.Equals(parameter.Name));

                var routeInfo = description.RouteInfo;

                operation.Deprecated = OpenApiOperation.DeprecatedDefault;

                parameter.Description ??= description.ModelMetadata?.Description;

                if (routeInfo is null) {
                    continue;
                }

                if (parameter.In != ParameterLocation.Path && parameter.Schema.Default == null) {
                    parameter.Schema.Default = new OpenApiString(routeInfo.DefaultValue.ToString());
                }

                parameter.Required |= !routeInfo.IsOptional;
            }
        }
    }

    //OK Revisada
    public class SwaggerAuthorizedMiddleware {
        private readonly RequestDelegate _next;

        public SwaggerAuthorizedMiddleware(RequestDelegate next) {
            _next = next;
        }

        public async Task Invoke(HttpContext context) {
            if (context.Request.Path.StartsWithSegments("/swagger") && !context.User.Identity.IsAuthenticated) {
                //context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.Redirect("/api/v1/entrar");
                return;
            }

            await _next.Invoke(context);
        }
    }
}