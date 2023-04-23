using LW.BkEndDb;
using LW.BkEndLogic.Commons.Interfaces;
using LW.BkEndModel;

namespace LW.BkEndLogic.Commons
{
	public class DbRepoCommon : IDbRepoCommon
	{
		private readonly LwDBContext _context;
		public DbRepoCommon(LwDBContext context)
		{
			_context = context;
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

		public IEnumerable<object> FindUsers(string emailOrPhone)
		{
			var users = _context.Users.Where(usr => usr.Email.Contains(emailOrPhone) || usr.PhoneNumber.Contains(emailOrPhone));
			var usersList = new List<object>();
			foreach
				(var usr in users)
			{
				usersList.Add(new
				{
					email = usr.Email,
					phoneNumber = usr.PhoneNumber,
					conexId = _context.ConexiuniConturi.FirstOrDefault(con => con.UserId == usr.Id)?.Id,
				});
			}
			return usersList;
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
