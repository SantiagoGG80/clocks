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
        [FunctionName(nameof(CreateRecord))]
        public static async Task<IActionResult> CreateRecord(
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
                EntryExit = todo.EntryExit,
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

        [FunctionName(nameof(UpdateRecord))]
        public static async Task<IActionResult> UpdateRecord(
           [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todo/{id}")] HttpRequest req,
           [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
           string id,
           ILogger log)
        {
            log.LogInformation($"Update for record: {id}, received");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Todo todo = JsonConvert.DeserializeObject<Todo>(requestBody);

            // Validate todo id
            TableOperation findOperation = TableOperation.Retrieve<TodoEntity>("CLOCK", id);
            TableResult findResult = await todoTable.ExecuteAsync(findOperation);
            if (findResult.Result == null)
            {
                return new BadRequestObjectResult(new Responses
                {
                    IsSuccess = false,
                    Message = "Record not found."
                });
            }

            // Update todo
            TodoEntity todoEntity = (TodoEntity)findResult.Result;
            todoEntity.Type = todo.Type;
            todoEntity.Consolidated = todo.Consolidated;
            if (todo.IdEmployee != 0)
            {
                todoEntity.IdEmployee = todo.IdEmployee;
            }

            TableOperation addOperation = TableOperation.Replace(todoEntity);
            await todoTable.ExecuteAsync(addOperation);

            string message = $"Record: {id}, update in table";
            log.LogInformation(message);


            return new OkObjectResult(new Responses
            {
                IsSuccess = true,
                Message = message,
                Result = todoEntity,

            });
        }

        [FunctionName(nameof(GetAllRecord))]
        public static async Task<IActionResult> GetAllRecord(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo")] HttpRequest req,
           [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
           ILogger log)
        {
            log.LogInformation("get all records received.");

            TableQuery<TodoEntity> query = new TableQuery<TodoEntity>();
            TableQuerySegment<TodoEntity> todos = await todoTable.ExecuteQuerySegmentedAsync(query, null);


            string message = "Retrieved all records.";
            log.LogInformation(message);


            return new OkObjectResult(new Responses
            {
                IsSuccess = true,
                Message = message,
                Result = todos,

            });
        }

        [FunctionName(nameof(GetRecordById))]
        public static IActionResult GetRecordById(
          [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo/{id}")] HttpRequest req,
          [Table("todo", "CLOCK", "{id}", Connection = "AzureWebJobsStorage")] TodoEntity todoEntity,
          string id,
          ILogger log)
        {
            log.LogInformation($"get record by id: {id} received");

            if (todoEntity == null)
            {
                return new BadRequestObjectResult(new Responses
                {
                    IsSuccess = false,
                    Message = "Record not found."
                });
            }

            string message = $"Record: {todoEntity.RowKey}, retrieved.";
            log.LogInformation(message);


            return new OkObjectResult(new Responses
            {
                IsSuccess = true,
                Message = message,
                Result = todoEntity,

            });
        }

        [FunctionName(nameof(DeleteRecord))]
        public static async Task<IActionResult> DeleteRecord(
         [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todo/{id}")] HttpRequest req,
         [Table("todo", "CLOCK", "{id}", Connection = "AzureWebJobsStorage")] TodoEntity todoEntity,
         [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
         string id,
         ILogger log)
        {
            log.LogInformation($"Delete record: {id}, received.");

            if (todoEntity == null)
            {
                return new BadRequestObjectResult(new Responses
                {
                    IsSuccess = false,
                    Message = "Record not found."
                });
            }

            await todoTable.ExecuteAsync(TableOperation.Delete(todoEntity));
            string message = $"Record: {todoEntity.RowKey}, deleted.";
            log.LogInformation(message);


            return new OkObjectResult(new Responses
            {
                IsSuccess = true,
                Message = message,
                Result = todoEntity,

            });
        }

        /*[FunctionName(nameof(CreateRecordC))]
        public static async Task<IActionResult> CreateRecordC(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "time")] HttpRequest req,
            [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            [Table("Consolidated", Connection = "AzureWebJobsStorage")] CloudTable ConsolidatedTable,
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
        }*/




    }
}
