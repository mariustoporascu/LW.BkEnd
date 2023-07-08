using LW.BkEndModel;
using LW.BkEndModel.Enums;

namespace LW.BkEndLogic.RegularUser
{
    public interface IDbRepoUser
    {
        IEnumerable<Tranzactii> GetAllTranzactiiWithDraw(Guid conexId);
        IEnumerable<Tranzactii> GetAllTranzactiiTransfer(Guid conexId);
        IEnumerable<Documente> GetAllDocumenteFileManager(Guid conexId);
        object GetDashboardInfo(Guid conexId);
        Guid GetMyHybridId(Guid conexId);
        IEnumerable<Documente> GetAllDocumenteOperatii(Guid conexId);
        Documente GetDocument(Guid entityId);
        Task<bool> AddTranzaction(
            Guid conexId,
            Documente documente,
            TranzactionTypeEnum tranzactionType,
            Guid? nextConexId
        );
    }
}
