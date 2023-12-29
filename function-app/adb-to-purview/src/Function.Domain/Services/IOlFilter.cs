
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Function.Domain.Models.OL;

namespace Function.Domain.Services
{
    public interface IOlFilter
    {
        bool FilterOlMessage(Event? olEvent);
        string GetJobNamespace(Event olEvent);
    }
}