﻿using LW.BkEndModel;

namespace LW.BkEndLogic.Commons.Interfaces
{
	public interface IDbRepoCommon
	{
		IEnumerable<FirmaDiscount> GetAllFolders();
		IEnumerable<FirmaDiscount> GetAllFolders(Guid hybridId);
		IEnumerable<object> FindUsers(string emailOrPhone);
		Task<bool> EmailNotTaken(string email);
		Task<bool> AddCommonEntity<T>(T entity);
		Task<bool> UpdateCommonEntity<T>(T entity);
		Task<bool> DeleteCommonEntity<T>(T entity);
	}

}
