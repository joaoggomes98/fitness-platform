
using Microsoft.AspNetCore.Identity;

namespace FitnessPlatform.API.Models;
public class AppRole : IdentityRole
{
    /// <summary>
    /// Relação entre usuários e esta role.
    /// </summary>
    public ICollection<AppUserRole> UserRoles { get; set; } = new List<AppUserRole>();
}
