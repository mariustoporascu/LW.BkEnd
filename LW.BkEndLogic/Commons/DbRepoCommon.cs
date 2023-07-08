using LW.BkEndDb;
using LW.BkEndLogic.Commons.Interfaces;
using LW.BkEndModel;
using Microsoft.EntityFrameworkCore;

namespace LW.BkEndLogic.Commons
{
    public class DbRepoCommon : IDbRepoCommon
    {
        private readonly LwDBContext _context;

        public DbRepoCommon(LwDBContext context)
        {
            _context = context;
        }

        public async Task<bool> AddFavoriteUser(Guid conexId, Guid favConexId)
        {
            var newFavorite = new PreferinteHybrid { ConexId = conexId, MyConexId = favConexId, };
            return await AddCommonEntity(newFavorite);
        }

        public async Task<bool> RemoveFavoriteUser(Guid conexId, Guid favConexId)
        {
            var favorite = _context.PreferinteHybrid.FirstOrDefault(
                fav => fav.ConexId == conexId && fav.MyConexId == favConexId
            );
            if (favorite == null)
            {
                return false;
            }
            return await DeleteCommonEntity(favorite);
        }

        public IEnumerable<object> GetFavoritesList(Guid conexId)
        {
            var favorites = _context.PreferinteHybrid.Where(fav => fav.ConexId == conexId);
            var favoritesList = new List<object>();
            foreach (var fav in favorites)
            {
                var conex = _context.ConexiuniConturi
                    .Include(c => c.ProfilCont)
                    .FirstOrDefault(con => con.Id == fav.MyConexId);
                favoritesList.Add(
                    new
                    {
                        conexId = conex.Id,
                        fullName = $"{conex.ProfilCont?.Name} {conex.ProfilCont?.FirstName}",
                        phoneNumber = conex.ProfilCont.PhoneNumber,
                        email = conex.ProfilCont.Email,
                    }
                );
            }
            return favoritesList;
        }

        public async Task<bool> EmailNotTaken(string email)
        {
            return !(await _context.Users.AnyAsync(usr => usr.Email == email));
        }

        public IEnumerable<object> FindUsers(string emailOrPhone, Guid conexId)
        {
            var users = _context.ConexiuniConturi
                .Include(c => c.ProfilCont)
                .Where(
                    usr =>
                        (
                            usr.ProfilCont.Email.ToLower().Contains(emailOrPhone.ToLower())
                            || usr.ProfilCont.PhoneNumber.Contains(emailOrPhone)
                        )
                        && usr.FirmaDiscountId == null
                        && usr.HybridId == null
                        && usr.UserId != null
                        && usr.Id != conexId
                        && !_context.PreferinteHybrid.Any(
                            fav => fav.ConexId == conexId && fav.MyConexId == usr.Id
                        )
                );
            //var users = users.Where(
            //    usr =>
            //        !_context.PreferinteHybrid.Any(
            //            fav => fav.ConexId == conexId && fav.MyConexId == usr.Id
            //        )
            //);
            var usersList = new List<object>();
            foreach (var usr in users)
            {
                usersList.Add(
                    new
                    {
                        conexId = usr.Id,
                        fullName = $"{usr.ProfilCont?.Name} {usr.ProfilCont?.FirstName}",
                        phoneNumber = usr.ProfilCont.PhoneNumber,
                        email = usr.ProfilCont.Email,
                    }
                );
            }
            return usersList;
        }

        public async Task<bool> AddCommonEntity<T>(T entity)
        {
            _context.Add(entity);
            return await SaveChangesAsync();
        }

        public async Task<bool> DeleteCommonEntity<T>(T entity)
        {
            _context.Remove(entity);
            return await SaveChangesAsync();
        }

        public IEnumerable<FirmaDiscount> GetAllFolders()
        {
            return _context.FirmaDiscount
                .Where(x => x.IsActive)
                .Select(
                    x =>
                        new FirmaDiscount
                        {
                            Id = x.Id,
                            Name = x.Name,
                            CuiNumber = x.CuiNumber,
                            DiscountPercent = x.DiscountPercent,
                            IsActive = x.IsActive,
                        }
                )
                .AsEnumerable();
        }

        public IEnumerable<FirmaDiscount> GetAllFolders(Guid hybridId)
        {
            var firmaDiscountId = _context.Hybrid
                .FirstOrDefault(x => x.Id == hybridId)
                ?.FirmaDiscountId;
            return _context.FirmaDiscount
                .Where(x => x.IsActive && x.Id == firmaDiscountId)
                .Select(
                    x =>
                        new FirmaDiscount
                        {
                            Id = x.Id,
                            Name = x.Name,
                            CuiNumber = x.CuiNumber,
                            DiscountPercent = x.DiscountPercent,
                            IsActive = x.IsActive,
                        }
                )
                .AsEnumerable();
        }

        public async Task<bool> UpdateCommonEntity<T>(T entity)
        {
            _context.Update(entity);
            return await SaveChangesAsync();
        }

        private async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<T?> GetCommonEntity<T>(Guid entityId)
            where T : class
        {
            return await _context.Set<T>().FindAsync(entityId);
        }
    }
}
