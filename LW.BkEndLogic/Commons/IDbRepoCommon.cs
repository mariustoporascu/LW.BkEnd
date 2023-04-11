﻿using LW.BkEndDb;
using LW.BkEndModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LW.BkEndLogic.Commons
{
	public interface IDbRepoCommon
	{
		IEnumerable<FirmaDiscount> GetAllFolders();
		Task<bool> AddCommonEntity<T>(T entity);
		Task<bool> UpdateCommonEntity<T>(T entity);
		Task<bool> DeleteCommonEntity<T>(T entity);
	}
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