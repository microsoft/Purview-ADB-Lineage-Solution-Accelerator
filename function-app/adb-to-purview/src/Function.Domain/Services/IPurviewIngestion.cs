// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Function.Domain.Helpers;

namespace Function.Domain.Services
{
    public interface IPurviewIngestion
    {
        public Task<JArray> SendToPurview(JArray Processes, IColParser colParser);
        public Task<bool> SendToPurview(JObject json, IColParser colParser);
    }
}