﻿using LW.BkEndDb;
using LW.BkEndModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LW.BkEndLogic.RegularUser
{
	public interface IDbRepoUser
	{
		IEnumerable<Documente> GetAllDocumente(Guid conexId);
		Documente GetDocument(Guid entityId);
		IEnumerable<DataProcDocs> GetAllDataProcDocs(Guid conexId);
		DataProcDocs GetDataProcDoc(Guid entityId);
	}
	public class DbRepoUser : IDbRepoUser
	{
		private readonly LwDBContext _context;
		public DbRepoUser(LwDBContext context)
		{
			_context = context;
		}

		public IEnumerable<DataProcDocs> GetAllDataProcDocs(Guid conexId)
		{
			return _context.DataProcDocs.Include(d => d.FisiereDocumente)
				.Where(d => d.ConexId == conexId)
				.AsEnumerable();
		}

		public IEnumerable<Documente> GetAllDocumente(Guid conexId)
		{
			return _context.Documente.Include(d => d.FisiereDocumente)
				.Where(d => d.ConexId == conexId)
				.AsEnumerable();
		}

		public DataProcDocs GetDataProcDoc(Guid entityId)
		{
			return _context.DataProcDocs.Include(d => d.FisiereDocumente).First(d => d.Id == entityId);
		}

		public Documente GetDocument(Guid entityId)
		{
			return _context.Documente.Include(d => d.FisiereDocumente).First(d => d.Id == entityId);
		}

	}
}