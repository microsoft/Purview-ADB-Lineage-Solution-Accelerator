
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Function.Domain.Services
{
    public interface IOlFilter
    {
        bool FilterOlMessage(string strRequest);
        string GetJobNamespace(string strRequest);
    }
}