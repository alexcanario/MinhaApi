using DevIO.Api.Controllers;
using DevIO.Api.Extensions;
using DevIO.Api.ViewModels;
using DevIO.Business.Intefaces;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DevIO.Api.V1 {

    [ApiVersion("2.0")]
    [ApiVersion("1.0", Deprecated = true)]
    [Route("api")]
    [Route("api/v{version:apiVersion}")]
    //[DisableCors] //Desabilita as permissões ou facilidades do Cors para essa controller
    public class AuthController : MainController {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppSettingsToken _appSettingsToken;
        private readonly ILogger _logger;
        public AuthController(INotificador notificador, 
                                SignInManager<IdentityUser> signInManager, 
                                UserManager<IdentityUser> userManager, 
                                IOptions<AppSettingsToken> appSettingsToken,
                                IUser user, ILogger<AuthController> logger) : base(notificador, user) {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _appSettingsToken = appSettingsToken.Value;
        }

        [Route("registrar")]
        [HttpPost]
        //[EnableCors("Developement")] //Habilito as facilidades do Cors para esse verbo,
        //porem não sebescreve a politica global informado no starup para Devlopment ou Production
        public async Task<ActionResult> RegistrarUsuario(RegisterUserViewModel registerUser) {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var user = new IdentityUser() {
                UserName = registerUser.Email, 
                Email = registerUser.Email, 
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, registerUser.Password);

            if (result.Succeeded) {
                await _signInManager.SignInAsync(user, false);
                return CustomResponse(await GerarJwt(user.Email));
            }

            foreach (var erro in result.Errors) {
                NotifyError($"Erro {erro.Code} | {erro.Description}");
            }

            return CustomResponse(registerUser);
        }

        
        [Route("entrar")]
        [HttpPost]
        public async Task<ActionResult> Entrar(LoginUserViewModel loginUser) {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var result = await _signInManager.PasswordSignInAsync(loginUser.Email, loginUser.Password, false, true);

            if (result.Succeeded) {
                _logger.LogInformation($"{DateTime.Now} | O usuário: {loginUser.Email}, logou com sucesso");
                return CustomResponse(await GerarJwt(loginUser.Email));
            }

            if (result.IsLockedOut) {
                _logger.LogInformation($"{DateTime.Now} | O usuário: { AppUser.Name }, foi bloqueado por tentativas de logon inválidas");
                NotifyError("Usuário bloqueado por tentativas inválidas");
                CustomResponse(loginUser);
            }

            NotifyError("Usuario e ou Senha inválidos");

            return CustomResponse(loginUser);
        }

        //Modo simples
        private string GerarJwt() {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettingsToken.Secret);
            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor {
                Issuer = _appSettingsToken.Emissor,
                Audience = _appSettingsToken.ValidoEm,
                Expires = DateTime.UtcNow.AddHours(_appSettingsToken.ExpiracaoHoras),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            });

            var encodedToken = tokenHandler.WriteToken(token);
            return encodedToken;
        }

        //private async Task<string> GerarJwt(string email) {
        //    var user = await _userManager.FindByEmailAsync(email);
        //    var claims = await _userManager.GetClaimsAsync(user);
        //    var userHoles = await _userManager.GetRolesAsync(user);

        //    claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id));
        //    claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
        //    claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
        //    claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, ToUnixEpochDate(DateTime.UtcNow).ToString()));
        //    claims.Add(new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64));

        //    foreach (var userHole in userHoles) {
        //        claims.Add(new Claim("hole", userHole));
        //    }

        //    var identityClaims = new ClaimsIdentity();
        //    identityClaims.AddClaims(claims);

        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var key = Encoding.ASCII.GetBytes(_appSettingsToken.Secret);
        //    var token = tokenHandler.CreateToken(new SecurityTokenDescriptor {
        //        Issuer = _appSettingsToken.Emissor,
        //        Audience = _appSettingsToken.ValidoEm,
        //        Expires = DateTime.UtcNow.AddHours(_appSettingsToken.ExpiracaoHoras),
        //        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
        //        Subject = identityClaims
        //    });

        //    var encodedToken = tokenHandler.WriteToken(token);
        //    return encodedToken;
        //}

        private async Task<LoginResponseViewModel> GerarJwt(string email) {
            var user = await _userManager.FindByEmailAsync(email);
            var claims = await _userManager.GetClaimsAsync(user);
            var userHoles = await _userManager.GetRolesAsync(user);

            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id));
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, ToUnixEpochDate(DateTime.UtcNow).ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64));

            foreach (var userHole in userHoles) {
                claims.Add(new Claim("hole", userHole));
            }

            var identityClaims = new ClaimsIdentity();
            identityClaims.AddClaims(claims);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettingsToken.Secret);
            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor {
                Issuer = _appSettingsToken.Emissor,
                Audience = _appSettingsToken.ValidoEm,
                Expires = DateTime.UtcNow.AddHours(_appSettingsToken.ExpiracaoHoras),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Subject = identityClaims
            });

            var encodedToken = tokenHandler.WriteToken(token);
            var response = new LoginResponseViewModel {
                AccessToken = encodedToken,
                ExpiresIn = TimeSpan.FromHours(_appSettingsToken.ExpiracaoHoras).TotalSeconds,
                UserToken = new UserTokenViewModel() {
                    Id = user.Id,
                    Email = user.Email,
                    Claims = claims.Select(c => new ClaimViewModel() {Type = c.Type, Value = c.Value})
                }
            };

            return response;
        }

        private static long ToUnixEpochDate(DateTime date) 
            => (long) Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
    }
}