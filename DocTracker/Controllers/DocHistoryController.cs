using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table.Queryable;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DocTracker.Controllers {
   [Route("api/[controller]")]
   [ApiController]
   public class DocHistoryController : ControllerBase {


      private readonly ILogger<DocHistoryController> _logger;
      private readonly IConfiguration _config;


      public DocHistoryController(ILogger<DocHistoryController> logger, IConfiguration configuration) {
         _logger = logger;
         _config = configuration;
      }


      /*
      // GET: api/<DocHistoryController>
      [HttpGet]
      public IEnumerable<string> Get() {
         return new string[] { "value1", "value2" };
      }

      // GET api/<DocHistoryController>/5
      [HttpGet("{id}")]
      public string Get(int id) {
         return "value";
      }
      */

      /// <summary>
      /// Creates a report with all the operations performed on a document
      /// </summary>
      /// <remarks>
      /// You just need to enter the UniqueID in the request body. <br /> 
      /// Please enclose the ID in double quotes, e.g. "Doc432a"
      /// </remarks>
      // POST api/<DocHistoryController>
      [HttpPost]
      public IActionResult Post([FromBody] string UniqueID) {
         if (string.IsNullOrWhiteSpace(UniqueID))
            return BadRequest($"Please enter a valid UniqueID");
         // let's connect with the database on Azure
         var mytable = Utils.CreateTable(_config["TableAccount"],
                                         _config["TableName"],
                                         _config["TableSASToken"]);
         // we try to get the requested entity from db
         var db_entity = 
            Utils.RetrieveEntityUsingPointQueryAsync<DocEntity>(mytable, GlobConst.DocIDKeyword, UniqueID).Result;
         if (db_entity == null)
            return NotFound($"UniqueID '{UniqueID}' was not found, double check it is correct and try again");
         // we got the first doc upload in history, now we query all subsequent processing operations
         var myquery = mytable.CreateQuery<DocEntity>()
            .Where(r => r.PartitionKey == GlobConst.ProcessingKeyword &&  
                   string.Compare(r.RowKey, db_entity.RowKey) <= 0 &&     
                   r.UniqueID == db_entity.UniqueID); 
         var qresult = mytable.ExecuteQuery(myquery.AsTableQuery()).ToList();
         // let's add the original upload to all the processing events and then sort in the proper order
         var sorted_result = qresult.Append(db_entity).Reverse();
         return Ok(sorted_result);
      }

      /*
      // PUT api/<DocHistoryController>/5
      [HttpPut("{id}")]
      public void Put(int id, [FromBody] string value) {
      }

      // DELETE api/<DocHistoryController>/5
      [HttpDelete("{id}")]
      public void Delete(int id) {
      }
      */
   }
}
