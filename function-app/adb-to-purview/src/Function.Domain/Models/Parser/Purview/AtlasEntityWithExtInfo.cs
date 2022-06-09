using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Function.Domain.Models.Purview
{
    public class AtlasEntityWithExtInfo
    {
        public List<Asset> ? entities;
        public Dictionary<string, Asset>? referredEntities;
        public Asset ? entity;
    }
}