using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Table.Queryable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DocTracker.Controllers {
   [Route("api/[controller]")]
   [ApiController]
   public class TrackDocEventsController : ControllerBase {

      private readonly ILogger<WeatherForecastController> _logger;
      private readonly IConfiguration _config;


      public TrackDocEventsController(ILogger<WeatherForecastController> logger, IConfiguration configuration) {
         _logger = logger;
         _config = configuration;
      }



      /*
      // GET: api/<TrackDocUploadController>
      [HttpGet]
      public IEnumerable<string> Get() {
         return new string[] { "value1", "value2" };
      }

      // GET api/<TrackDocUploadController>/5
      [HttpGet("{id}")]
      public string Get(int id) {
         return "value";
      }
      */


      // POST api/<TrackDocUploadController>
      [HttpPost]
      public string Post([FromBody] BatchEventsBody body) {
         var mytable = Utils.CreateTable(_config["TableAccount"], 
                                         _config["TableName"], 
                                         _config["TableSASToken"]);
         /*
         var rec1 = new DocEntity {
            PartitionKey = GlobConst.DocIDKeyword,
            RowKey = body.UniqueID,
            Category = body.Category,
            Name = body.Name,
            Size = body.Size,
            Event = GlobConst.UploadEvent,
            UniqueID = body.UniqueID,
            ExternalID = body.ExternalID,
         };
         var rec2 = new DocEntity {
            PartitionKey = GlobConst.EventKeyword,
            RowKey = DateTime.UtcNow.InverseTicks(), // we need to reverse time to get descending sort
            Category = body.Category,
            Name = body.Name,
            Size = body.Size,
            Event = GlobConst.UploadEvent,
            UniqueID = body.UniqueID,
            ExternalID = body.ExternalID,
         };
         */
         DocEntity rec1 = null;
         foreach (DocUploadBody entry in body.entries) {
            rec1 = new DocEntity {
               PartitionKey = GlobConst.DocIDKeyword,
               RowKey = entry.UniqueID,
               Category = entry.Category,
               Name = entry.Name,
               Size = entry.Size,
               Event = GlobConst.UploadEvent,
               UniqueID = entry.UniqueID,
               ExternalID = entry.ExternalID,
            };
         }
         var batch = new TableBatchOperation();
         batch.Insert(rec1);
         var ins_result = mytable.ExecuteBatch(batch);
         var ins_list = ins_result.ToList();
         /*
         batch = new TableBatchOperation();
         batch.Insert(rec2);
         ins_result = mytable.ExecuteBatch(batch);
         ins_list = ins_result.ToList();
         */
         return $"Uploaded {ins_list.Count} records";
      }

      /*
      // PUT api/<TrackDocUploadController>/5
      [HttpPut("{id}")]
      public void Put(int id, [FromBody] string value) {
      }

      // DELETE api/<TrackDocUploadController>/5
      [HttpDelete("{id}")]
      public void Delete(int id) {
      }
      */
   }
}
