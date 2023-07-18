using LW.BkEndDb;
using LW.BkEndModel;
using LW.BkEndModel.Enums;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace LW.BkEndLogic.FirmaDiscUser
{
    public class DbRepoFirma : IDbRepoFirma
    {
        private readonly LwDBContext _context;

        public DbRepoFirma(LwDBContext context)
        {
            _context = context;
        }

        public IEnumerable<Documente> GetAllDocumenteWFP(Guid conexId)
        {
            var conex = _context.ConexiuniConturi.Find(conexId);
            if (conex == null)
                return null;
            return _context.Documente
                .Include(d => d.FisiereDocumente)
                .Include(d => d.ConexiuniConturi)
                .ThenInclude(c => c.ProfilCont)
                .Where(
                    d =>
                        d.FirmaDiscountId == conex.FirmaDiscountId
                        && d.Status == (int)StatusEnum.WaitingForApproval
                )
                .Select(
                    d =>
                        new Documente
                        {
                            Id = d.Id,
                            OcrDataJson = d.OcrDataJson,
                            FirmaDiscountId = d.FirmaDiscountId,
                            DiscountValue = d.DiscountValue,
                            ConexiuniConturi = d.ConexiuniConturi,
                            FisiereDocumente = d.FisiereDocumente,
                        }
                )
                .AsEnumerable();
        }

        public async Task<bool> UpdateDocStatusAsync(Documente documente, StatusEnum status)
        {
            documente.Status = (int)status;
            return await UpdateCommonEntity(documente);
        }

        public object GetDashboardInfo(Guid conexId)
        {
            var conex = _context.ConexiuniConturi.Find(conexId);
            if (conex == null)
                return null;
            int[] opsInts = new int[]
            {
                (int)StatusEnum.Approved,
                (int)StatusEnum.Rejected,
                (int)StatusEnum.WaitingForPreApproval,
                (int)StatusEnum.WaitingForApproval,
            };
            var tableDocs = _context.Documente
                .Where(
                    d => d.FirmaDiscountId == conex.FirmaDiscountId && opsInts.Contains(d.Status)
                )
                .OrderByDescending(doc => doc.Uploaded)
                .Select(
                    doc =>
                        new Documente
                        {
                            Id = doc.Id,
                            OcrDataJson = doc.OcrDataJson,
                            Status = doc.Status,
                            FirmaDiscountId = doc.FirmaDiscountId,
                            Uploaded = doc.Uploaded,
                            DiscountValue = doc.DiscountValue,
                        }
                )
                .Take(5)
                .AsEnumerable();

            // curr date
            var currentDate = DateTime.UtcNow;
            var currentMonth = currentDate.Month;
            var currentYear = currentDate.Year;
            var previousMonth = currentDate.AddMonths(-1).Month;
            var previousYear = currentDate.AddMonths(-1).Year;

            // this month
            var countDocUpThisMth = _context.Documente
                .Where(
                    d =>
                        d.FirmaDiscountId == conex.FirmaDiscountId
                        && opsInts.Contains(d.Status)
                        && d.Uploaded.Month == currentMonth
                        && d.Uploaded.Year == currentYear
                )
                .Count();
            var countPtsRejThisMth = _context.Documente
                .Where(
                    d =>
                        d.FirmaDiscountId == conex.FirmaDiscountId
                        && d.Status == (int)StatusEnum.Rejected
                        && d.Uploaded.Month == currentMonth
                        && d.Uploaded.Year == currentYear
                )
                .Select(doc => doc.DiscountValue)
                .Sum();
            var countPtsAprThisMth = _context.Documente
                .Where(
                    d =>
                        d.FirmaDiscountId == conex.FirmaDiscountId
                        && d.Status == (int)StatusEnum.Approved
                        && d.Uploaded.Month == currentMonth
                        && d.Uploaded.Year == currentYear
                )
                .Select(doc => doc.DiscountValue)
                .Sum();

            // last month
            var countDocUpLastMth = _context.Documente
                .Where(
                    d =>
                        d.FirmaDiscountId == conex.FirmaDiscountId
                        && opsInts.Contains(d.Status)
                        && d.Uploaded.Month == previousMonth
                        && d.Uploaded.Year == previousYear
                        && d.Tranzactii != null
                        && !d.Tranzactii.Any(
                            t => t.Type == (int)TranzactionTypeEnum.Transfer && t.ConexId == conexId
                        )
                )
                .Count();
            var countPtsRejLastMth = _context.Documente
                .Where(
                    d =>
                        d.FirmaDiscountId == conex.FirmaDiscountId
                        && d.Status == (int)StatusEnum.Rejected
                        && d.Uploaded.Month == previousMonth
                        && d.Uploaded.Year == previousYear
                )
                .Select(doc => doc.DiscountValue)
                .Sum();
            var countPtsAprLastMth = _context.Documente
                .Where(
                    d =>
                        d.FirmaDiscountId == conex.FirmaDiscountId
                        && d.Status == (int)StatusEnum.Approved
                        && d.Uploaded.Month == previousMonth
                        && d.Uploaded.Year == previousYear
                )
                .Select(doc => doc.DiscountValue)
                .Sum();

            var monthlyAnalitics = new List<object>();
            for (int i = 1; i <= 12; i++)
            {
                monthlyAnalitics.Add(
                    new
                    {
                        Label = DateTime
                            .Parse($"2000-{(i < 9 ? $"0{i}" : i)}-{01}")
                            .ToString("MMM"),
                        Value = _context.Documente
                            .Where(
                                d =>
                                    d.FirmaDiscountId == conex.FirmaDiscountId
                                    && d.Status == (int)StatusEnum.Approved
                                    && d.Uploaded.Month == i
                                    && d.Uploaded.Year == currentYear
                            )
                            .Select(doc => doc.DiscountValue)
                            .Sum(),
                    }
                );
            }

            return new
            {
                LatestDocs = tableDocs,
                LastTwoMths = new
                {
                    CountDocUpThisMth = countDocUpThisMth,
                    CountPtsRejThisMth = countPtsRejThisMth,
                    CountPtsAprThisMonth = countPtsAprThisMth,
                    CountDocUpLastMth = countDocUpLastMth,
                    CountPtsRejLastMth = countPtsRejLastMth,
                    CountPtsAprLastMonth = countPtsAprLastMth,
                },
                MonthlyAnalitics = monthlyAnalitics
            };
        }

        private async Task<bool> AddCommonEntity<T>(T entity)
        {
            _context.Add(entity);
            return await SaveChangesAsync();
        }

        private async Task<bool> UpdateCommonEntity<T>(T entity)
        {
            _context.Update(entity);
            return await SaveChangesAsync();
        }

        private async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public Guid GetFirmaDiscountId(Guid conexId)
        {
            return _context.ConexiuniConturi.Find(conexId)?.FirmaDiscountId ?? Guid.Empty;
        }

        public IEnumerable<Hybrid> GetAllHybrids(Guid firmaDiscountId)
        {
            return _context.Hybrid
                .Where(h => h.FirmaDiscountId == firmaDiscountId)
                .Select(
                    h =>
                        new Hybrid
                        {
                            Id = h.Id,
                            Name = h.Name,
                            NoSubAccounts = h.ConexiuniConturi.Count,
                        }
                )
                .AsEnumerable();
        }

        public async Task<bool> DeleteHybrid(Guid firmaDiscountId, Guid groupId)
        {
            var hybrid = _context.Hybrid.Find(groupId);
            if (hybrid == null)
                return false;

            var users = _context.Users.Where(u => u.ConexiuniConturi.HybridId == groupId);
            if (users.Any())
                _context.Users.RemoveRange(users);

            _context.Hybrid.Remove(hybrid);
            return await SaveChangesAsync();
        }

        public async Task<bool> CheckIfHybrindExists(string name, Guid firmaDiscountId)
        {
            return await _context.Hybrid.AnyAsync(
                h => h.Name.ToLower() == name.ToLower() && h.FirmaDiscountId == firmaDiscountId
            );
        }

        public IEnumerable<Documente> GetAllDocuments(Guid conexId)
        {
            var conex = _context.ConexiuniConturi.Find(conexId);
            if (conex == null)
                return null;
            return _context.Documente
                .Include(d => d.FisiereDocumente)
                .Include(d => d.ConexiuniConturi)
                .ThenInclude(c => c.ProfilCont)
                .Where(d => d.FirmaDiscountId == conex.FirmaDiscountId)
                .Select(
                    d =>
                        new Documente
                        {
                            Id = d.Id,
                            OcrDataJson = d.OcrDataJson,
                            FirmaDiscountId = d.FirmaDiscountId,
                            DiscountValue = d.DiscountValue,
                            ConexiuniConturi = d.ConexiuniConturi,
                            FisiereDocumente = d.FisiereDocumente,
                        }
                )
                .AsEnumerable();
        }
    }
}
