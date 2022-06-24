using System.Collections.Generic;
using Newtonsoft.Json;

namespace Function.Domain.Models.Purview
{
    public class ColumnLevelAttributes
    {
        [JsonProperty("DatasetMapping")]
        public List<DatasetMappingClass> datasetMapping = new List<DatasetMappingClass>();
        [JsonProperty("ColumnMapping")]
        public List<ColumnMappingClass> columnMapping = new List<ColumnMappingClass>();

    }

    public class DatasetMappingClass
    {
       [JsonProperty("Source")]
       public string source = "";
       [JsonProperty("Sink")]
       public string sink = "";
    }

    public class ColumnMappingClass
    {
       [JsonProperty("Source")]
       public string source = "";
       [JsonProperty("Sink")]
       public string sink = "";
    }


}