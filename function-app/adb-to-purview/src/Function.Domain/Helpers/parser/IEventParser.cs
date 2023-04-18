// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Function.Domain.Models.OL;
using Function.Domain.Models.Purview;

namespace Function.Domain.Helpers
{
    public interface IEventParser
    {
        public Event? ParseOlEvent(string eventPayload);

        public string TrimPrefix(string strEvent);
    }
}