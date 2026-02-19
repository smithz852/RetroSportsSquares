using RSS_DB;
using RSS_DB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSS_Services
{
    public class UserServices
    {
        private readonly AppDbContext _appDbContext;
        public UserServices(AppDbContext appDbContext) 
        {
             _appDbContext = appDbContext;
        }

        public ApplicationUser FindUserById(string userId)
        {
            var user = _appDbContext.Users.FirstOrDefault(u => u.Id == userId);
            return user;
        }
    }
}
