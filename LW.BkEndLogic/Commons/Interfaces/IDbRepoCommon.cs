using LW.BkEndModel;

namespace LW.BkEndLogic.Commons.Interfaces
{
	public interface IDbRepoCommon
	{
		IEnumerable<FirmaDiscount> GetAllFolders();
		Task<bool> AddCommonEntity<T>(T entity);
		Task<bool> UpdateCommonEntity<T>(T entity);
		Task<bool> DeleteCommonEntity<T>(T entity);
	}

}
