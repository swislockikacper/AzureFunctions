using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Dapper;
using System.IO;
using Company.Models;
using Company.Extensions;
using System.Data.SqlClient;
using System;

namespace Company.Function
{
    public static class BlobFilesOrchestrator
    {
        [FunctionName("BlobFilesOrchestrator")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            await context.CallActivityAsync("SaveData", context.GetInput<BlobFile>());
        }

        [FunctionName("SaveData")]
        public static async Task SaveData([ActivityTrigger]BlobFile blob)
        {
            var insert = $"INSERT INTO [dbo].[File] ([FileName], [Extension], [Path], [Size]) " +
                $"VALUES (@FileName, @Extension, @Path, @Size)";

            var FileName = blob.FileName;
            var Extension = blob.Extension;
            var Path = blob.Path;
            var Size = blob.Size;

            using (var connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                await connection.ExecuteAsync(insert, new
                {
                    FileName,
                    Extension,
                    Path,
                    Size
                });
            }
        }

        [FunctionName("BlobFilesOrchestrator_Start")]
        public static async Task Start(
            [BlobTrigger("files/{name}", Connection = "BlobConnectionString")]Stream blob, string name,
            [OrchestrationClient]DurableOrchestrationClient starter)
            => await starter.StartNewAsync("BlobFilesOrchestrator",
                new BlobFile
                {
                    FileName = name,
                    Extension = name.GetFileExtension(),
                    Path = $"files/{name}",
                    Size = blob.Length
                });
    }
}