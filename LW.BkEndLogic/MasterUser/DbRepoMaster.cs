using LW.BkEndDb;
using LW.BkEndModel;
using LW.BkEndModel.Enums;
using Microsoft.EntityFrameworkCore;
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
                .Where(x => x.Status == (int)StatusEnum.Approved)
                .Sum(x => x.DiscountValue);
            var puncteRetraseCount = _context.Tranzactii
                .Where(x => x.Type == (int)TranzactionTypeEnum.Withdraw)
                .Sum(x => x.Amount);
            var puncteFacturateCount = 0;
            var documenteRespinse = _context.Documente
                .Where(x => x.Status == (int)StatusEnum.Rejected)
                .Count();
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

        public IEnumerable<Documente> GetDocumenteList()
        {
            return _context.Documente
                .Include(d => d.FisiereDocumente)
                .Include(d => d.ConexiuniConturi.ProfilCont)
                .Include(d => d.NextConexiuniConturi.ProfilCont)
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
                            NextConexId = doc.NextConexId,
                            ConexiuniConturi = doc.ConexiuniConturi,
                            NextConexiuniConturi = doc.NextConexiuniConturi,
                            FisiereDocumente = doc.FisiereDocumente
                        }
                )
                .AsEnumerable();
        }

        public IEnumerable<FirmaDiscount> GetFirmeDiscountList()
        {
            return _context.FirmaDiscount
                .Select(
                    x =>
                        new FirmaDiscount
                        {
                            Id = x.Id,
                            Name = x.Name,
                            MainContactEmail = x.MainContactEmail,
                            MainContactName = x.MainContactName,
                            MainContactPhone = x.MainContactPhone,
                            CuiNumber = x.CuiNumber,
                            DiscountPercent = x.DiscountPercent,
                            IsActive = x.IsActive,
                            IsActiveSecondary = x.IsActiveSecondary,
                        }
                )
                .AsEnumerable();
        }

        public async Task<bool> ChangeDocStatus(Guid documentId, StatusEnum status)
        {
            var document = _context.Documente.FirstOrDefault(d => d.Id == documentId);
            if (document != null)
            {
                document.Status = (int)status;
                document.StatusName = Enum.GetName(typeof(StatusEnum), status);
                return await UpdateCommonEntity(document);
            }
            return false;
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

        public IEnumerable<Documente> GetDocumentePreAprobareList()
        {
            return _context.Documente
                .Include(d => d.FisiereDocumente)
                .Include(d => d.ConexiuniConturi.ProfilCont)
                .Where(x => x.Status == (int)StatusEnum.WaitingForPreApproval)
                .Select(
                    doc =>
                        new Documente
                        {
                            Id = doc.Id,
                            OcrDataJson = doc.OcrDataJson,
                            Uploaded = doc.Uploaded,
                            DiscountValue = doc.DiscountValue,
                            IsInvoice = doc.IsInvoice,
                            FirmaDiscountId = doc.FirmaDiscountId,
                            ConexiuniConturi = doc.ConexiuniConturi,
                            FisiereDocumente = doc.FisiereDocumente
                        }
                )
                .AsEnumerable();
        }

        public bool FirmaDiscountExists(string cuiNumber)
        {
            return _context.FirmaDiscount.Any(f => f.CuiNumber == cuiNumber);
        }
    }
}
