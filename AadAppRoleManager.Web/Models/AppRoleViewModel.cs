using System.Collections.Generic;
using Microsoft.Azure.ActiveDirectory.GraphClient;

namespace AadAppRoleManager.Web.Models
{
    public class AppRoleViewModel
    {
        public IApplication Application { get; set; }
        public IServicePrincipal ServicePrincipal { get; set; }
        public List<IAppRoleAssignment> AppRoleAssignments { get; set; }
        public AppRole AppRole { get; set; }
    }
}