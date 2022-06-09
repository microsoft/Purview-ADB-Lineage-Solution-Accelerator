using System.Collections.Generic;
using Newtonsoft.Json;

namespace Function.Domain.Models.Purview
{
       public interface IDatabricksJobTaskAttributes
    {
        public string Name { get; set; }
        public string QualifiedName { get; set; }
        public long JobId { get; set; }
        public string ClusterId { get; set; }
        public string SparkVersion { get; set; }
    }

}