using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Consilient.Data
{
    public class UsersDbContext(DbContextOptions<UsersDbContext> options) : IdentityDbContext<IdentityUser>(options)
    {
    }
}
