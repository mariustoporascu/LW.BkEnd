using LW.BkEndDb;
using LW.DocProcLogic.DbRepo;
using LW.DocProcLogic.MicrosoftOcr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
	.ConfigureFunctionsWorkerDefaults()
	.ConfigureServices(services =>
	{
		services.AddDbContext<LwDBContext>(options =>
		{
			options.UseSqlServer(Environment
				.GetEnvironmentVariable("DbConnection"),
				sqlServerOptionsAction =>
				{
					sqlServerOptionsAction.EnableRetryOnFailure(
						maxRetryCount: 10,
						maxRetryDelay: TimeSpan.FromSeconds(10),
						errorNumbersToAdd: null);
				}
			);
		});
		services.AddScoped<IOcrPrebuilt, OcrPrebuilt>();
		services.AddScoped<IDbRepo, DbRepo>();
	})
	.Build();

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(Environment.GetEnvironmentVariable("Syncfusion"));

host.Run();
