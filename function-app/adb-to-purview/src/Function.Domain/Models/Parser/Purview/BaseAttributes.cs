// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Function.Domain.Models.Purview
{
    public class BaseAttributes
    {
        [JsonProperty("name")]
        public string Name = "";
        [JsonProperty("qualifiedName")]
        public string QualifiedName = "";
    }
}