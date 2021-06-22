﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using System.Linq;
using System.Security.Claims;

namespace DevIO.Api.Extensions {
    public class CustomAuthorization {
        public static bool ValidarClaimsUsuario(HttpContext context, string claimName, string claimValue) {
            if (context.User.Identity != null && !context.User.Identity.IsAuthenticated) return false;

            return context.User.Claims.Any(c => c.Type.Equals(claimName) && c.Value.Contains(claimValue));
        }
    }

    public class ClaimsAuthorizeAttribute : TypeFilterAttribute {
        public ClaimsAuthorizeAttribute(string claimName, string claimValue) : base(typeof(RequisitoClaimFilter)) {
            Arguments = new object[] {new Claim(claimName, claimValue)};
        }
    }

    public class RequisitoClaimFilter : IAuthorizationFilter {
        private readonly Claim _claim;

        public RequisitoClaimFilter(Claim claim) {
            _claim = claim;
        }

        public void OnAuthorization(AuthorizationFilterContext context) {
            if (context.HttpContext.User.Identity != null && !context.HttpContext.User.Identity.IsAuthenticated) {
                context.Result = new StatusCodeResult(401);
                return;
            }

            if (CustomAuthorization.ValidarClaimsUsuario(context.HttpContext, _claim.Type, _claim.Value)) return;
            context.Result = new StatusCodeResult(403);
            return;
        }
    }
}