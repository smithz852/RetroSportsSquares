using RSS_DB;
using RSS_DB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSS_Services
{
    public class GeneralServices
    {
        AppDbContext _appDbContext;

        public GeneralServices(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<int> SaveData<T>(T entity) where T : class
        {
            if (!typeof(T).Namespace.StartsWith("RSS_DB.Entities"))
            {
                return 0;
            }
            var result =await _appDbContext.SaveChangesAsync();
            return result;
        }
    }
}
