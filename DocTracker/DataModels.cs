using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocTracker {
   // maps the body payload coming with the doc uploaded POST request 
   public class DocUploadBody {
      public string  Name { get; set; }
      public string  Category { get; set; }
      public Int64   Size { get; set; }
      public string  UniqueID { get; set; }
      public string  ExternalID { get; set; }
   }

   public class BatchEventsBody {
      public DocUploadBody[] entries { get; set; }
   }

   // constants hardcoded in the sources and used across multiple controllers
   public class GlobConst {
      public static readonly string EventKeyword = "Event";
      public static readonly string DocIDKeyword = "DocID";
      public static readonly string UploadEvent = "Upload";
   }

   // this is the Class representing (mapping) a record row in the database
   // it includes all possible fields (columns) used in the database 
   // since we are using a nosql db (Azure table storge)
   // data is stored denormalized and can be redundant across rows to optimize query performance
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


}
