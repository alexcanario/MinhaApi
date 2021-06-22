using System;
using System.Text;
using DevIO.Api.Data;
using DevIO.Api.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace DevIO.Api.Configuration {
    public static class IdentityConfig {
        public static IServiceCollection AddIdentityConfiguration(this IServiceCollection services, IConfiguration configuration) {
            services.AddDbContext<ApplicationDbContext>(options => {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddDefaultIdentity<IdentityUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddErrorDescriber<IdentityMsgPortugues>(); //02.06.2021 Adiciona a classe com a tradução das mensagens do Identity

            //02.06.2021 - JWT
            //Obtemos as configurações para o token informadas no appsettings.json, na seção appsettings
            var appSettingsSection = configuration.GetSection("AppSettingsTokenData");
            services.Configure<AppSettingsToken>(appSettingsSection);

            var appSettings = appSettingsSection.Get<AppSettingsToken>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);

            //02.06.2021 Adiciona e configura a autenticação
            //Informar o asp.net.core que nossa aplicação funciona a base de token JWT
            services.AddAuthentication(x => {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x => {
                //Somente requisições https
                x.RequireHttpsMetadata = true;
                //Guardar o token apos uma autenticação com sucesso
                x.SaveToken = true;
                //Configurar as validações
                x.TokenValidationParameters = new TokenValidationParameters() {
                    //Validar o emissor do token baseado na chave
                    ValidateIssuerSigningKey = true,
                    //Configura a chave para validação
                    IssuerSigningKey = new SymmetricSecurityKey(key),

                    //Validar o emissor com base no emissor
                    ValidateIssuer = true, 
                    ValidIssuer = appSettings.Emissor,

                    //Validar a audiencia, url onde está sua aplicação
                    ValidateAudience = true, 
                    ValidAudience = appSettings.ValidoEm
                };
            });
            return services;
        }
    }
}