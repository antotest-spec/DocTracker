<h2>
DocTracker
<br/>
.NET 5.0 ASP.NET WebAPI to track documents processed on a separate DocManager application
</h2>
<br/>

<p>

**IMPORTANT: ONE CRITICAL FILE IS MISSING IN THIS REPO BECAUSE IT CONTAINS SOME SECRETS**

The file is named appsettings.json and it's located in the project main folder (same folder where Program.cs is)

**I am sharing this file privately only with my team mates.**

Anyone can create the file on it's own, an Azure subscription is needed, create a new table storage there, then follow this schema
</p>

    {
        "Logging": {
            "LogLevel": {
                "Default": "Information",
                "Microsoft": "Warning",
                "Microsoft.Hosting.Lifetime": "Information"
            }
        },
        "AllowedHosts": "*",
        "TableAccount": "<INSERT YOUR AZURE STORAGE ACCOUNT NAME HERE>",
        "TableName": "<INSERT YOUR AZURE TABLE STORAGE NAME HERE>",
        "TableSASToken": "<INSERT YOUR TABLE STORAGE SAS TOKEN (NOT THE URI) HERE>",
    }



To run this API on your machine, clone this repository on your local machine and open the solution file DocTracker.sln with Visual studio 2019. Press F5 to run the application in debug mode. After some seconds you will see the Swagger frontend UI in your default browser, that will allow you to explore and test the API methods with the parameters of your choice. **To save time** a predefined template named "Test Example.json" is provided in this repository, more on this later.

The application is very easy and self explanatory, you basically have 3 API POST methods: 
<p>

**TrackDocEvents** This method tells the API that some events occured on the DocManager, if the payload is in a valid format, the events will be stored in the database and will contribute to enrich the reports created with the other API  methods. Please note that the top 4 fields in this method are not really needed to fill (they are overriden with auto generated data by the method) . In the next version I will remove them. Again, as said above, refer to "Test Example.json" to save time understanding how data should be entered. By the way The 4 unnecessary fields are
</p>

    {
    "partitionKey": "string",
    "rowKey": "string",
    "timestamp": "2021-06-05T17:31:20.675Z",
    "eTag": "string"
    }
 


<p>

**DocHistory** This method creates a report of all the processing events that occurred on a document since its firts upload. You just need to fill a single string parameter with the document UniqueID. Please enclose the id in double quotes, e.g. "doc4232" 
</p>

<p>

**UploadsReport** This method creates a report of all the documents uploaded in the past "X" seconds or a report of the past "X" documents uploaded, you can change the behaviour with the parameters: 

e.g. 
last 5 documents

    {
    "howMany": 5,
    "specifyUploadOrSecond": "upload"
    }

or in the last 30 minutes 

    {
    "howMany": 1800,
    "specifyUploadOrSecond": "second"
    }

the documents are grouped by category and both subtotals and totals are displayed in the report

</p>
