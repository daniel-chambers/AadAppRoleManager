using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using AadAppRoleManager.Web.Models;
using AadAppRoleManager.Web.Modules;
using AadAppRoleManager.Web.Services;
using AadAppRoleManager.Web.Utilities;
using Microsoft.Azure.ActiveDirectory.GraphClient;

namespace AadAppRoleManager.Web.Controllers
{
    [Authorize]
    public class ApplicationController : Controller
    {
        private readonly ActiveDirectoryClientFactory _activeDirectoryClientFactory;
        private readonly AadAccessTokenRepositoryFactory _aadAccessTokenRepositoryFactory;
        private IActiveDirectoryClient _client;
        private IAadAccessTokenRepository _aadAccessTokenRepository;

        public ApplicationController(
            ActiveDirectoryClientFactory activeDirectoryClientFactory,
            AadAccessTokenRepositoryFactory aadAccessTokenRepositoryFactory)
        {
            _activeDirectoryClientFactory = activeDirectoryClientFactory;
            _aadAccessTokenRepositoryFactory = aadAccessTokenRepositoryFactory;
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var tenantId = claimsIdentity.FindFirst(AadClaimTypes.TenantId).Value;
            var userObjectId = claimsIdentity.FindFirst(AadClaimTypes.ObjectId).Value;

            _client = _activeDirectoryClientFactory(tenantId, userObjectId);
            _aadAccessTokenRepository = _aadAccessTokenRepositoryFactory(tenantId, userObjectId);
        }

        public async Task<ActionResult> Index()
        {
            var pagedCollection = await _client.Applications.ExecuteAsync();
            var applications = await pagedCollection.EnumerateAllAsync();
            return View(applications);
        }

        public async Task<ActionResult> Application(string applicationId)
        {
            var application = await _client.Applications.GetByObjectId(applicationId).ExecuteAsync();

            return View(application);
        }

        public async Task<ActionResult> AppRole(string applicationId, string appRoleId)
        {
            var appRoleGuid = Guid.Parse(appRoleId);

            var application = await _client.Applications.GetByObjectId(applicationId).ExecuteAsync();
            var appId = application.AppId;
            var servicePrincipal = await _client.ServicePrincipals.Where(p => p.AppId == appId).ExecuteSingleAsync();

            var appRoleAssignments = (await (await _client.ServicePrincipals
                .GetByObjectId(servicePrincipal.ObjectId)
                .AppRoleAssignedTo
                .ExecuteAsync())
                .EnumerateAllAsync())
                .Where(a => a.Id == appRoleGuid)
                .ToList();

            var viewModel = new AppRoleViewModel
            {
                Application = application,
                AppRole = application.AppRoles.First(a => a.Id == appRoleGuid),
                ServicePrincipal = servicePrincipal,
                AppRoleAssignments = appRoleAssignments,
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<ActionResult> CreateAssignment(string applicationId, string appRoleId)
        {
            var appRoleGuid = Guid.Parse(appRoleId);

            var application = await _client.Applications.GetByObjectId(applicationId).ExecuteAsync();

            var viewModel = new CreateAssignmentViewModel
            {
                Application = application,
                AppRole = application.AppRoles.First(a => a.Id == appRoleGuid),
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> CreateAssignment(string applicationId, string appRoleId, CreateAssignmentViewModel viewModel)
        {
            var appRoleGuid = Guid.Parse(appRoleId);

            var application = await _client.Applications.GetByObjectId(applicationId).ExecuteAsync();

            viewModel.Application = application;
            viewModel.AppRole = application.AppRoles.First(a => a.Id == appRoleGuid);

            if (ModelState.IsValid == false)
                return View();

            var appId = application.AppId;
            var servicePrincipal = await _client.ServicePrincipals.Where(p => p.AppId == appId).ExecuteSingleAsync();

            var appRoleAssignment = new AppRoleAssignment
            {
                Id = appRoleGuid,
                ResourceId = Guid.Parse(servicePrincipal.ObjectId),
                PrincipalType = viewModel.AssignmentType.ToString(),
                PrincipalId = viewModel.ObjectId,
            };

            if (viewModel.AssignmentType == AssignmentType.Group)
            {
                await _client.Groups
                    .GetByObjectId(viewModel.ObjectId.ToString())
                    .AppRoleAssignments
                    .AddAppRoleAssignmentAsync(appRoleAssignment);
            }
            else
            {
                await _client.Users
                    .GetByObjectId(viewModel.ObjectId.ToString())
                    .AppRoleAssignments
                    .AddAppRoleAssignmentAsync(appRoleAssignment);
            }

            return RedirectToAction("AppRole", new { applicationId, appRoleId });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteAssignment(string applicationId, string appRoleId, string assignmentId)
        {
            var application = await _client.Applications.GetByObjectId(applicationId).ExecuteAsync();

            var appId = application.AppId;
            var servicePrincipal = await _client.ServicePrincipals.Where(p => p.AppId == appId).ExecuteSingleAsync();

            var assignment = await _client.ServicePrincipals.GetByObjectId(servicePrincipal.ObjectId).AppRoleAssignedTo
                .Where(a => a.ObjectId == assignmentId)
                .ExecuteSingleAsync();

            //Can't figure out how to delete stuff using the API, so we're doing it LIVE, cough, manually.
            var token = await _aadAccessTokenRepository.GetAccessTokenAsync();
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var tenantId = claimsIdentity.FindFirst(AadClaimTypes.TenantId).Value;

            if (assignment.PrincipalType == "Group")
            {
                await httpClient.DeleteAsync(string.Format("https://graph.windows.net/{0}/groups/{1}/appRoleAssignments/{2}?api-version=1.5", tenantId, assignment.PrincipalId, assignment.ObjectId));
            }
            else
            {
                await httpClient.DeleteAsync(string.Format("https://graph.windows.net/{0}/users/{1}/appRoleAssignments/{2}?api-version=1.5", tenantId, assignment.PrincipalId, assignment.ObjectId));
            }

            //No this doesn't work
            //await assignment.DeleteAsync();

            return RedirectToAction("AppRole", new { applicationId, appRoleId });
        }
    }
}