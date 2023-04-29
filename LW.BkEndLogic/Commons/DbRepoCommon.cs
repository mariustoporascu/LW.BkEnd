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
		public IEnumerable<object> FindUsers(string emailOrPhone)
		{
			var users = _context.Users
				.Where(usr => (usr.Email.Contains(emailOrPhone) || usr.PhoneNumber.Contains(emailOrPhone)) &&
				 usr.ConexiuniConturi.FirmaDiscountId == null && usr.ConexiuniConturi.HybridId == null);
			var usersList = new List<object>();
			foreach
				(var usr in users)
			{
				var conex = _context.ConexiuniConturi.Include(c => c.ProfilCont).FirstOrDefault(con => con.UserId == usr.Id);
				if (conex == null)
				{
					continue;
				}
				usersList.Add(new
				{
					fullName = $"{conex.ProfilCont?.Name} {conex.ProfilCont?.FirstName}",
					phoneNumber = usr.PhoneNumber,
					conexId = _context.ConexiuniConturi.FirstOrDefault(con => con.UserId == usr.Id)?.Id,
				});
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
			return _context.FirmaDiscount.Where(x => x.IsActive)
				.Select(x => new FirmaDiscount
				{
					Id = x.Id,
					Name = x.Name,
					DiscountPercent = x.DiscountPercent,
					IsActive = x.IsActive,
				}).AsEnumerable();
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
	}
}
