using Microsoft.AspNetCore.Identity;
using RSS_DB;
using RSS_DB.Entities;

namespace RSS_Services
{
    public class UserServices
    {
        private readonly AppDbContext _appDbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserServices(AppDbContext appDbContext, UserManager<ApplicationUser> userManager)
        {
            _appDbContext = appDbContext;
            _userManager = userManager;
        }

        public ApplicationUser FindUserById(string userId)
        {
            return _appDbContext.Users.FirstOrDefault(u => u.Id == userId);
        }

        public async Task<IdentityResult> UpdateDisplayNameAsync(string userId, string displayName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });

            user.DisplayName = displayName;
            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> UpdateGamerTagAsync(string userId, string gamerTag)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });

            user.GamerTag = gamerTag;
            return await _userManager.UpdateAsync(user);
        }
    }
}
