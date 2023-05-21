﻿using LW.BkEndDb;
using LW.BkEndModel;
using LW.BkEndModel.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LW.BkEndLogic.HybridUser
{
	public class DbRepoHybrid : IDbRepoHybrid
	{
		private readonly LwDBContext _context;
		public DbRepoHybrid(LwDBContext context)
		{
			_context = context;
		}

		public async Task<bool> AddTranzaction(Guid conexId, Documente documente, TranzactionTypeEnum tranzactionType, Guid? nextConexId)
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

		public IEnumerable<Documente> GetAllDocumenteFileManager(Guid hybridId)
		{
			return _context.Documente.Include(d => d.FisiereDocumente)
				.Where(d => d.ConexiuniConturi.HybridId == hybridId && (d.Status == (int)StatusEnum.Processing ||
				d.Status == (int)StatusEnum.PartialyProcessed ||
				d.Status == (int)StatusEnum.FailedProcessing ||
				d.Status == (int)StatusEnum.NoStatus) && d.Tranzactii != null &&
				!d.Tranzactii.Any(t => t.ConexiuniConturi.HybridId == hybridId))
				.Select(doc => new Documente
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
				})
				.AsEnumerable();
		}

		public IEnumerable<Documente> GetAllDocumenteOperatii(Guid hybridId)
		{
			return _context.Documente.Include(d => d.FisiereDocumente)
				.Where(d => d.ConexiuniConturi.HybridId == hybridId && (d.Status == (int)StatusEnum.Approved ||
				d.Status == (int)StatusEnum.Rejected ||
				d.Status == (int)StatusEnum.WaitingForApproval) && d.Tranzactii != null &&
				!d.Tranzactii.Any(t => t.ConexiuniConturi.HybridId == hybridId))
				.Select(doc => new Documente
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
				})
				.AsEnumerable();
		}

		public IEnumerable<Tranzactii> GetAllTranzactiiTransfer(Guid hybridId)
		{
			return _context.Tranzactii.Include(t => t.Documente.NextConexiuniConturi.ProfilCont)
				.Where(d => d.ConexiuniConturi.HybridId == hybridId && d.Type == (int)TranzactionTypeEnum.Transfer)
				.AsEnumerable();
		}

		public object GetDashboardInfo(Guid hybridId)
		{
			var tableDocs = _context.Documente
				.Where(d => d.ConexiuniConturi.HybridId == hybridId &&
				(d.Status == (int)StatusEnum.Approved ||
				d.Status == (int)StatusEnum.Rejected ||
				d.Status == (int)StatusEnum.WaitingForApproval))
				.OrderByDescending(doc => doc.Uploaded)
				.Select(doc => new Documente
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
				})
				.Take(5).AsEnumerable();

			// curr date
			var currentDate = DateTime.UtcNow;
			var currentMonth = currentDate.Month;
			var currentYear = currentDate.Year;
			var previousMonth = currentDate.AddMonths(-1).Month;
			var previousYear = currentDate.AddMonths(-1).Year;

			// this month
			var countDocUpThisMth = _context.Documente.Where(d => d.ConexiuniConturi.HybridId == hybridId &&
				(d.Status == (int)StatusEnum.Approved ||
				d.Status == (int)StatusEnum.Rejected ||
				d.Status == (int)StatusEnum.WaitingForApproval) &&
				d.Uploaded.Month == currentMonth && d.Uploaded.Year == currentYear
				&& d.Tranzactii != null &&
				!d.Tranzactii.Any(t => t.Type == (int)TranzactionTypeEnum.Transfer && t.ConexiuniConturi.HybridId == hybridId))
				.Count();
			var countPtsRcvdThisMth = _context.Documente.Where(d => d.ConexiuniConturi.HybridId == hybridId &&
				d.Status == (int)StatusEnum.Approved &&
				d.Uploaded.Month == currentMonth && d.Uploaded.Year == currentYear
				&& d.Tranzactii != null &&
				!d.Tranzactii.Any(t => t.Type == (int)TranzactionTypeEnum.Transfer && t.ConexiuniConturi.HybridId == hybridId))
				.Select(doc => doc.DiscountValue).Sum();
			var countPtsSpentThisMonth = _context.Tranzactii.Where(d => d.ConexiuniConturi.HybridId == hybridId &&
				d.Type == (int)TranzactionTypeEnum.Transfer &&
				d.Created.Month == currentMonth && d.Created.Year == currentYear)
				.Select(doc => doc.Amount).Sum();
			// last month
			var countDocUpLastMth = _context.Documente.Where(d => d.ConexiuniConturi.HybridId == hybridId &&
				(d.Status == (int)StatusEnum.Approved ||
				d.Status == (int)StatusEnum.Rejected ||
				d.Status == (int)StatusEnum.WaitingForApproval) &&
				d.Uploaded.Month == previousMonth && d.Uploaded.Year == previousYear
				&& d.Tranzactii != null &&
				!d.Tranzactii.Any(t => t.Type == (int)TranzactionTypeEnum.Transfer && t.ConexiuniConturi.HybridId == hybridId))
				.Count();
			var countPtsRcvdLastMth = _context.Documente.Where(d => d.ConexiuniConturi.HybridId == hybridId &&
				d.Status == (int)StatusEnum.Approved &&
				d.Uploaded.Month == previousMonth && d.Uploaded.Year == previousYear
				&& d.Tranzactii != null &&
				!d.Tranzactii.Any(t => t.Type == (int)TranzactionTypeEnum.Transfer && t.ConexiuniConturi.HybridId == hybridId))
				.Select(doc => doc.DiscountValue).Sum();
			var countPtsSpentLastMonth = _context.Tranzactii.Where(d => d.ConexiuniConturi.HybridId == hybridId &&
				d.Type == (int)TranzactionTypeEnum.Transfer &&
				d.Created.Month == previousMonth && d.Created.Year == previousYear)
				.Select(doc => doc.Amount).Sum();

			var monthlyAnalitics = new List<object>();
			for (int i = 1; i <= 12; i++)
			{
				monthlyAnalitics.Add(new
				{
					Label = DateTime.Parse($"2000-{(i < 9 ? $"0{i}" : i)}-{01}").ToString("MMM"),
					Value = _context.Documente.Where(d => d.ConexiuniConturi.HybridId == hybridId &&
								d.Status == (int)StatusEnum.Approved &&
								d.Uploaded.Month == i && d.Uploaded.Year == currentYear
								&& d.Tranzactii != null &&
								!d.Tranzactii.Any(t => t.Type == (int)TranzactionTypeEnum.Transfer && t.ConexiuniConturi.HybridId == hybridId))
								.Select(doc => doc.DiscountValue).Sum(),
				});
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

		public Documente GetDocument(Guid entityId)
		{
			return _context.Documente.Include(d => d.FisiereDocumente)
				.First(d => d.Id == entityId);
		}

		public Guid GetMyHybridId(Guid conexId)
		{
			return _context.ConexiuniConturi.Find(conexId)?.HybridId ?? Guid.Empty;
		}
		private async Task<bool> AddCommonEntity<TEntity>(TEntity entity)
		{
			_context.Add(entity);
			return await SaveChangesAsync();
		}
		private async Task<bool> UpdateCommonEntity<TEntity>(TEntity entity)
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