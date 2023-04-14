using LW.BkEndDb;
using LW.BkEndModel;
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
				.Where(d => d.ConexId == conexId && d.isWithdraw)
				.AsEnumerable();
		}
		public IEnumerable<Tranzactii> GetAllTranzactiiTransfer(Guid conexId)
		{
			return _context.Tranzactii
				.Where(d => d.ConexId == conexId && !d.isWithdraw)
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

	}
}
