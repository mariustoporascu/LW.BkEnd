using LW.BkEndModel;
using LW.BkEndModel.Enums;

namespace LW.BkEndLogic.FirmaDiscUser
{
    public interface IDbRepoFirma
    {
        IEnumerable<Documente> GetAllDocumenteWFP(Guid conexId);
        IEnumerable<Documente> GetAllDocuments(Guid conexId);
        Task<bool> UpdateDocStatusAsync(Documente documente, StatusEnum status);
        object GetDashboardInfo(Guid conexId);
        Guid GetFirmaDiscountId(Guid conexId);
        IEnumerable<Hybrid> GetAllHybrids(Guid firmaDiscountId);
        Task<bool> CheckIfHybrindExists(string name, Guid firmaDiscountId);
        Task<bool> DeleteHybrid(Guid firmaDiscountId, Guid groupId);
    }
}
