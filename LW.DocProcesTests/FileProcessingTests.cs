using LW.BkEndDb;
using LW.BkEndModel.Enums;
using LW.BkEndModel;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace LW.DocProcesTests
{
    public class FileProcessingTests
    {
        // dev local
        public static string DbConnString =
            "Data Source=.;Initial Catalog=lwins;Integrated Security=true;TrustServerCertificate=true;";
        private readonly HttpClient _client;
        private readonly string _path = "/runtime/webhooks/EventGrid?functionName=OnNewFileV2";
        private readonly ITestOutputHelper _output;

        public FileProcessingTests(ITestOutputHelper output)
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri("http://localhost:7288");
            _client.DefaultRequestHeaders.Add("aeg-event-type", "Notification");
            _output = output;
        }

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

            var docs = context.Documente
                .Include(d => d.FisiereDocumente)
                .Where(d => d.Status == (int)StatusEnum.NoStatus);
            var jsonString =
                @"[{
  ""topic"": ""/subscriptions/{subscription-id}/resourceGroups/Storage/providers/Microsoft.Storage/storageAccounts/my-storage-account"",
  ""subject"": ""/blobServices/default/containers/test-container/blobs/new-file.txt"",
  ""eventType"": ""Microsoft.Storage.BlobCreated"",
  ""eventTime"": ""2017-06-26T18:41:00.9584103Z"",
  ""id"": ""831e1650-001e-001b-66ab-eeb76e069631"",
  ""data"": {
    ""api"": ""PutBlockList"",
    ""clientRequestId"": ""6d79dbfb-0e37-4fc4-981f-442c9ca65760"",
    ""requestId"": ""831e1650-001e-001b-66ab-eeb76e000000"",
    ""eTag"": ""\""0x8D4BCC2E4835CD0\"""",
    ""contentType"": ""text/plain"",
    ""contentLength"": 524288,
    ""blobType"": ""BlockBlob"",
    ""url"": ""http://127.0.0.1:10000/devstoreaccount1/uploadsblobdev/c83d3b68-87f8-46cb-bb54-1dbd241618c2"",
    ""sequencer"": ""00000000000004420000000000028963"",
    ""storageDiagnostics"": {
      ""batchId"": ""b68529f3-68cd-4744-baa4-3c0498ec19f0""
    }
  },
  ""dataVersion"": """",
  ""metadataVersion"": ""1""
}]";
            foreach (var doc in docs)
            {
                var newJsonString = jsonString.Replace(
                    "c83d3b68-87f8-46cb-bb54-1dbd241618c2",
                    doc.FisiereDocumente.Identifier
                );
                var content = new StringContent(newJsonString, Encoding.UTF8, "application/json");
                var response = await _client.PostAsync(_path, content);
                _output.WriteLine(response.StatusCode.ToString());
            }
        }
    }
}
