using LW.BkEndDb;
using LW.BkEndModel;
using LW.BkEndModel.Enums;
using Microsoft.EntityFrameworkCore;
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
            doc.StatusName = status.ToString();
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
    }
}
