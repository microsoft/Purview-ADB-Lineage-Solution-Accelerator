// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
namespace Function.Domain.Models
{
    public class UniqueAttributes
    {
        [JsonProperty("qualifiedName")]
        public string QualifiedName { get; set; } = "";

    }

}