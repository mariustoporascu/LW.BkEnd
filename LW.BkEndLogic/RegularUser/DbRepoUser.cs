using LW.BkEndDb;
using LW.BkEndModel;
using LW.BkEndModel.Enums;
using Microsoft.EntityFrameworkCore;

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
				.Where(d => d.ConexId == conexId && d.Type == (int)TransferTypeEnum.Withdraw)
				.AsEnumerable();
		}
		public IEnumerable<Tranzactii> GetAllTranzactiiTransfer(Guid conexId)
		{
			return _context.Tranzactii
				.Where(d => d.ConexId == conexId && d.Type == (int)TransferTypeEnum.Transfer)
				.AsEnumerable();
		}
		public IEnumerable<Documente> GetAllDocumenteOperatii(Guid conexId)
		{
			return _context.Documente.Include(d => d.FisiereDocumente)
				.Where(d => d.ConexId == conexId && (d.Status == (int)StatusEnum.Approved ||
				d.Status == (int)StatusEnum.Rejected ||
				d.Status == (int)StatusEnum.WaitingForApproval))
				.AsEnumerable();
		}
		public IEnumerable<Documente> GetAllDocumenteFileManager(Guid conexId)
		{
			return _context.Documente.Include(d => d.FisiereDocumente)
				.Where(d => d.ConexId == conexId && (d.Status == (int)StatusEnum.Processing ||
				d.Status == (int)StatusEnum.CompletedProcessing ||
				d.Status == (int)StatusEnum.FailedProcessing ||
				d.Status == (int)StatusEnum.NoStatus))
				.AsEnumerable();
		}

		public Documente GetDocument(Guid entityId)
		{
			return _context.Documente.Include(d => d.FisiereDocumente).First(d => d.Id == entityId);
		}

		public object GetDashboardInfo(Guid conexId)
		{
			var tableDocs = _context.Documente.Where(d => d.ConexId == conexId && (d.Status == (int)StatusEnum.Approved ||
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
			var countDocUpThisMth = _context.Documente.Where(d => d.ConexId == conexId &&
				(d.Status == (int)StatusEnum.Approved ||
				d.Status == (int)StatusEnum.Rejected ||
				d.Status == (int)StatusEnum.WaitingForApproval) &&
				d.Uploaded.Month == currentMonth && d.Uploaded.Year == currentYear).Count();
			var countPtsRcvdThisMth = _context.Documente.Where(d => d.ConexId == conexId &&
				d.Status == (int)StatusEnum.Approved &&
				d.Uploaded.Month == currentMonth && d.Uploaded.Year == currentYear)
				.Select(doc => doc.DiscountValue).Sum();
			// last month
			var countDocUpLastMth = _context.Documente.Where(d => d.ConexId == conexId &&
				(d.Status == (int)StatusEnum.Approved ||
				d.Status == (int)StatusEnum.Rejected ||
				d.Status == (int)StatusEnum.WaitingForApproval) &&
				d.Uploaded.Month == previousMonth && d.Uploaded.Year == previousYear).Count();
			var countPtsRcvdLastMth = _context.Documente.Where(d => d.ConexId == conexId &&
				d.Status == (int)StatusEnum.Approved &&
				d.Uploaded.Month == previousMonth && d.Uploaded.Year == previousYear)
				.Select(doc => doc.DiscountValue).Sum();
			// TO DO 
			var countPtsSpentThisMonth = 0.00M;
			var countPtsSpentLastMonth = 0.00M;
			var monthlyAnalitics = new List<object>();
			for (int i = 1; i <= 12; i++)
			{
				monthlyAnalitics.Add(new
				{
					Label = DateTime.Parse($"2000-{(i < 9 ? $"0{i}" : i)}-{01}").ToString("MMM"),
					Value = _context.Documente.Where(d => d.ConexId == conexId &&
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
					CountPtsRcvdThisMth = countPtsRcvdThisMth,
					CountPtsSpentThisMonth = countPtsSpentThisMonth,

					CountDocUpLastMth = countDocUpLastMth,
					CountPtsRcvdLastMth = countPtsRcvdLastMth,
					CountPtsSpentLastMonth = countPtsSpentLastMonth,
				},
				MonthlyAnalitics = monthlyAnalitics
			};
		}
	}
}
