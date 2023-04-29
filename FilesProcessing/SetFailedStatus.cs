using System;
using LW.BkEndModel.Enums;
using LW.DocProcLogic.DbRepo;
using LW.DocProcLogic.MicrosoftOcr;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FilesProcessing
{
	public class SetFailedStatus
	{
		private readonly ILogger _logger;
		private readonly HttpClient _httpClient;
		private readonly IOcrPrebuilt _ocrPrebuilt;
		private readonly IConfiguration _config;
		private readonly IDbRepo _dbRepo;

		public SetFailedStatus(ILoggerFactory loggerFactory, IOcrPrebuilt ocrPrebuilt, IConfiguration config, IDbRepo dbRepo)
		{
			_logger = loggerFactory.CreateLogger<OnNewFile>();
			_httpClient = new HttpClient();
			_ocrPrebuilt = ocrPrebuilt;
			_config = config;
			_dbRepo = dbRepo;
		}

		[Function("SetFailedStatus")]
		public async Task Run([QueueTrigger("webjobs-blobtrigger-poison", Connection = "AzureWebJobsStorage")] string myQueueItem)
		{
			_logger.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
			var queueObject = JsonConvert.DeserializeObject<JObject>(myQueueItem);
			if (!await _dbRepo.UpdateBlobStatus(queueObject["BlobName"].ToString(), StatusEnum.FailedProcessing)) throw new InvalidOperationException();
		}
	}
}
