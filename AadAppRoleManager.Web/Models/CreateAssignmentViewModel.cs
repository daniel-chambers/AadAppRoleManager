using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.ActiveDirectory.GraphClient;

namespace AadAppRoleManager.Web.Models
{
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
}