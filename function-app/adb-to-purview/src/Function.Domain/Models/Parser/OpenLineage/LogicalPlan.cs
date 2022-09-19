// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Function.Domain.Models.OL
{
    public class LogicalPlan
    {
        public List<Plan> plan { get; set; } = new List<Plan>();
    }
}


