﻿using LW.BkEndDb;
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
		FirmaDiscount? GetFirmaDiscountById(Guid id);
		Documente? GetDocumentByBlobName(string blobName);
		ConexiuniConturi? GetConexCont(Guid conexId);
		Documente? GetDocumentById(Guid documentId);
		bool CheckDocExists(Guid conexId, Guid documentId);
		string? GetBlobFileType(string blobName);
		Task<bool> UpdateBlobStatus(string blobName, StatusEnum status);
		bool GetBlobType(string blobName);
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
		public Documente? GetDocumentById(Guid documentId)
		{
			return _context.Documente.FirstOrDefault(d => d.Id == documentId);
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
		public IEnumerable<FirmaDiscount> GetAllFolders()
		{
			return _context.FirmaDiscount.Where(x => x.IsActive)
				.Select(x => new FirmaDiscount
				{
					Id = x.Id,
					Name = x.Name,
					DiscountPercent = x.DiscountPercent,
					IsActive = x.IsActive,
				}).AsEnumerable();
		}

		public string? GetBlobFileType(string blobName)
		{
			return _context.FisiereDocumente.FirstOrDefault(f => f.Identifier == blobName)?.FileExtension;
		}

		public bool GetBlobType(string blobName)
		{
			return _context.Documente.FirstOrDefault(d => d.FisiereDocumente.Identifier == blobName)?.IsInvoice ?? false;
		}

		public Documente? GetDocumentByBlobName(string blobName)
		{
			return _context.Documente.Include(d => d.ConexiuniConturi).Include(d => d.FisiereDocumente).FirstOrDefault(d => d.FisiereDocumente.Identifier == blobName);
		}

		public FirmaDiscount? GetFirmaDiscountById(Guid id)
		{
			return _context.FirmaDiscount.FirstOrDefault(x => x.Id == id);
		}

		public async Task<bool> UpdateBlobStatus(string blobName, StatusEnum status)
		{
			var doc = _context.Documente.FirstOrDefault(d => d.FisiereDocumente.Identifier == blobName);
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

		public ConexiuniConturi? GetConexCont(Guid conexId)
		{
			return _context.ConexiuniConturi.FirstOrDefault(c => c.Id == conexId);
		}
	}
}
