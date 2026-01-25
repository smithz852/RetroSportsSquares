using Microsoft.EntityFrameworkCore;
using RSS_DB;
using RSS_DB.Entities;

namespace RSS_Services
{
    public class AvailableGamesServices
    {
        private readonly AppDbContext _appDbContext;

        public AvailableGamesServices(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public List<AvailableGames> GetAllAvailableGames()
        {
            return _appDbContext.AvailableGames.ToList();
        }

    }
}