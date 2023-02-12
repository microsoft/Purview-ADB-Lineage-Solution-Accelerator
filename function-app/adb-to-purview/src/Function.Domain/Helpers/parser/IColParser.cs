// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Function.Domain.Models.OL;
using Function.Domain.Models.Purview;
using System.Collections.Generic;

namespace Function.Domain.Helpers
{
    public interface IColParser
     {
        public List<ColumnLevelAttributes> GetColIdentifiers();
    }
}