using LW.BkEndDb;
using LW.BkEndModel;
using LW.BkEndModel.Enums;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace LW.BkEndLogic.RegularUser
{
    public class DbRepoUser : IDbRepoUser
    {
        private readonly LwDBContext _context;

        public DbRepoUser(LwDBContext context)
        {
            _context = context;
        }

        public IEnumerable<Tranzactii> GetAllTranzactiiWithDraw(Guid conexId)
        {
            return _context.Tranzactii
                .Where(d => d.ConexId == conexId && d.Type == (int)TranzactionTypeEnum.Withdraw)
                .AsEnumerable();
        }

        public IEnumerable<Tranzactii> GetAllTranzactiiTransfer(Guid conexId)
        {
            return _context.Tranzactii
                .Include(t => t.Documente.NextConexiuniConturi.ProfilCont)
                .Where(d => d.ConexId == conexId && d.Type == (int)TranzactionTypeEnum.Transfer)
                .AsEnumerable();
        }

        public IEnumerable<Documente> GetAllDocumenteOperatii(Guid conexId)
        {
            return _context.Documente
                .Include(d => d.FisiereDocumente)
                .Where(
                    d =>
                        (d.ConexId == conexId || d.NextConexId == conexId)
                        && d.Status == (int)StatusEnum.Approved
                        && d.Tranzactii != null
                        && !d.Tranzactii.Any(t => t.ConexId == conexId)
                )
                .Select(
                    doc =>
                        new Documente
                        {
                            Id = doc.Id,
                            OcrDataJson = doc.OcrDataJson,
                            Status = doc.Status,
                            Uploaded = doc.Uploaded,
                            StatusName = doc.StatusName,
                            DiscountValue = doc.DiscountValue,
                            IsInvoice = doc.IsInvoice,
                            FirmaDiscountId = doc.FirmaDiscountId,
                            FisiereDocumente = doc.FisiereDocumente
                        }
                )
                .AsEnumerable();
        }

        public IEnumerable<Documente> GetAllDocumente(Guid conexId)
        {
            int[] opsInts = new int[]
            {
                (int)StatusEnum.Processing,
                (int)StatusEnum.FailedProcessing,
                (int)StatusEnum.CompletedProcessing,
                (int)StatusEnum.PartialyProcessed,
                (int)StatusEnum.NoStatus,
            };
            return _context.Documente
                .Include(d => d.FisiereDocumente)
                .Where(
                    d =>
                        (d.ConexId == conexId || d.NextConexId == conexId)
                        && !opsInts.Contains(d.Status)
                        && d.Tranzactii != null
                        && !d.Tranzactii.Any(
                            t => t.ConexId == conexId && t.Type == (int)TranzactionTypeEnum.Transfer
                        )
                )
                .Select(
                    doc =>
                        new Documente
                        {
                            Id = doc.Id,
                            OcrDataJson = doc.OcrDataJson,
                            Status = doc.Status,
                            Uploaded = doc.Uploaded,
                            StatusName = doc.StatusName,
                            DiscountValue = doc.DiscountValue,
                            IsInvoice = doc.IsInvoice,
                            FirmaDiscountId = doc.FirmaDiscountId,
                            FisiereDocumente = doc.FisiereDocumente
                        }
                )
                .AsEnumerable();
        }

        public IEnumerable<Documente> GetAllDocumenteFileManager(Guid conexId)
        {
            int[] opsInts = new int[]
            {
                (int)StatusEnum.Processing,
                (int)StatusEnum.PartialyProcessed,
                (int)StatusEnum.FailedProcessing,
                (int)StatusEnum.NoStatus,
            };
            return _context.Documente
                .Include(d => d.FisiereDocumente)
                .Where(
                    d =>
                        (d.ConexId == conexId || d.NextConexId == conexId)
                        && opsInts.Contains(d.Status)
                        && d.Tranzactii != null
                        && !d.Tranzactii.Any(t => t.ConexId == conexId)
                )
                .Select(
                    doc =>
                        new Documente
                        {
                            Id = doc.Id,
                            OcrDataJson = doc.OcrDataJson,
                            Status = doc.Status,
                            Uploaded = doc.Uploaded,
                            StatusName = doc.StatusName,
                            DiscountValue = doc.DiscountValue,
                            IsInvoice = doc.IsInvoice,
                            FirmaDiscountId = doc.FirmaDiscountId,
                            FisiereDocumente = doc.FisiereDocumente
                        }
                )
                .AsEnumerable();
        }

        public object GetDashboardInfo(Guid conexId)
        {
            int[] opsInts = new int[]
            {
                (int)StatusEnum.Approved,
                (int)StatusEnum.Rejected,
                (int)StatusEnum.WaitingForPreApproval,
                (int)StatusEnum.WaitingForApproval,
            };
            var tableDocs = _context.Documente
                .Where(
                    d =>
                        (d.ConexId == conexId || d.NextConexId == conexId)
                        && opsInts.Contains(d.Status)
                )
                .OrderByDescending(doc => doc.Uploaded)
                .Select(
                    doc =>
                        new Documente
                        {
                            Id = doc.Id,
                            OcrDataJson = doc.OcrDataJson,
                            Status = doc.Status,
                            Uploaded = doc.Uploaded,
                            StatusName = doc.StatusName,
                            DiscountValue = doc.DiscountValue,
                            IsInvoice = doc.IsInvoice,
                            FirmaDiscountId = doc.FirmaDiscountId,
                            FisiereDocumente = doc.FisiereDocumente
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
                        (d.ConexId == conexId || d.NextConexId == conexId)
                        && opsInts.Contains(d.Status)
                        && d.Uploaded.Month == currentMonth
                        && d.Uploaded.Year == currentYear
                        && d.Tranzactii != null
                        && !d.Tranzactii.Any(
                            t => t.Type == (int)TranzactionTypeEnum.Transfer && t.ConexId == conexId
                        )
                )
                .Count();
            var countPtsRcvdThisMth = _context.Documente
                .Where(
                    d =>
                        (d.ConexId == conexId || d.NextConexId == conexId)
                        && d.Status == (int)StatusEnum.Approved
                        && d.Uploaded.Month == currentMonth
                        && d.Uploaded.Year == currentYear
                        && d.Tranzactii != null
                        && !d.Tranzactii.Any(
                            t => t.Type == (int)TranzactionTypeEnum.Transfer && t.ConexId == conexId
                        )
                )
                .Select(doc => doc.DiscountValue)
                .Sum();
            var countPtsSpentThisMonth = _context.Tranzactii
                .Where(
                    d =>
                        d.ConexId == conexId
                        && d.Type == (int)TranzactionTypeEnum.Withdraw
                        && d.Created.Month == currentMonth
                        && d.Created.Year == currentYear
                )
                .Select(doc => doc.Amount)
                .Sum();
            // last month
            var countDocUpLastMth = _context.Documente
                .Where(
                    d =>
                        (d.ConexId == conexId || d.NextConexId == conexId)
                        && opsInts.Contains(d.Status)
                        && d.Uploaded.Month == previousMonth
                        && d.Uploaded.Year == previousYear
                        && d.Tranzactii != null
                        && !d.Tranzactii.Any(
                            t => t.Type == (int)TranzactionTypeEnum.Transfer && t.ConexId == conexId
                        )
                )
                .Count();
            var countPtsRcvdLastMth = _context.Documente
                .Where(
                    d =>
                        (d.ConexId == conexId || d.NextConexId == conexId)
                        && d.Status == (int)StatusEnum.Approved
                        && d.Uploaded.Month == previousMonth
                        && d.Uploaded.Year == previousYear
                        && d.Tranzactii != null
                        && !d.Tranzactii.Any(
                            t => t.Type == (int)TranzactionTypeEnum.Transfer && t.ConexId == conexId
                        )
                )
                .Select(doc => doc.DiscountValue)
                .Sum();
            var countPtsSpentLastMonth = _context.Tranzactii
                .Where(
                    d =>
                        d.ConexId == conexId
                        && d.Type == (int)TranzactionTypeEnum.Withdraw
                        && d.Created.Month == previousMonth
                        && d.Created.Year == previousYear
                )
                .Select(doc => doc.Amount)
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
                                    (d.ConexId == conexId || d.NextConexId == conexId)
                                    && d.Status == (int)StatusEnum.Approved
                                    && d.Uploaded.Month == i
                                    && d.Uploaded.Year == currentYear
                                    && d.Tranzactii != null
                                    && !d.Tranzactii.Any(
                                        t =>
                                            t.Type == (int)TranzactionTypeEnum.Transfer
                                            && t.ConexId == conexId
                                    )
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
                    CountPtsRcvdThisMth = countPtsRcvdThisMth,
                    CountPtsSpentThisMonth = countPtsSpentThisMonth,
                    CountDocUpLastMth = countDocUpLastMth,
                    CountPtsRcvdLastMth = countPtsRcvdLastMth,
                    CountPtsSpentLastMonth = countPtsSpentLastMonth,
                },
                MonthlyAnalitics = monthlyAnalitics
            };
        }

        public async Task<bool> AddTranzaction(
            Guid conexId,
            Documente documente,
            TranzactionTypeEnum tranzactionType,
            Guid? nextConexId
        )
        {
            Tranzactii tranzactie = new Tranzactii
            {
                ConexId = conexId,
                DocumenteId = documente.Id,
                Type = (int)tranzactionType,
                TypeName = Enum.GetName(typeof(TranzactionTypeEnum), tranzactionType),
                Amount = documente.DiscountValue,
            };
            if (tranzactionType == TranzactionTypeEnum.Transfer && nextConexId != null)
            {
                documente.NextConexId = nextConexId;
                await UpdateCommonEntity(documente);
            }
            return await AddCommonEntity(tranzactie);
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

        public Guid GetMyHybridId(Guid conexId)
        {
            return _context.ConexiuniConturi.Find(conexId)?.HybridId ?? Guid.Empty;
        }
    }
}
