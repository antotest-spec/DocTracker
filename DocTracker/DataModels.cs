using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocTracker {
   // maps the body payload coming with the doc uploaded POST request 
   public class DocEntity : TableEntity {
      public string EventType { get; set; }
      public string UniqueID { get; set; }
      public string ExternalID { get; set; }
      public string UploadName { get; set; }
      public string UploadCategory { get; set; }
      public Int64 UploadSize { get; set; }
      public string ProcessingType { get; set; }
      public Int64 ProcessingTimeInSeconds { get; set; }
      public string ProcessingResultDescr { get; set; }

      public string Validate() {
         var retstr = new StringBuilder();
         string[] valid_events =  { GlobConst.UploadKeyword, GlobConst.ProcessingKeyword };
         var valid_events_ndx = Array.FindIndex(valid_events, ev => ev.Equals(EventType, StringComparison.OrdinalIgnoreCase));
         if (valid_events_ndx == -1)
            retstr.Append($"Unknown EventType \"{EventType}\", valid values are: \"{string.Join("/", valid_events)}\" ");
         else if (valid_events_ndx == 0 &&
            Enumerable.Any(new string[] {UploadName, UploadCategory, ExternalID}, string.IsNullOrWhiteSpace))
               retstr.Append($"Please provide a value for UploadName, UploadCategory and ExternalID ");
         else if (valid_events_ndx == 1 &&
            Enumerable.Any(new string[] { ProcessingType, ProcessingResultDescr }, string.IsNullOrWhiteSpace))
              retstr.Append($"Please provid a value for ProcessingType and ProcessingResultDescr ");
         if (Enumerable.Any(new string[] { UniqueID }, string.IsNullOrWhiteSpace))
            retstr.Append($"Please provide a value for UniqueID");
         return retstr.ToString();
      }

      public string ValidateWithNdx(int ndx) {
         string result = Validate();
         return string.IsNullOrEmpty(result) ? result : $"Entry {ndx+1} {EventType} {UniqueID}: {result}";
      }

      public bool ExistsInDB(CloudTable table) {
         var db_entity = 
            Utils.RetrieveEntityUsingPointQueryAsync<DocEntity>(table, PartitionKey, RowKey).Result;
         return db_entity != null;
      }

   }

   public class BatchEventsBody {
      public DocEntity[] entries { get; set; }
   }


   public class UploadsReportBody {
      public Int64 HowMany { get; set; }
      public string SpecifyUploadOrSecond{ get; set; }
   }

   public class UploadsReportCategoryResult {
      public string Category { get; set; }
      public DocEntity[] entries { get; set; }
      public int NumDocumentsInThisCategory { get; set; }
      public Int64 TotalSizeInThisCategory { get; set; }
   }

   public class UploadsReportResult {
      public UploadsReportCategoryResult[] categories { get; set; }
      public int NumDocumentsInAllCategories { get; set; }
      public Int64 TotalSizeInAllCategories { get; set; }
   }

   // constants hardcoded in the sources and used across multiple controllers
   public class GlobConst {
      public static readonly string EventKeyword = "Event";
      public static readonly string UploadKeyword = "Upload";
      public static readonly string ProcessingKeyword = "Processing";
      public static readonly string DocIDKeyword = "DocID";
      public static readonly string SecondsKeyword = "Second";
   }


   // this is the Class representing (mapping) a record row in the database
   // it includes all possible fields (columns) used in the database 
   // since we are using a nosql db (Azure table storge)
   // data is stored denormalized and can be redundant across rows to optimize query performance
   /*
   public class DocEntity : TableEntity {
      public DocEntity() {
      }

      public DocEntity(string partition_key, string row_key) {
         PartitionKey = partition_key;
         RowKey = row_key;
      }

      public string  Name { get; set; }
      public string  Category { get; set; }
      public Int64   Size { get; set; }
      public string  Event { get; set; }
      public string UniqueID { get; set; }
      public string ExternalID { get; set; }
   }
   */


}
