using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Function.Domain.Models
{
#pragma warning disable CS8618
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Contact
    {
        public string contactType { get; set; }
        public string id { get; set; }
    }

    public class Term
    {
        public string name { get; set; }
        public string guid { get; set; }
        public string glossaryName { get; set; }
    }

    public class EntityModel
    {
        public object owner { get; set; }
        public string qualifiedName { get; set; }
        public string entityType { get; set; }
        public List<Contact> contact { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public List<Term> term { get; set; }
        public string id { get; set; }
        public List<string> label { get; set; }
        public List<string> classification { get; set; }
        public string collectionId { get; set; }
        public List<string> assetType { get; set; }

        [JsonProperty("@search.highlights")]
        public object? SearchHighlights { get; set; }

        [JsonProperty("@search.score")]
        public double? SearchScore { get; set; }
    }

    public class QueryValeuModel
    {
        public object owner { get; set; }
        public string qualifiedName { get; set; }
        public string entityType { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public List<Term> term { get; set; }
        public string id { get; set; }
        public List<string> label { get; set; }
        public List<string> classification { get; set; }
        public string collectionId { get; set; }
        public List<string> assetType { get; set; }

        [JsonProperty("@search.highlights")]
        public object? SearchHighlights { get; set; }

        [JsonProperty("@search.score")]
        public double? SearchScore { get; set; }
    }

    public class PurviewEntitySearchResponseModel
    {
        [JsonProperty("@search.count")]
        public int SearchCount { get; set; }
        [JsonProperty("value")]
        public List<EntityModel> entities { get; set; }

        [JsonProperty("@search.facets")]
        public object SearchFacets { get; set; }
    }

    public class PurviewQueryResponseModel
    {
        [JsonProperty("@search.count")]
        public int SearchCount { get; set; }
        [JsonProperty("value")]
        public List<QueryValeuModel> entities { get; set; }

    }

}
