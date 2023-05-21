using LW.BkEndModel.Enums;
using LW.BkEndModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LW.BkEndLogic.HybridUser
{
	public interface IDbRepoHybrid
	{
		Guid GetMyHybridId(Guid conexId);
		IEnumerable<Tranzactii> GetAllTranzactiiTransfer(Guid hybridId);
		IEnumerable<Documente> GetAllDocumenteFileManager(Guid hybridId);
		object GetDashboardInfo(Guid hybridId);
		IEnumerable<Documente> GetAllDocumenteOperatii(Guid hybridId);
		Documente GetDocument(Guid entityId);
		Task<bool> AddTranzaction(Guid conexId, Documente documente, TranzactionTypeEnum tranzactionType, Guid? nextConexId);
	}
}
