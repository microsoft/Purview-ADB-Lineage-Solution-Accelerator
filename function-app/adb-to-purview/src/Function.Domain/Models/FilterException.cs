using System.Collections.Generic;
using Newtonsoft.Json;
namespace Function.Domain.Models
{
    public class FilterExeption
    {
        [JsonProperty("attributes")]
        public List<FilterAttributes?>? Attributes { get; set; }
        public bool IsExeption(string name)
        {
            foreach (FilterAttributes? attribute in this.Attributes!)
            {
                if (attribute!.typeName!.Contains('*'))
                {
                    if(name.Contains(attribute!.typeName!.Replace("*","")))
                        return true;
                }
                else
                    if (attribute!.typeName == name)
                        return true;
            }

            return false;
        }
    }

    public class FilterAttributes
    {
        public string? typeName { get; set; }
    }
}