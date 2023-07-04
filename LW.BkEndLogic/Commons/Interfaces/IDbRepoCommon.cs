using LW.BkEndModel;

namespace LW.BkEndLogic.Commons.Interfaces
{
    public interface IDbRepoCommon
    {
        IEnumerable<FirmaDiscount> GetAllFolders();
        IEnumerable<FirmaDiscount> GetAllFolders(Guid hybridId);
        IEnumerable<object> FindUsers(string emailOrPhone);
        Task<bool> RemoveFavoriteUser(Guid conexId, Guid favConexId);
        Task<bool> AddFavoriteUser(Guid conexId, Guid favConexId);
        IEnumerable<object> GetFavoritesList(Guid conexId);
        Task<bool> EmailNotTaken(string email);
        Task<bool> AddCommonEntity<T>(T entity);
        Task<bool> UpdateCommonEntity<T>(T entity);
        Task<bool> DeleteCommonEntity<T>(T entity);
    }
}
