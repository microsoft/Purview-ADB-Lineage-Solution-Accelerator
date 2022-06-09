using System;
using Azure.Data.Tables;

namespace Function.Domain.Models.Settings
{
    // Used to save state of OL messages in Azure Table Storage for later message consolodation with incoming messages
    public class OlTableEntity : ITableEntity
    {
        public string EnvFacet = "";
        public string PartitionKey { get; set; } = "";
        public string RowKey { get; set; } = "";
        public DateTimeOffset? Timestamp { get; set; }
        public Azure.ETag ETag { get; set; }
    }
}