using LW.BkEndModel;

namespace LW.BkEndLogic.FirmaDiscUser
{
	public interface IDbRepoFirma
	{
		IEnumerable<Documente> GetAllDocumente(Guid firmaId);
		Documente GetDocument(Guid entityId);
	}

}
