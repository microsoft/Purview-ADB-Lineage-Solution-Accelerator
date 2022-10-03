// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Function.Domain.Models;
using Function.Domain.Models.OL;
using Function.Domain.Models.Settings;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace UnitTests.Function.Domain.Helpers
{
    public static class UnitTestData
    {
        public struct QnParserTestData
        {
            public static List<MountPoint> MountPoints = new List<MountPoint>()
            {
                new MountPoint(){MountPointName="/databricks/mlflow-registry",Source="databricks/mlflow-registry"},
                new MountPoint(){MountPointName="/databricks-datasets",Source="databricks-datasets"},
                new MountPoint(){MountPointName="/mnt/rawdata",Source="abfss://rawdata@purviewexamplessa.dfs.core.windows.net/"},
                new MountPoint(){MountPointName="/databricks/mlflow-tracking",Source="databricks/mlflow-tracking"},
                new MountPoint(){MountPointName="/mnt/delta",Source="abfss://deltalake@purviewexamplessa.dfs.core.windows.net/"},
                new MountPoint(){MountPointName="/mnt/outputdata",Source="abfss://outputdata@purviewexamplessa.dfs.core.windows.net/"},
                new MountPoint(){MountPointName="/databricks-results",Source="databricks-results"},
                new MountPoint(){MountPointName="/databricks-results",Source="databricks-results"},
                new MountPoint(){MountPointName="/mnt/purview2/",Source="abfss://purview2@purviewexamplessa.dfs.core.windows.net/"}
            };
        }
    }
}