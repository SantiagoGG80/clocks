using clocks.Common.Models;
using clocks.Common.Responses;
using clocks.Functions.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace clocks.Functions.Functions
{
    public static class TodoApi
    {
        [FunctionName(nameof(CreateTodo))]
        public static async Task<IActionResult> CreateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequest req,
            [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log)
        {
            log.LogInformation("Recieved a new record.");


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Todo todo = JsonConvert.DeserializeObject<Todo>(requestBody);

            if (string.IsNullOrEmpty((todo?.IdEmployee).ToString()))
            {
                return new BadRequestObjectResult(new Responses
                {
                    IsSuccess = false,
                    Message = "The employee must have id."
                });
            }


            if (todo.Type < 0 || todo.Type > 1)
            {
                return new BadRequestObjectResult(new Responses
                {
                    IsSuccess = false,
                    Message = "Invalid record type. 0 Entry, 1 Exit"
                });
            }

            TodoEntity todoEntity = new TodoEntity
            {
                EntryExit = DateTime.UtcNow,
                ETag = "*",
                Consolidated = false,
                PartitionKey = "CLOCK",
                RowKey = Guid.NewGuid().ToString(),
                IdEmployee = todo.IdEmployee,
                Type = todo.Type
            };

            TableOperation addOperation = TableOperation.Insert(todoEntity);
            await todoTable.ExecuteAsync(addOperation);

            string message = "New record stored in table";
            log.LogInformation(message);


            return new OkObjectResult(new Responses
            {
                IsSuccess = true,
                Message = message,
                Result = todoEntity,

            });
        }
    }
}
