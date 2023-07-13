using LW.BkEndModel;
using LW.BkEndModel.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LW.BkEndLogic.MasterUser
{
    public interface IDbRepoMaster
    {
        object GetDashboardInfo();
        IEnumerable<FirmaDiscount> GetFirmeDiscountList();
        IEnumerable<Documente> GetDocumenteList();
        IEnumerable<Documente> GetDocumentePreAprobareList();
        Task<bool> ChangeDocStatus(Guid documentId, StatusEnum status);
        bool FirmaDiscountExists(string cuiNumber);
    }
}
