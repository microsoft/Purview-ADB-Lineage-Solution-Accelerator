using System.Collections.Generic;
using Newtonsoft.Json;

namespace Function.Domain.Models.Purview
{
    public class ColumnNames
    {
       [JsonProperty("InputCol")]
       public string InputCol = "";
       [JsonProperty("Outputcol")]
       public string Outputcol = "";
    }


}