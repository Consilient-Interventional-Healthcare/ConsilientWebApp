using Consilient.Constants;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;

namespace Consilient.WebApp.Helpers
{
    internal static class SelectListHelpers
    {
        public static SelectList GetRolesSelectList()
        {
            var roles = typeof(ApplicationConstants.Roles)
                .GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Select(p => p.GetValue(null)?.ToString())
                .Where(s => !string.IsNullOrEmpty(s))
                .Select(r => new SelectListItem(r, r))
                .ToList();

            return new SelectList(roles, "Value", "Text");
        }
    }
}
