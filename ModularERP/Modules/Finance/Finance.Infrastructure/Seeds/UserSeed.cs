using Microsoft.AspNetCore.Identity;
using ModularERP.Modules.Finance.Finance.Infrastructure.Data;
using ModularERP.Common.Models;

namespace ModularERP.Modules.Finance.Finance.Infrastructure.Seeds
{
    public static class UserSeed
    {
        public static void SeedUsers(this FinanceDbContext context)
        {

                var userId = Guid.Parse("f0602c31-0c12-4b5c-9ccf-fe17811d5c53");
                if (!context.Users.Any(u => u.Id == userId))
                {
                    var user = new ApplicationUser
                    {
                        Id = userId,
                        UserName = "Hossam",
                        Email = "hossam@example.com",
                        EmailConfirmed = true,
                        PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(null, "Admin@123")
                    };
                    context.Users.Add(user);
                    context.SaveChanges();
                }

            
        }
    }
}
