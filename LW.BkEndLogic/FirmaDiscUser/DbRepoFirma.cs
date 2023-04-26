using LW.BkEndDb;
using LW.BkEndModel;
using LW.BkEndModel.Enums;
using Microsoft.EntityFrameworkCore;

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
			return _context.Documente.Include(d => d.FisiereDocumente).Include(d => d.ConexiuniConturi.ProfilCont)
				.Where(d => d.FirmaDiscountId == conex.FirmaDiscountId && d.Status == (int)StatusEnum.WaitingForApproval)
				.AsEnumerable();
		}
		public Documente GetDocument(Guid entityId)
		{
			return _context.Documente.Include(d => d.FisiereDocumente).First(d => d.Id == entityId);
		}
		public async Task<bool> UpdateDocStatusAsync(Documente documente, StatusEnum status)
		{
			documente.Status = (int)status;
			documente.StatusName = Enum.GetName(typeof(StatusEnum), status);
			return await UpdateCommonEntity(documente);
		}
		public object GetDashboardInfo(Guid conexId)
		{
			var conex = _context.ConexiuniConturi.Find(conexId);
			if (conex == null)
				return null;

			var tableDocs = _context.Documente
				.Where(d => d.FirmaDiscountId == conex.FirmaDiscountId &&
				(d.Status == (int)StatusEnum.Approved ||
				d.Status == (int)StatusEnum.Rejected ||
				d.Status == (int)StatusEnum.WaitingForApproval))
				.Select(doc => new Documente
				{
					Id = doc.Id,
					DocNumber = doc.DocNumber,
					Status = doc.Status,
					Uploaded = doc.Uploaded,
					StatusName = doc.StatusName,
					DiscountValue = doc.DiscountValue,
					Total = doc.Total,
					ExtractedBusinessData = doc.ExtractedBusinessData,
				})
				.OrderByDescending(doc => doc.Uploaded)
				.Take(5).AsEnumerable();

			// curr date
			var currentDate = DateTime.UtcNow;
			var currentMonth = currentDate.Month;
			var currentYear = currentDate.Year;
			var previousMonth = currentDate.AddMonths(-1).Month;
			var previousYear = currentDate.AddMonths(-1).Year;

			// this month
			var countDocUpThisMth = _context.Documente.Where(d => d.FirmaDiscountId == conex.FirmaDiscountId &&
				(d.Status == (int)StatusEnum.Approved ||
				d.Status == (int)StatusEnum.Rejected ||
				d.Status == (int)StatusEnum.WaitingForApproval) &&
				d.Uploaded.Month == currentMonth && d.Uploaded.Year == currentYear)
				.Count();
			var countPtsRejThisMth = _context.Documente.Where(d => d.FirmaDiscountId == conex.FirmaDiscountId &&
				d.Status == (int)StatusEnum.Rejected &&
				d.Uploaded.Month == currentMonth && d.Uploaded.Year == currentYear)
				.Select(doc => doc.DiscountValue).Sum();
			var countPtsAprThisMth = _context.Documente.Where(d => d.FirmaDiscountId == conex.FirmaDiscountId &&
				d.Status == (int)StatusEnum.Approved &&
				d.Uploaded.Month == currentMonth && d.Uploaded.Year == currentYear)
				.Select(doc => doc.DiscountValue).Sum();

			// last month
			var countDocUpLastMth = _context.Documente.Where(d => d.FirmaDiscountId == conex.FirmaDiscountId &&
				(d.Status == (int)StatusEnum.Approved ||
				d.Status == (int)StatusEnum.Rejected ||
				d.Status == (int)StatusEnum.WaitingForApproval) &&
				d.Uploaded.Month == previousMonth && d.Uploaded.Year == previousYear
				&& d.Tranzactii != null && !d.Tranzactii.Any(t => t.Type == (int)TranzactionTypeEnum.Transfer && t.ConexId == conexId))
				.Count();
			var countPtsRejLastMth = _context.Documente.Where(d => d.FirmaDiscountId == conex.FirmaDiscountId &&
				d.Status == (int)StatusEnum.Rejected &&
				d.Uploaded.Month == previousMonth && d.Uploaded.Year == previousYear)
				.Select(doc => doc.DiscountValue).Sum();
			var countPtsAprLastMth = _context.Documente.Where(d => d.FirmaDiscountId == conex.FirmaDiscountId &&
				d.Status == (int)StatusEnum.Approved &&
				d.Uploaded.Month == previousMonth && d.Uploaded.Year == previousYear)
				.Select(doc => doc.DiscountValue).Sum();

			var monthlyAnalitics = new List<object>();
			for (int i = 1; i <= 12; i++)
			{
				monthlyAnalitics.Add(new
				{
					Label = DateTime.Parse($"2000-{(i < 9 ? $"0{i}" : i)}-{01}").ToString("MMM"),
					Value = _context.Documente.Where(d => d.FirmaDiscountId == conex.FirmaDiscountId &&
								d.Status == (int)StatusEnum.Approved &&
								d.Uploaded.Month == i && d.Uploaded.Year == currentYear)
								.Select(doc => doc.DiscountValue).Sum(),
				});
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
	}
}
