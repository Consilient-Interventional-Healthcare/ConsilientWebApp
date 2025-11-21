using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Consilient.Users.Services.Data
{
    internal class UsersDbContext(DbContextOptions<UsersDbContext> options) : IdentityDbContext<IdentityUser>(options)
    {
    }
}
