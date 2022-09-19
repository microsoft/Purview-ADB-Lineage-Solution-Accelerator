// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Function.Domain.Models.Adb
{
    public enum JobType
    {
        InteractiveNotebook,
        JobNotebook,
        JobPython,
        JobWheel,
        JobJar,
        Unsupported
    }
}