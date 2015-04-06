using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AadAppRoleManager.Web.Modules;
using AadAppRoleManager.Web.Services;
using AadAppRoleManager.Web.Utilities;

namespace AadAppRoleManager.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly AadAccessTokenRepositoryFactory _accessTokenRepositoryFactory;
        private readonly IConfigurationSettings _configuration;

        public HomeController(
            AadAccessTokenRepositoryFactory accessTokenRepositoryFactory,
            IConfigurationSettings configuration)
        {
            _accessTokenRepositoryFactory = accessTokenRepositoryFactory;
            _configuration = configuration;
        }

        public async Task<ActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var tenantId = claimsIdentity.FindFirst(AadClaimTypes.TenantId).Value;
                var userObjectId = claimsIdentity.FindFirst(AadClaimTypes.ObjectId).Value;

                var tokenRepository = _accessTokenRepositoryFactory(tenantId, userObjectId);
                var accessToken = await tokenRepository.GetAuthenticationResultAsync();
                ViewBag.TokenExpiryUtc = accessToken.ExpiresOn.ToUniversalTime();
                ViewBag.TokenExpiryLocal = TimeZoneInfo.ConvertTime(accessToken.ExpiresOn, _configuration.LocalTimeZoneInfo);
            }

            return View();
        }
    }
}