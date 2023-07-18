using LW.BkEndDb;
using LW.BkEndModel;
using LW.BkEndModel.Enums;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LW.DocProcLogic.DbRepo
{
    public interface IDbRepo
    {
        Documente? GetDocumentByBlobName(string blobName);
        bool CheckDocExists(Guid conexId, Guid documentId);
        Task<bool> CheckDuplicateDocExists(
            Guid firmaDiscountId,
            Guid documentId,
            JObject processedResult
        );
        string? GetBlobFileType(string blobName);
        Task<bool> UpdateBlobStatus(string blobName, StatusEnum status);
        bool GetBlobType(string blobName);
        Task<T?> GetCommonEntity<T>(Guid entityId)
            where T : class;
        Task<bool> AddCommonEntity<T>(T entity);
        Task<bool> UpdateCommonEntity<T>(T entity);
        Task<bool> DeleteCommonEntity<T>(T entity);
    }

    public class DbRepo : IDbRepo
    {
        private readonly LwDBContext _context;

        public DbRepo(LwDBContext context)
        {
            _context = context;
        }

        public async Task<bool> AddCommonEntity<T>(T entity)
        {
            _context.Add(entity);
            return await SaveChangesAsync();
        }

        public bool CheckDocExists(Guid conexId, Guid documentId)
        {
            return _context.Documente.Any(d => d.ConexId == conexId && d.Id == documentId);
        }

        public async Task<bool> DeleteCommonEntity<T>(T entity)
        {
            _context.Remove(entity);
            return await SaveChangesAsync();
        }

        public async Task<T?> GetCommonEntity<T>(Guid entityId)
            where T : class
        {
            return await _context.Set<T>().FindAsync(entityId);
        }

        public string? GetBlobFileType(string blobName)
        {
            return _context.FisiereDocumente
                .FirstOrDefault(f => f.Identifier == blobName)
                ?.FileExtension;
        }

        public bool GetBlobType(string blobName)
        {
            return _context.Documente
                    .FirstOrDefault(d => d.FisiereDocumente.Identifier == blobName)
                    ?.IsInvoice ?? false;
        }

        public Documente? GetDocumentByBlobName(string blobName)
        {
            return _context.Documente
                .Include(d => d.ConexiuniConturi)
                .Include(d => d.FisiereDocumente)
                .FirstOrDefault(d => d.FisiereDocumente.Identifier == blobName);
        }

        public async Task<bool> UpdateBlobStatus(string blobName, StatusEnum status)
        {
            var doc = _context.Documente.FirstOrDefault(
                d => d.FisiereDocumente.Identifier == blobName
            );
            if (doc == null)
            {
                return false;
            }
            doc.Status = (int)status;
            return await UpdateCommonEntity(doc);
        }

        public async Task<bool> UpdateCommonEntity<T>(T entity)
        {
            _context.Update(entity);
            return await SaveChangesAsync();
        }

        private async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> CheckDuplicateDocExists(
            Guid firmaDiscountId,
            Guid documentId,
            JObject processedResult
        )
        {
            int[] excludeStatus = new int[]
            {
                (int)StatusEnum.FailedProcessing,
                (int)StatusEnum.DuplicateError,
            };
            var docNumber = processedResult["docNumber"]["value"].ToString();
            var dateValue = processedResult["dataTranzactie"]["value"].ToString();
            var totalValue = processedResult["total"]["value"].ToString();
            var doc = _context.Documente
                .Include(d => d.ConexiuniConturi)
                .FirstOrDefault(
                    d =>
                        d.FirmaDiscountId == firmaDiscountId
                        && !excludeStatus.Contains(d.Status)
                        && d.Id != documentId
                        && (
                            d.OcrDataJson.Contains(docNumber)
                            && !string.IsNullOrWhiteSpace(docNumber)
                        )
                        && (
                            d.OcrDataJson.Contains(dateValue)
                            && !string.IsNullOrWhiteSpace(dateValue)
                        )
                        && (
                            d.OcrDataJson.Contains(totalValue)
                            && !string.IsNullOrWhiteSpace(totalValue)
                        )
                );
            if (doc == null)
                return false;
            if (doc.ConexiuniConturi.HybridId == null)
                return true;
            // Uploaded previously by hybrid, so give priority to normal user
            // only if the hybrid user has not withdrawn the document
            if (
                !_context.Tranzactii.Any(
                    t => t.DocumenteId == doc.Id && t.Type == (int)TranzactionTypeEnum.Withdraw
                )
            )
            {
                doc.Status = (int)StatusEnum.DuplicateError;
                await UpdateCommonEntity(doc);
                return false;
            }
            return true;
        }
    }
}
