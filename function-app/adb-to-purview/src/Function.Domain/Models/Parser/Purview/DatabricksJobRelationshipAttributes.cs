// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
namespace Function.Domain.Models.Purview
{
    public class DatabricksJobRelationshipAttributes
    {
        [JsonProperty("workspace")]
        public RelationshipAttribute Workspace = new RelationshipAttribute();
    }

}