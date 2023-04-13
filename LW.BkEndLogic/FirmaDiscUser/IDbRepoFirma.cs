using LW.BkEndDb;
using LW.BkEndModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LW.BkEndLogic.FirmaDiscUser
{
	public interface IDbRepoFirma
	{
		IEnumerable<Documente> GetAllDocumente(Guid firmaId);
		Documente GetDocument(Guid entityId);
	}
	public class DbRepoFirma : IDbRepoFirma
	{
		private readonly LwDBContext _context;
		public DbRepoFirma(LwDBContext context)
		{
			_context = context;
		}
		public IEnumerable<Documente> GetAllDocumente(Guid firmaId)
		{
			return _context.Documente.Include(d => d.FisiereDocumente)
				.Where(d => d.FirmaDiscountId == firmaId)
				.AsEnumerable();
		}
		public Documente GetDocument(Guid entityId)
		{
			return _context.Documente.Include(d => d.FisiereDocumente).First(d => d.Id == entityId);
		}
	}
}
