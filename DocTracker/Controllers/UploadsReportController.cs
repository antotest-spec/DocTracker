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
   public class UploadsReportController : ControllerBase {


      private readonly ILogger<UploadsReportController> _logger;
      private readonly IConfiguration _config;


      public UploadsReportController(ILogger<UploadsReportController> logger, IConfiguration configuration) {
         _logger = logger;
         _config = configuration;
      }



      /*
      // GET: api/<UploadsReportController>
      [HttpGet]
      public IEnumerable<string> Get() {
         return new string[] { "value1", "value2" };
      }

      // GET api/<UploadsReportController>/5
      [HttpGet("{id}")]
      public string Get(int id) {
         return "value";
      }
      */

      // POST api/<UploadsReportController>
      [HttpPost]
      public IActionResult Post([FromBody] UploadsReportBody body) {
         // validate body fields and reject bad requests
         if (body.HowMany <= 0)
            return BadRequest($"Field HowMany must be greater than zero");
         string[] valid_selector = { GlobConst.SecondsKeyword, GlobConst.UploadKeyword};
         var valid_selector_ndx = Array.FindIndex (valid_selector, x => x.Equals(body.SpecifyUploadOrSecond, StringComparison.OrdinalIgnoreCase));
         if (valid_selector_ndx == -1)
            return BadRequest($"Wrong SpecifyUploadOrSecond field, current value is: \"{body.SpecifyUploadOrSecond}\", valid values are: \"{string.Join("/", valid_selector)}\" ");
         // let's connect with the database on Azure
         var mytable = Utils.CreateTable(_config["TableAccount"],
                                         _config["TableName"],
                                         _config["TableSASToken"]);

         var qresult = new List<DocEntity>();
         if (valid_selector_ndx == 0) { // caller wants a time based report (show all uploads in past "X" seconds)
            var start_time = DateTime.UtcNow.AddSeconds(-body.HowMany).ReverseTicks();
            var start_time_str = $"{start_time:X16}";
            var myquery = mytable.CreateQuery<DocEntity>()
               .Where(r => r.PartitionKey == GlobConst.UploadKeyword &&
                      string.Compare(r.RowKey, start_time_str) <= 0);
            qresult = mytable.ExecuteQuery(myquery.AsTableQuery()).ToList();
         }
         else {  // caller wants the past "X" uploads
            // the query is simple, since the upload partition is sorted in decending time order 
            // we just need the top "X" records from the upload partition
            var myquery = mytable.CreateQuery<DocEntity>()
               .Where(r => r.PartitionKey == GlobConst.UploadKeyword)
               .Take((int)body.HowMany);
            qresult = mytable.ExecuteQuery(myquery.AsTableQuery()).ToList();
         }
         // let's group results by category
         var cat_result = qresult.GroupBy(x => x.UploadCategory);
         // now we fill the final report fields
         var report = new UploadsReportResult();
         report.categories = new UploadsReportCategoryResult[cat_result.Count()];
         int cont_cat = 0;
         foreach(var cur_category in cat_result) {
            report.categories[cont_cat] = new UploadsReportCategoryResult();
            report.categories[cont_cat].Category = cur_category.Key;
            report.categories[cont_cat].NumDocumentsInThisCategory = cur_category.Count();
            report.categories[cont_cat].entries = cur_category.ToArray();
            report.categories[cont_cat++].TotalSizeInThisCategory = cur_category.Sum(e => e.UploadSize);
         }
         report.NumDocumentsInAllCategories = report.categories.Sum(cat => cat.NumDocumentsInThisCategory);
         report.TotalSizeInAllCategories = report.categories.Sum(cat => cat.TotalSizeInThisCategory);
         return Ok(report);
      }

      /*
      // PUT api/<UploadsReportController>/5
      [HttpPut("{id}")]
      public void Put(int id, [FromBody] string value) {
      }

      // DELETE api/<UploadsReportController>/5
      [HttpDelete("{id}")]
      public void Delete(int id) {
      }
      */
   }
}
