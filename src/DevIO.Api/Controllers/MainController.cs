using System;
using DevIO.Business.Intefaces;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using System.Linq;
using DevIO.Business.Notificacoes;

namespace DevIO.Api.Controllers {
    [ApiController]
    public abstract class MainController : ControllerBase {
        private readonly INotificador _notificador;
        public readonly IUser AppUser;

        public Guid UsuarioId => AppUser.GetUserId();
        public bool UsuarioAutenticado => AppUser.IsAuthenticated();

        protected MainController(INotificador notificador, IUser appUser) {
            _notificador = notificador;
            AppUser = appUser;
        }

        protected void NotificateErrorInvalidModelState(ModelStateDictionary modelState) {
            var errors = modelState.Values.SelectMany(m => m.Errors);

            foreach (var error in errors) {
                var errorMessage = error.Exception is null ? error.ErrorMessage : error.Exception.Message;
                NotifyError(errorMessage);
            }
        }

        protected void NotifyError(string message) {
            _notificador.Handle(new Notificacao(message));
        }

        protected ActionResult CustomResponse(ModelStateDictionary modelState) {
            if(!modelState.IsValid) NotificateErrorInvalidModelState(modelState);

            return CustomResponse();
        }

        protected ActionResult CustomResponse(object result = null) {
            if (ValidOperation()) {
                return Ok(new {
                    Success = true,
                    Data = result
                });
            }

            return BadRequest(new {
                Success = false,
                Errors = _notificador.ObterNotificacoes().Select(n => n.Mensagem)
            });
        }

        protected bool ValidOperation() {
            return (!_notificador.TemNotificacao());
        }
    }
}