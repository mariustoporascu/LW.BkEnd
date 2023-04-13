using Bogus;
using LW.BkEndDb;
using LW.BkEndModel;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bogus.Extensions;

namespace SqlDummyData
{
	public class CreateDataProc
	{
		// dev local
		public static string DbConnString = "Data Source=.;Initial Catalog=lwdevelop;Integrated Security=true;TrustServerCertificate=true;";
		// dev azure
		//public static string DbConnString = "Server=tcp:lw-dbserver.database.windows.net,1433;Initial Catalog=lw-database;Persist Security Info=False;User ID=lwdbadmin;Password=Vib3r0n3;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

		[Fact]
		public async Task AddToContext()
		{
			//add data to sql database using EF Core
			//data will be generated using bogus library
			//models used are in project LW.BkEndModel
			//context used is in project LW.BkEndDb
			//create a user manager in order to add users to the database

			var optionsBuilder = new DbContextOptionsBuilder<LwDBContext>();
			optionsBuilder.UseSqlServer(DbConnString);
			var context = new LwDBContext(optionsBuilder.Options);
			context.Database.EnsureCreated();

			var userStore = new UserStore<User, Role, LwDBContext, Guid>(context);
			var hasher = new PasswordHasher<User>();
			var lookupNormalizer = new UpperInvariantLookupNormalizer();
			var userManager = new UserManager<User>(userStore, null, hasher, null, null, lookupNormalizer, null, null, null);

			var firma = context.FirmaDiscount.FirstOrDefault();
			var regularUser = await userManager.FindByEmailAsync("office@topodvlp.website");
			var conexUser = context.ConexiuniConturi.FirstOrDefault(c => c.UserId == regularUser.Id);
			bool generate = true;
			int generatedCount = 0;
			Random random = new Random();
			while (generate)
			{
				if (generatedCount > 100)
				{
					generate = false;
					break;
				}

				// comment to enter unapproved data

				var docsFaker = new Faker<Documente>()
					.RuleFor(x => x.DocNumber, x => x.Random.AlphaNumeric(8))
					.RuleFor(x => x.Total, x => x.Random.Decimal(0.0M, 1000.0M))
					.RuleFor(x => x.IsInvoice, x => x.Random.Bool())
					.RuleFor(x => x.ReceiptId, x => x.Random.AlphaNumeric(8))
					.RuleFor(x => x.ExtractedBusinessData, x => x.Random.Words(4))
					.RuleFor(x => x.ExtractedBusinessAddress, x => x.Random.Words(4));
				var docs = docsFaker.Generate();
				docs.DiscountValue = docs.Total * firma.DiscountPercent / 100;
				docs.FirmaDiscountId = firma.Id;
				docs.Status = random.Next(0, 7);
				docs.StatusName = Enum.GetName(typeof(StatusEnum), docs.Status);
				docs.ConexId = conexUser.Id;
				if (!await SaveEntity(docs, context))
					continue;

				// end comment

				var fileInfoFaker = new Faker<FisiereDocumente>()
					.RuleFor(x => x.FileName, x => x.Random.Word())
					.RuleFor(x => x.FileExtension, x => x.Random.Word());
				var fileInfo = fileInfoFaker.Generate();
				fileInfo.DocumenteId = docs.Id;

				if (!await SaveEntity(fileInfo, context))
					continue;

				generatedCount++;
			}
		}
		private async Task<bool> SaveEntity<T>(T entity, LwDBContext context)
		{
			context.Add(entity);
			var result = await context.SaveChangesAsync();
			if (result > 0)
				return true;
			return false;
		}
	}
}
