using LW.BkEndDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LW.BkEndLogic.MasterUser
{
    public class DbRepoMaster : IDbRepoMaster
    {
        private readonly LwDBContext _context;

        public DbRepoMaster(LwDBContext context)
        {
            _context = context;
        }

        public object GetDashboardInfo()
        {
            var firmeActiveCount = _context.FirmaDiscount.Where(x => x.IsActive).Count();
            var firmeInactiveCount = _context.FirmaDiscount.Where(x => !x.IsActive).Count();
            var useriCount = _context.Users.Count();
            var documenteCount = _context.Documente.Count();
            var puncteAcordateCount = _context.Documente
                .Where(x => x.Status == 1)
                .Sum(x => x.DiscountValue);
            var puncteRetraseCount = _context.Tranzactii.Where(x => x.Type == 2).Sum(x => x.Amount);
            var puncteFacturateCount = 0;
            var documenteRespinse = _context.Documente.Where(x => x.Status == 2).Count();
            return new
            {
                firmeActiveCount,
                firmeInactiveCount,
                useriCount,
                documenteCount,
                puncteAcordateCount,
                puncteRetraseCount,
                puncteFacturateCount,
                documenteRespinse
            };
        }
    }
}
