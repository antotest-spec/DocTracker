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

      private readonly ILogger<TrackDocEventsController> _logger;
      private readonly IConfiguration _config;


      public TrackDocEventsController(ILogger<TrackDocEventsController> logger, IConfiguration configuration) {
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
      public IActionResult Post([FromBody] BatchEventsBody body) {

         // we check for eventual malformed requests and if found reject them listing errors to caller so they can fix
         string validation_errors = string.Join("\n", body.entries.Select((ev, ndx) => ev.ValidateWithNdx(ndx)));
         if (!string.IsNullOrWhiteSpace(validation_errors)) 
            return BadRequest($"Invalid entries, please fix them and retry. Here are the errors:\n{validation_errors}");

         // let's connect with the database on Azure
         var mytable = Utils.CreateTable(_config["TableAccount"],
                                         _config["TableName"],
                                         _config["TableSASToken"]);
         // to optimize query performances we divide events in 3 nosql partitions: upload, processing and docid 
         // in future, using hashing techniques, we could add even more partitions to further improve performances
         var gentries = body.entries.GroupBy(e => e.EventType);
         // let's start with docid partition, this is a partition grouping upload events
         // with fast access by docid 
         var upload_entries = gentries.FirstOrDefault(g => string.Equals(g.Key, GlobConst.UploadKeyword, StringComparison.OrdinalIgnoreCase));
         if (upload_entries != null) {
            foreach (var entity in upload_entries) {
               entity.PartitionKey = GlobConst.DocIDKeyword;
               entity.RowKey = entity.UniqueID;
            }
            // we need to skip uploads events eventually repeated, only the first upload event is stored for each UniqueID
            var valid_uploads = upload_entries.Where(entity => !entity.ExistsInDB(mytable)).ToList();
            // todo: report to user if he inserted duplicates that were skipped
            // now we can insert valid upload events in db
            if (valid_uploads.Count() > 0) {
               var batch = new TableBatchOperation();
               foreach (var entity in valid_uploads)
                  batch.Insert(entity);
               var ins_result = mytable.ExecuteBatch(batch);
               // todo: check that insert was ok 

               // now we can create the upload partition, this is essentialy same entity fields contained
               // in docid partition but with different partition/row to allow faster query by date
               // (useful in the history report)
               var event_time = DateTime.UtcNow.ReverseTicks(); // we revert time to achieve descending sort 
               batch = new TableBatchOperation();
               foreach (var entity in valid_uploads) {
                  entity.PartitionKey = GlobConst.UploadKeyword;
                  entity.RowKey = $"{event_time--:X16}"; // the increment needed to make each entry unique, it's a hundred nanoseconds diff
                  batch.Insert(entity);
               }
               ins_result = mytable.ExecuteBatch(batch);
               // todo: check that insert was ok 
            }
         }
         // finally we create the 3rd and last partition, the processing events 
         var processing_entries = gentries.FirstOrDefault(g => string.Equals(g.Key, GlobConst.ProcessingKeyword, StringComparison.OrdinalIgnoreCase));
         if (processing_entries != null) {
            var event_time = DateTime.UtcNow.ReverseTicks(); // we revert time to achieve descending sort 
            var batch = new TableBatchOperation();
            // todo: check the documents processed have been previously uploaded (no zombies doc wanted)
            foreach (var entity in processing_entries) {
               entity.PartitionKey = GlobConst.ProcessingKeyword;
               entity.RowKey = $"{event_time--:X16}"; // the increment needed to make each entry unique, it's a hundred nanoseconds diff
               batch.Insert(entity);
            }
            var ins_result = mytable.ExecuteBatch(batch);
            // todo: check that insert was ok 
         }

         return Ok();
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
