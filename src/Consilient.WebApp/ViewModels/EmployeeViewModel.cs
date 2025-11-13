using Consilient.WebApp.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Consilient.WebApp.ViewModels
{
    public class EmployeeViewModel
    {
        public int EmployeeId { get; init; }

        [Display(Name = "First Name")]
        public string? FirstName { get; init; }

        [Display(Name = "Last Name")]
        public string? LastName { get; init; }

        [Display(Name = "Title Extension")]
        public string? TitleExtension { get; init; }

        [Display(Name = "Is a Provider")]
        public bool IsProvider { get; init; }

        [Display(Name = "Role")]
        public string? Role { get; init; }

        [Display(Name = "Full Name")]
        public string? FullName { get; init; }

        [Display(Name = "Is an Admin")]
        public bool IsAdministrator { get; init; }

        public string? Email { get; init; }

        [Display(Name = "Can Approve Visits")]
        public bool CanApproveVisits { get; init; }

        public SelectList RolesSelectList { get; } = SelectListHelpers.GetRolesSelectList();
    }
}
