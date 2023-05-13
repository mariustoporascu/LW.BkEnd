using LW.BkEndModel;
using LW.BkEndModel.Enums;

namespace LW.BkEndLogic.FirmaDiscUser
{
	public interface IDbRepoFirma
	{
		IEnumerable<Documente> GetAllDocumenteWFP(Guid conexId);
		Documente GetDocument(Guid entityId);
		Task<bool> UpdateDocStatusAsync(Documente documente, StatusEnum status);
		object GetDashboardInfo(Guid conexId);
	}

}
