using System;

using Newtonsoft.Json;

using Microsoft.WindowsAzure.Storage.Table;



namespace AzureOuterLogs.Common
{

    public class baseLogEntry : TableEntity
    { 
        public baseLogEntry()
        {
            PartitionKey = $"XMLTest-{DateTime.Now.ToShortDateString()}";
            RowKey = $"{CorrelationID}";
        }
        public Guid CorrelationID { get; set; }
        public DateTime dateTIme { get; set; }
        public string User { get; set; }
        public string Methodname { get; set; }
        public string MachineName { get; set; }
        public string requestorIP { get; set; }
       
    }

    public class LogEntry: baseLogEntry
    {                   
        public string RequestHeader { get; set; }
        public string RequestBody { get; set; }            
    }


    public class JSONentry
    {
        public JSONentry(baseLogEntry logEntry)
        {
            this.entry = JsonConvert.SerializeObject(logEntry);
        }

        public JSONentry(LogEntry logEntry)
        {
            this.entry = JsonConvert.SerializeObject(logEntry);
        }
        public string entry { get; set; }
    }

}
