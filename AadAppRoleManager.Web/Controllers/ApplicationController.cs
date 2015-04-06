using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using AadAppRoleManager.Web.Models;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AadAppRoleManager.Web.Controllers
{
    [Authorize]
    public class ApplicationController : Controller
    {
        private const string GraphResourceId = "https://graph.windows.net";
        private IActiveDirectoryClient _client;

        public ApplicationController()
        {
            
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var tenantId = claimsIdentity.FindFirst(AadClaimTypes.TenantId).Value;

            _client = new ActiveDirectoryClient(
                new Uri(GraphResourceId + "/" + tenantId),
                GetAccessTokenAsync);
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
            var token = await GetAccessTokenAsync();
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

        private async Task<string> GetAccessTokenAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var tenantId = claimsIdentity.FindFirst(AadClaimTypes.TenantId).Value;
            var userObjectId = claimsIdentity.FindFirst(AadClaimTypes.ObjectId).Value;

            var clientCreds = new ClientCredential(ConfigurationManager.AppSettings["ida:ClientId"], ConfigurationManager.AppSettings["ida:AppKey"]);
            var authContext = new AuthenticationContext(string.Format("https://login.microsoftonline.com/{0}", tenantId), new TableStorageTokenCache(tenantId, userObjectId));
            var result = await authContext.AcquireTokenSilentAsync(GraphResourceId, clientCreds, new UserIdentifier(userObjectId, UserIdentifierType.UniqueId));
            return result.AccessToken;
        }
    }

    public class AppRoleViewModel
    {
        public IApplication Application { get; set; }
        public IServicePrincipal ServicePrincipal { get; set; }
        public List<IAppRoleAssignment> AppRoleAssignments { get; set; }
        public AppRole AppRole { get; set; }
    }

    public class CreateAssignmentViewModel
    {
        public IApplication Application { get; set; }
        public AppRole AppRole { get; set; }

        [Required]
        [Display(Name = "Type")]
        public AssignmentType AssignmentType { get; set; }

        [Required]
        [Display(Name = "Object ID")]
        public Guid? ObjectId { get; set; }
    }

    public enum AssignmentType
    {
        Group,
        User
    }
}