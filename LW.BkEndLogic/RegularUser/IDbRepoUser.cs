using LW.BkEndModel;

namespace LW.BkEndLogic.RegularUser
{
	public interface IDbRepoUser
	{
		IEnumerable<Tranzactii> GetAllTranzactiiWithDraw(Guid conexId);
		IEnumerable<Tranzactii> GetAllTranzactiiTransfer(Guid conexId);
		IEnumerable<Documente> GetAllDocumenteFileManager(Guid conexId);
		IEnumerable<Documente> GetAllDocumenteOperatii(Guid conexId);
		Documente GetDocument(Guid entityId);
	}

}
