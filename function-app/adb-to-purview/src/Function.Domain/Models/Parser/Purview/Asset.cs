using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System;


namespace Function.Domain.Models.Purview
{
    public class Asset
    {
        public string typeName = "";
        public string lastModifiedTS = "1";
        public string guid = "";
        public string status = "ACTIVE";
        public string createdBy = "";
        public string updatedBy = "";
        public Int64 createTime = 0;
        public Int64 updateTime = 0;
        public int version = 0;
        public Dictionary<string,object>? sourceDetails;
        public Dictionary<string,object>? relationshipAttributes;
        public Dictionary<string,object> ? attributes;
        public string source = "";
    }
}