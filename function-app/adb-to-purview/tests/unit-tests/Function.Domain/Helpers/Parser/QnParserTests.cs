// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.IO;
using Function.Domain.Helpers;
using Function.Domain.Models.Settings;
using Function.Domain.Models.OL;
using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;

namespace UnitTests.Function.Domain.Helpers
{
    public class QnParserTests
    {
        private ParserSettings _config;
        private List<MountPoint> _mounts_info = UnitTestData.QnParserTestData.MountPoints;

        private IQnParser _qnparser;

        public QnParserTests()
        {
            var mockLoggerFactory = new NullLoggerFactory();
                _config = JsonConvert.DeserializeObject<ParserSettings>(File.ReadAllText("../../../../../../../deployment/infra/OlToPurviewMappings.json")) ?? new ParserSettings();
                _config.AdbWorkspaceUrl = "adb-unit-test.1.azuredatabricks.net";
                _qnparser = new QnParser(_config, mockLoggerFactory, _mounts_info);
        }


        //Tests Qualified Name parsing for each scenario
        [Theory]
        // Hive not default
        [InlineData("dbfs", 
                    "/user/hive/warehouse/notdefault.db/hiveexamplea", 
                    "notdefault.hiveexamplea@adb-unit-test.1.azuredatabricks.net")] 
        // Hive default
        [InlineData("dbfs", 
                    "/user/hive/warehouse/hiveexampleoutput000", 
                    "default.hiveexampleoutput000@adb-unit-test.1.azuredatabricks.net")] 
        // WASBS Blob - only supported in Azure Storage, not ADLS Gen2
        [InlineData("wasbs://rawdata@purviewexamplessa.blob.core.windows.net", 
                    "/retail", 
                    "https://purviewexamplessa.blob.core.windows.net/rawdata/retail")]
        // WASB
        [InlineData("wasb://rawdata@purviewexamplessa.blob.core.windows.net", 
                    "/retail", 
                    "https://purviewexamplessa.blob.core.windows.net/rawdata/retail")]
        // ABFSS
        [InlineData("abfss://rawdata@purviewexamplessa.dfs.core.windows.net", 
                    "/retail", 
                    "https://purviewexamplessa.dfs.core.windows.net/rawdata/retail")]
        // ABFS
        [InlineData("abfs://rawdata@purviewexamplessa.dfs.core.windows.net", 
                    "/retail", 
                    "https://purviewexamplessa.dfs.core.windows.net/rawdata/retail")]
        // ABFSS - Blob
        [InlineData("abfss://rawdata@purviewexamplessa.blob.core.windows.net", 
                    "/retail", 
                    "https://purviewexamplessa.dfs.core.windows.net/rawdata/retail")]
        // // Cosmos
        // [InlineData("azurecosmos://purview-to-adb-cdb.documents.azure.com/dbs/NewWriteScalaDB", 
        //             "/colls/NewWriteScalaCon", 
        //             "https://purview-to-adb-cdb.documents.azure.com/dbs/NewWriteScalaDB/colls/NewWriteScalaCon")]
        // Azure SQL
        [InlineData("sqlserver://purview-to-adb-sql.database.windows.net:1433;database=purview-to-adb-sqldb;encrypt=true;", 
                    "borrower_with_pid", 
                    "mssql://purview-to-adb-sql.database.windows.net/purview-to-adb-sqldb/dbo/borrower_with_pid")]
        // Azure SQL - databaseName
        [InlineData("sqlserver://purview-to-adb-sql.database.windows.net:1433;databaseName=purview-to-adb-sqldb;encrypt=true;", 
                    "borrower_with_pid", 
                    "mssql://purview-to-adb-sql.database.windows.net/purview-to-adb-sqldb/dbo/borrower_with_pid")]
        // Synapse
        [InlineData("sqlserver://purviewadbsynapsews.sql.azuresynapse.net:1433;database=SQLPool1;", 
                    "exampleinputA", 
                    "mssql://purviewadbsynapsews.sql.azuresynapse.net/SQLPool1/dbo/exampleinputA")]
        // DBFS mount
        [InlineData("dbfs", 
                    "/mnt/rawdata/retail", 
                    "https://purviewexamplessa.dfs.core.windows.net/rawdata/retail")]  
        // DBFS mount - Shortest String Match
        [InlineData("dbfs", 
                    "/mnt/x/abc", 
                    "https://xsa.dfs.core.windows.net/x/abc")]  
        // DBFS mount - Longest String Match
        [InlineData("dbfs", 
                    "/mnt/x/y/abc", 
                    "https://ysa.dfs.core.windows.net/y/abc")]  
        // DBFS mount trailing slash in def
        [InlineData("dbfs", 
                    "/mnt/purview2", 
                    "https://purviewexamplessa.dfs.core.windows.net/purview2")]  
        // Azure SQL Non DBO Schema - <need verification of Purview string>
        [InlineData("sqlserver://purview-to-adb-sql.database.windows.net;database=purview-to-adb-sqldb;", 
                    "[mytest].[tablename.will.mark]", 
                    "mssql://purview-to-adb-sql.database.windows.net/purview-to-adb-sqldb/mytest/tablename.will.mark")]
        // Azure SQL Non DBO Schema - dots only
        [InlineData("sqlserver://purview-to-adb-sql.database.windows.net;database=purview-to-adb-sqldb;", 
                    "mytest.tablename",
                    "mssql://purview-to-adb-sql.database.windows.net/purview-to-adb-sqldb/mytest/tablename")]
        // Synapse Non DBO Schema
        [InlineData("sqlserver://purviewadbsynapsews.sql.azuresynapse.net:1433;database=SQLPool1;", 
                    "sales.region", 
                    "mssql://purviewadbsynapsews.sql.azuresynapse.net/SQLPool1/sales/region")]
        // Azure MySQL
        [InlineData("mysql://fikz4nmpfka4s.mysql.database.azure.com:3306/mydatabase", 
                    "fruits", 
                    "mysql://fikz4nmpfka4s.mysql.database.azure.com/mydatabase/fruits")]

        public void GetIdentifiers_OlSource_ReturnsPurviewIdentifier(string nameSpace, string name, string expectedResult)
        {
            var rslt = _qnparser.GetIdentifiers(nameSpace, name);

            Xunit.Assert.Equal(expectedResult, rslt.QualifiedName);
        }
        // [Theory]
        // [InlineData("sqlserver://purviewadbsynapsews.sql.azuresynapse.net:1433;database=SQLPool1;", 
        //             "sales.region", 
        //             "mssql://purviewadbsynapsews.sql.azuresynapse.net/SQLPool1/sales/region")]         
        // public void GetIdentifiers_OlSource_ReturnsPurviewIde(string nameSpace, string name, string expectedResult)
        // {
        //     var rslt = _qnparser.GetIdentifiers(nameSpace, name);

        //     Xunit.Assert.Equal(expectedResult, rslt.QualifiedName);
        // }
    }
}