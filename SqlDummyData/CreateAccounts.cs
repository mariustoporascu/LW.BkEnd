using Bogus;
using Bogus.Extensions.Brazil;
using Bogus.Extensions.UnitedStates;
using LW.BkEndDb;
using LW.BkEndModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDummyData
{
	public class CreateAccounts
	{
		// dev local
		//public static string DbConnString = "Data Source=.;Initial Catalog=lwdevelop;Integrated Security=true;TrustServerCertificate=true;";
		// dev azure
		public static string DbConnString = "Server=tcp:lw-dbserver.database.windows.net,1433;Initial Catalog=lw-database;Persist Security Info=False;User ID=lwdbadmin;Password=Vib3r0n3;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

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

			var userFaker = new Faker<User>()
				.RuleFor(x => x.Email, x => x.Person.Email)
				.RuleFor(x => x.PhoneNumber, x => x.Person.Phone);

			await CreateMasterAdmin(userManager, context);

			bool generate = true;
			int generatedCount = 0;
			while (generate)
			{
				if (generatedCount > 100)
				{
					generate = false;
					break;
				}
				var userFaked = userFaker.Generate();
				var user = new User
				{
					UserName = userFaked.Email,
					NormalizedUserName = userFaked.Email.ToUpper(),
					Email = userFaked.Email,
					NormalizedEmail = userFaked.Email.ToUpper(),
					PhoneNumber = userFaked.PhoneNumber,
				};

				var result = await userManager.CreateAsync(user, "testpass");
				if (result.Succeeded)
				{
					generatedCount++;

					var conexCont = GenerateConexiuniConturi(user);
					if (!await SaveEntity(conexCont, context))
						continue;

					var profilCont = GenerateProfil(conexCont, user, generatedCount);
					if (!await SaveEntity(profilCont, context))
						continue;
				}
				else
				{
					throw new Exception("Result user is null");
				}
			}
		}
		private async Task CreateMasterAdmin(UserManager<User> userManager, LwDBContext context)
		{
			try
			{
				var user = new User
				{
					Email = "sa@sa.com",
					NormalizedEmail = "sa@sa.com".ToUpper(),
					UserName = "sa@sa.com",
					NormalizedUserName = "sa@sa.com".ToUpper(),
					PhoneNumber = "0725135822",
					EmailConfirmed = true,
				};
				var result = await userManager.CreateAsync(user, "testpass");
				if (result.Succeeded)
				{

					var conexCont = GenerateConexiuniConturi(user);
					if (!await SaveEntity(conexCont, context))
						throw new Exception("Result conexCont is null");

					var profilCont = GenerateProfil(conexCont, user, 2);
					if (!await SaveEntity(profilCont, context))
						throw new Exception("Result profilCont is null");
					var firmaDiscount = GenerateFirmaDiscount(profilCont);
					if (!await SaveEntity(firmaDiscount, context))
						throw new Exception("Result firmaDiscount is null");
					conexCont.FirmaDiscountId = firmaDiscount.Id;
					if (!await UpdateEntity(conexCont, context))
						throw new Exception("Update conexCont is null");
				}
				else
				{
					throw new Exception("Result user is null");
				}

			}
			catch (Exception)
			{
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
		private async Task<bool> UpdateEntity<T>(T entity, LwDBContext context)
		{
			context.Update(entity);
			var result = await context.SaveChangesAsync();
			if (result > 0)
				return true;
			return false;
		}
		private ConexiuniConturi GenerateConexiuniConturi(User user)
		{
			var conexCont = new ConexiuniConturi
			{
				UserId = user.Id,
			};
			return conexCont;
		}
		private ProfilCont GenerateProfil(ConexiuniConturi conexCont, User user, int count)
		{
			var profilFaker = new Faker<ProfilCont>()
					.RuleFor(x => x.Name, x => x.Person.LastName)
					.RuleFor(x => x.FirstName, x => x.Person.FirstName);
			var profil = profilFaker.Generate();
			profil.Email = user.Email;
			profil.PhoneNumber = user.PhoneNumber;
			profil.IsBusiness = count % 2 == 0;
			profil.NoDocsUploaded = count;
			profil.ConexId = conexCont.Id;
			return profil;
		}
		private FirmaDiscount GenerateFirmaDiscount(ProfilCont profilCont)
		{
			var firmaDiscountFaker = new Faker<FirmaDiscount>()
						.RuleFor(x => x.Name, x => x.Company.CompanyName())
						.RuleFor(x => x.CuiNumber, x => x.Company.Ein())
						.RuleFor(x => x.NrRegCom, x => x.Company.Cnpj())
						.RuleFor(x => x.Address, x => $"{x.Person.Address.City}, {x.Person.Address.Street}");
			var firmaDiscount = firmaDiscountFaker.Generate();
			firmaDiscount.DiscountPercent = 20.0M;
			firmaDiscount.TotalGivenDiscount = 10000.0M;
			firmaDiscount.IsActive = true;
			firmaDiscount.MainContactName = $"{profilCont.FirstName} {profilCont.Name}";
			firmaDiscount.MainContactEmail = profilCont.Email;
			firmaDiscount.MainContactPhone = profilCont.PhoneNumber;

			return firmaDiscount;
		}
	}
}
