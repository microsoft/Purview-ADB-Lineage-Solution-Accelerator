using Function.Domain.Models;
using Function.Domain.Models.OL;
using Function.Domain.Models.Settings;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace UnitTests.Function.Domain.Helpers
{
    public static class UnitTestColData
    {

        public struct OlToPurviewParsingServiceTestColData
        {
            public const string CompleteOlColMessage = "{\"eventType\":\"COMPLETE\",\"eventTime\":\"2022-01-25T19:36:00.668Z\",\"run\":{\"runId\":\"225e05d7-5c5a-434c-a544-ffbda7dfa44b\",\"facets\":{\"environment-properties\":{\"_producer\":\"https://github.com/OpenLineage/OpenLineage/tree/0.6.0-SNAPSHOT/integration/spark\",\"_schemaURL\":\"https://openlineage.io/spec/1-0-2/OpenLineage.json#/$defs/RunFacet\",\"environment-properties\":{\"spark.databricks.clusterUsageTags.clusterName\":\"CMI_Purview_POC01\",\"spark.databricks.clusterUsageTags.azureSubscriptionId\":\"f48a1ead-b2e2-48b7-9575-b2cf93652fa6\",\"spark.databricks.notebook.path\":\"/Shared/Scenario01DimOrg/S01DimOrg_raw2int\",\"mountPoints\":[{\"mountPoint\":\"/databricks-datasets\",\"source\":\"s3a://databricks-datasets-oregon/\"},{\"mountPoint\":\"/databricks/mlflow-tracking\",\"source\":\"unsupported-access-mechanism-for-path--use-mlflow-client:/\"},{\"mountPoint\":\"/databricks-results\",\"source\":\"wasbs://ephemeral@dbstorage3237izou4adxk.blob.core.windows.net/3464153745686717\"},{\"mountPoint\":\"/mnt/raw\",\"source\":\"abfss://raw@aaiarchdesignpurviewadls.dfs.core.windows.net/\"},{\"mountPoint\":\"/databricks/mlflow-registry\",\"source\":\"unsupported-access-mechanism-for-path--use-mlflow-client:/\"},{\"mountPoint\":\"/mnt/primary\",\"source\":\"abfss://primary@aaiarchdesignpurviewadls.dfs.core.windows.net/\"},{\"mountPoint\":\"/mnt/integration\",\"source\":\"abfss://integration@aaiarchdesignpurviewadls.dfs.core.windows.net/\"},{\"mountPoint\":\"/\",\"source\":\"wasbs://root@dbstorage3237izou4adxk.blob.core.windows.net/3464153745686717\"}],\"spark.databricks.clusterUsageTags.clusterOwnerOrgId\":\"3464153745686717\",\"user\":\"marktayl@cmiaaval.cummins.com\",\"userId\":\"5533428870378942\",\"orgId\":\"3464153745686717\"}},\"spark_version\":{\"_producer\":\"https://github.com/OpenLineage/OpenLineage/tree/0.6.0-SNAPSHOT/integration/spark\",\"_schemaURL\":\"https://openlineage.io/spec/1-0-2/OpenLineage.json#/$defs/RunFacet\",\"spark-version\":\"3.1.2\",\"openlineage-spark-version\":\"0.6.0-SNAPSHOT\"},\"spark.logicalPlan\":{\"_producer\":\"https://github.com/OpenLineage/OpenLineage/tree/0.6.0-SNAPSHOT/integration/spark\",\"_schemaURL\":\"https://openlineage.io/spec/1-0-2/OpenLineage.json#/$defs/RunFacet\",\"plan\":[{\"class\":\"org.apache.spark.sql.catalyst.plans.logical.OverwriteByExpression\",\"num-children\":1,\"table\":[{\"class\":\"org.apache.spark.sql.execution.datasources.v2.DataSourceV2Relation\",\"num-children\":0,\"table\":null,\"output\":[[{\"class\":\"org.apache.spark.sql.catalyst.expressions.AttributeReference\",\"num-children\":0,\"name\":\"OrganizationKey\",\"dataType\":\"integer\",\"nullable\":true,\"metadata\":{},\"exprId\":{\"product-class\":\"org.apache.spark.sql.catalyst.expressions.ExprId\",\"id\":2018,\"jvmId\":\"45fa987d-eacc-4f30-af81-87eac6172e23\"},\"qualifier\":[]}],[{\"class\":\"org.apache.spark.sql.catalyst.expressions.AttributeReference\",\"num-children\":0,\"name\":\"ParentOrganizationKey\",\"dataType\":\"integer\",\"nullable\":true,\"metadata\":{},\"exprId\":{\"product-class\":\"org.apache.spark.sql.catalyst.expressions.ExprId\",\"id\":2019,\"jvmId\":\"45fa987d-eacc-4f30-af81-87eac6172e23\"},\"qualifier\":[]}],[{\"class\":\"org.apache.spark.sql.catalyst.expressions.AttributeReference\",\"num-children\":0,\"name\":\"PercentageOfOwnership\",\"dataType\":\"double\",\"nullable\":true,\"metadata\":{},\"exprId\":{\"product-class\":\"org.apache.spark.sql.catalyst.expressions.ExprId\",\"id\":2020,\"jvmId\":\"45fa987d-eacc-4f30-af81-87eac6172e23\"},\"qualifier\":[]}],[{\"class\":\"org.apache.spark.sql.catalyst.expressions.AttributeReference\",\"num-children\":0,\"name\":\"OrganizationName\",\"dataType\":\"string\",\"nullable\":true,\"metadata\":{},\"exprId\":{\"product-class\":\"org.apache.spark.sql.catalyst.expressions.ExprId\",\"id\":2021,\"jvmId\":\"45fa987d-eacc-4f30-af81-87eac6172e23\"},\"qualifier\":[]}],[{\"class\":\"org.apache.spark.sql.catalyst.expressions.AttributeReference\",\"num-children\":0,\"name\":\"CurrencyKey\",\"dataType\":\"integer\",\"nullable\":true,\"metadata\":{},\"exprId\":{\"product-class\":\"org.apache.spark.sql.catalyst.expressions.ExprId\",\"id\":2022,\"jvmId\":\"45fa987d-eacc-4f30-af81-87eac6172e23\"},\"qualifier\":[]}]],\"catalog\":null,\"identifier\":null,\"options\":null}],\"deleteExpr\":[{\"class\":\"org.apache.spark.sql.catalyst.expressions.Literal\",\"num-children\":0,\"value\":\"true\",\"dataType\":\"boolean\"}],\"query\":0,\"writeOptions\":null,\"isByName\":false,\"requireImplicitCasting\":true},{\"class\":\"org.apache.spark.sql.execution.datasources.LogicalRelation\",\"num-children\":0,\"relation\":null,\"output\":[[{\"class\":\"org.apache.spark.sql.catalyst.expressions.AttributeReference\",\"num-children\":0,\"name\":\"OrganizationKey\",\"dataType\":\"integer\",\"nullable\":true,\"metadata\":{},\"exprId\":{\"product-class\":\"org.apache.spark.sql.catalyst.expressions.ExprId\",\"id\":6,\"jvmId\":\"45fa987d-eacc-4f30-af81-87eac6172e23\"},\"qualifier\":[]}],[{\"class\":\"org.apache.spark.sql.catalyst.expressions.AttributeReference\",\"num-children\":0,\"name\":\"ParentOrganizationKey\",\"dataType\":\"integer\",\"nullable\":true,\"metadata\":{},\"exprId\":{\"product-class\":\"org.apache.spark.sql.catalyst.expressions.ExprId\",\"id\":7,\"jvmId\":\"45fa987d-eacc-4f30-af81-87eac6172e23\"},\"qualifier\":[]}],[{\"class\":\"org.apache.spark.sql.catalyst.expressions.AttributeReference\",\"num-children\":0,\"name\":\"PercentageOfOwnership\",\"dataType\":\"double\",\"nullable\":true,\"metadata\":{},\"exprId\":{\"product-class\":\"org.apache.spark.sql.catalyst.expressions.ExprId\",\"id\":8,\"jvmId\":\"45fa987d-eacc-4f30-af81-87eac6172e23\"},\"qualifier\":[]}],[{\"class\":\"org.apache.spark.sql.catalyst.expressions.AttributeReference\",\"num-children\":0,\"name\":\"OrganizationName\",\"dataType\":\"string\",\"nullable\":true,\"metadata\":{},\"exprId\":{\"product-class\":\"org.apache.spark.sql.catalyst.expressions.ExprId\",\"id\":9,\"jvmId\":\"45fa987d-eacc-4f30-af81-87eac6172e23\"},\"qualifier\":[]}],[{\"class\":\"org.apache.spark.sql.catalyst.expressions.AttributeReference\",\"num-children\":0,\"name\":\"CurrencyKey\",\"dataType\":\"integer\",\"nullable\":true,\"metadata\":{},\"exprId\":{\"product-class\":\"org.apache.spark.sql.catalyst.expressions.ExprId\",\"id\":10,\"jvmId\":\"45fa987d-eacc-4f30-af81-87eac6172e23\"},\"qualifier\":[]}]],\"isStreaming\":false}]}}},\"job\":{\"namespace\":\"adbpurviewol1\",\"name\":\"databricks_shell.overwrite_by_expression_exec_v1\",\"facets\":{}},\"inputs\":[{\"namespace\":\"abfss://raw@aaiarchdesignpurviewadls.dfs.core.windows.net\",\"name\":\"/DimOrganization.parquet\",\"facets\":{\"dataSource\":{\"_producer\":\"https://github.com/OpenLineage/OpenLineage/tree/0.6.0-SNAPSHOT/integration/spark\",\"_schemaURL\":\"https://openlineage.io/spec/facets/1-0-0/DatasourceDatasetFacet.json#/$defs/DatasourceDatasetFacet\",\"name\":\"abfss://raw@aaiarchdesignpurviewadls.dfs.core.windows.net\",\"uri\":\"abfss://raw@aaiarchdesignpurviewadls.dfs.core.windows.net\"},\"schema\":{\"_producer\":\"https://github.com/OpenLineage/OpenLineage/tree/0.6.0-SNAPSHOT/integration/spark\",\"_schemaURL\":\"https://openlineage.io/spec/facets/1-0-0/SchemaDatasetFacet.json#/$defs/SchemaDatasetFacet\",\"fields\":[{\"name\":\"OrganizationKey\",\"type\":\"integer\"},{\"name\":\"ParentOrganizationKey\",\"type\":\"integer\"},{\"name\":\"PercentageOfOwnership\",\"type\":\"double\"},{\"name\":\"OrganizationName\",\"type\":\"string\"},{\"name\":\"CurrencyKey\",\"type\":\"integer\"}]}},\"inputFacets\":{}}],\"outputs\":[{\"namespace\":\"N2\",\"name\":\"outputTable\",\"facets\":{\"schema\":{\"fields\":[{\"name\":\"col_a\",\"type\":\"VARCHAR\"},{\"name\":\"col_b\",\"type\":\"int\"}]},\"columnLineage\":{\"fields\":{\"col_a\":{\"inputFields\":[{\"namespace\":\"N1\",\"name\":\"inputTable\",\"field\":\"col_a\"}]},\"col_b\":{\"inputFields\":[{\"namespace\":\"N1\",\"name\":\"inputTable\",\"field\":\"col_b\"}]}}}}}],\"producer\":\"https://github.com/OpenLineage/OpenLineage/tree/0.6.0-SNAPSHOT/integration/spark\",\"schemaURL\":\"https://openlineage.io/spec/1-0-2/OpenLineage.json#/$defs/RunEvent\"}";
            public static string SettingsString = "{\"OlToPurviewMappings\": [{\"UrlSuffix\":\"\",\"OlPrefix\":\"wasbs\",\"PurviewPrefix\":\"https\",\"PurviewDatatype\":\"azure_datalake_gen2_path\"},{\"UrlSuffix\":\"\",\"OlPrefix\":\"adfss\",\"PurviewPrefix\":\"https\",\"PurviewDatatype\":\"azure_datalake_gen2_path\"},{\"UrlSuffix\":\"\",\"OlPrefix\":\"azurecosmos\",\"PurviewPrefix\":\"https\",\"PurviewDatatype\":\"azure_cosmosdb_sqlapi_collection\"},{\"UrlSuffix\":\"database.windows.net\",\"OlPrefix\":\"sqlserver\",\"PurviewPrefix\":\"mssql\",\"PurviewDatatype\":\"azure_sql_table\"},{\"UrlSuffix\":\"sql.azuresynapse.net\",\"OlPrefix\":\"sqlserver\",\"PurviewPrefix\":\"mssql\",\"PurviewDatatype\":\"azure_synapse_dedicated_sql_table\"},{\"UrlSuffix\":\"\",\"OlPrefix\":\"dbfs\",\"PurviewPrefix\":\"https\",\"PurviewDatatype\":\"azure_datalake_gen2_path\"}]}";
        }
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

        public struct SparkProcessParserTestData
        {
            // Using the null exculsion operator here as json lib should ignore nulls
            public static Event GetEvent()
            {
                var trimString = TrimPrefix(OlToPurviewParsingServiceTestColData.CompleteOlColMessage);
                Event dEvent = JsonConvert.DeserializeObject<Event>(trimString)!;
                return dEvent;
            }

            private static string TrimPrefix(string strEvent)
            {
                return strEvent.Substring(strEvent.IndexOf('{')).Trim();
            }
        }

        public struct SharedTestData
        {              
            public static ParserSettings Settings = new ParserSettings(){OlToPurviewMappings = new List<OlToPurviewMapping>()
            {
                new OlToPurviewMapping{Name="wasbs",QualifiedName="https://purviewexamplessa.blob.core.windows.net/rawdata/retail",PurviewDataType="azure_blob_path"},
                new OlToPurviewMapping{Name="wasb",QualifiedName="https://purviewexamplessa.blob.core.windows.net/rawdata/retail",PurviewDataType="azure_blob_path"},
                new OlToPurviewMapping{Name="abfsBlobRootFS",QualifiedName="https://purviewexamplessa.dfs.core.windows.net/rawdata",PurviewDataType="azure_datalake_gen2_filesystem"},
                new OlToPurviewMapping{Name="abfsRootFS",QualifiedName="https://purviewexamplessa.dfs.core.windows.net/rawdata",PurviewDataType="azure_datalake_gen2_filesystem"},
                new OlToPurviewMapping{Name="abfssRootFS",QualifiedName="https://purviewexamplessa.dfs.core.windows.net/rawdata",PurviewDataType="azure_datalake_gen2_filesystem"},
                new OlToPurviewMapping{Name="abfsBlob",QualifiedName="https://purviewexamplessa.dfs.core.windows.net/rawdata/retail",PurviewDataType="azure_datalake_gen2_path"},
                new OlToPurviewMapping{Name="abfs",QualifiedName="https://purviewexamplessa.dfs.core.windows.net/rawdata/retail",PurviewDataType="azure_datalake_gen2_path"},
                new OlToPurviewMapping{Name="abfss",QualifiedName="https://purviewexamplessa.dfs.core.windows.net/rawdata/retail",PurviewDataType="azure_datalake_gen2_path"},
                new OlToPurviewMapping{Name="cosmos",QualifiedName="https://purview-to-adb-cdb.documents.azure.com/dbs/NewWriteScalaDB/colls/NewWriteScalaCon",PurviewDataType="azure_cosmosdb_sqlapi_collection"},
                new OlToPurviewMapping{Name="synapseSqlNonDbo",QualifiedName="mssql://purviewadbsynapsews.sql.azuresynapse.net/SQLPool1/sales/region",PurviewDataType="azure_synapse_dedicated_sql_table"},
                new OlToPurviewMapping{Name="azureSQLNonDbo",QualifiedName="mssql://purview-to-adb-sql.database.windows.net/purview-to-adb-sqldb/mytest/tablename.will.mark",PurviewDataType="azure_sql_table"},
                new OlToPurviewMapping{Name="synapseSql",QualifiedName="mssql://purviewadbsynapsews.sql.azuresynapse.net/SQLPool1/dbo/exampleinputA",PurviewDataType="azure_synapse_dedicated_sql_table"},
                new OlToPurviewMapping{Name="azureSQL",QualifiedName="mssql://purview-to-adb-sql.database.windows.net/purview-to-adb-sqldb/dbo/borrower_with_pid ",PurviewDataType="azure_sql_table"}
            }};
            public static string SettingsString = "{\"olToPurviewMappings\":[{\"name\":\"wasbs\",\"parserConditions\":[{\"op1\":\"prefix\",\"compare\":\"=\",\"op2\":\"wasbs\"}],\"qualifiedName\":\"https://{nameSpcBodyParts[1]}/{nameSpcBodyParts[0]}/{nameGroups[0]}\",\"purviewDataType\":\"azure_blob_path\",\"purviewPrefix\":\"https\"},{\"name\":\"wasb\",\"parserConditions\":[{\"op1\":\"prefix\",\"compare\":\"=\",\"op2\":\"wasb\"}],\"qualifiedName\":\"https://{nameSpcBodyParts[1]}/{nameSpcBodyParts[0]}/{nameGroups[0]}\",\"purviewDataType\":\"azure_blob_path\",\"purviewPrefix\":\"https\"},{\"name\":\"abfsBlobRootFS\",\"parserConditions\":[{\"op1\":\"prefix\",\"compare\":\"=\",\"op2\":\"abfs\"},{\"op1\":\"nameSpcBodyParts[1]\",\"compare\":\"contains\",\"op2\":\"blob\"},{\"op1\":\"nameGroups[0]\",\"compare\":\"=\",\"op2\":\"\"}],\"qualifiedName\":\"https://{nameSpcConParts[0]}.dfs.{nameSpcConParts[2]}.{nameSpcConParts[3]}.{nameSpcConParts[4]}/{nameSpcBodyParts[0]}/{nameGroups[0]}\",\"purviewDataType\":\"azure_datalake_gen2_filesystem\",\"purviewPrefix\":\"https\"},{\"name\":\"abfsRootFS\",\"parserConditions\":[{\"op1\":\"prefix\",\"compare\":\"=\",\"op2\":\"abfs\"},{\"op1\":\"nameGroups[0]\",\"compare\":\"=\",\"op2\":\"\"}],\"qualifiedName\":\"https://{nameSpcBodyParts[1]}/{nameSpcBodyParts[0]}/{nameGroups[0]}\",\"purviewDataType\":\"azure_datalake_gen2_filesystem\",\"purviewPrefix\":\"https\"},{\"name\":\"abfssBlobRootFS\",\"parserConditions\":[{\"op1\":\"prefix\",\"compare\":\"=\",\"op2\":\"abfss\"},{\"op1\":\"nameSpcBodyParts[1]\",\"compare\":\"contains\",\"op2\":\"blob\"},{\"op1\":\"nameGroups[0]\",\"compare\":\"=\",\"op2\":\"\"}],\"qualifiedName\":\"https://{nameSpcConParts[0]}.dfs.{nameSpcConParts[2]}.{nameSpcConParts[3]}.{nameSpcConParts[4]}/{nameSpcBodyParts[0]}/{nameGroups[0]}\",\"purviewDataType\":\"azure_datalake_gen2_filesystem\",\"purviewPrefix\":\"https\"},{\"name\":\"abfssRootFS\",\"parserConditions\":[{\"op1\":\"prefix\",\"compare\":\"=\",\"op2\":\"abfss\"},{\"op1\":\"nameGroups[0]\",\"compare\":\"=\",\"op2\":\"\"}],\"qualifiedName\":\"https://{nameSpcBodyParts[1]}/{nameSpcBodyParts[0]}/{nameGroups[0]}\",\"purviewDataType\":\"azure_datalake_gen2_filesystem\",\"purviewPrefix\":\"https\"},{\"name\":\"abfsBlob\",\"parserConditions\":[{\"op1\":\"prefix\",\"compare\":\"=\",\"op2\":\"abfs\"},{\"op1\":\"nameSpcBodyParts[1]\",\"compare\":\"contains\",\"op2\":\"blob\"}],\"qualifiedName\":\"https://{nameSpcConParts[0]}.dfs.{nameSpcConParts[2]}.{nameSpcConParts[3]}.{nameSpcConParts[4]}/{nameSpcBodyParts[0]}/{nameGroups[0]}\",\"purviewDataType\":\"azure_datalake_gen2_path\",\"purviewPrefix\":\"https\"},{\"name\":\"abfs\",\"parserConditions\":[{\"op1\":\"prefix\",\"compare\":\"=\",\"op2\":\"abfs\"}],\"qualifiedName\":\"https://{nameSpcBodyParts[1]}/{nameSpcBodyParts[0]}/{nameGroups[0]}\",\"purviewDataType\":\"azure_datalake_gen2_path\",\"purviewPrefix\":\"https\"},{\"name\":\"abfssBlob\",\"parserConditions\":[{\"op1\":\"prefix\",\"compare\":\"=\",\"op2\":\"abfss\"},{\"op1\":\"nameSpcBodyParts[1]\",\"compare\":\"contains\",\"op2\":\"blob\"}],\"qualifiedName\":\"https://{nameSpcConParts[0]}.dfs.{nameSpcConParts[2]}.{nameSpcConParts[3]}.{nameSpcConParts[4]}/{nameSpcBodyParts[0]}/{nameGroups[0]}\",\"purviewDataType\":\"azure_datalake_gen2_path\",\"purviewPrefix\":\"https\"},{\"name\":\"abfss\",\"parserConditions\":[{\"op1\":\"prefix\",\"compare\":\"=\",\"op2\":\"abfss\"}],\"qualifiedName\":\"https://{nameSpcBodyParts[1]}/{nameSpcBodyParts[0]}/{nameGroups[0]}\",\"purviewDataType\":\"azure_datalake_gen2_path\",\"purviewPrefix\":\"https\"},{\"name\":\"cosmos\",\"parserConditions\":[{\"op1\":\"prefix\",\"compare\":\"=\",\"op2\":\"azurecosmos\"}],\"qualifiedName\":\"https://{nameSpcBodyParts[0]}/{nameSpcBodyParts[1]}/{nameSpcBodyParts[2]}/{nameGroups[0]}\",\"purviewDataType\":\"azure_cosmosdb_sqlapi_collection\",\"purviewPrefix\":\"https\"},{\"name\":\"synapseSqlNonDbo\",\"parserConditions\":[{\"op1\":\"prefix\",\"compare\":\"=\",\"op2\":\"sqlserver\"},{\"op1\":\"nameSpcBodyParts[0]\",\"compare\":\"contains\",\"op2\":\"azuresynapse\"},{\"op1\":\"nameGroups[0].parts\",\"compare\":\">\",\"op2\":\"1\"}],\"qualifiedName\":\"mssql://{nameSpcBodyParts[0]}/{nameSpcNameVals['database']}/{nameGroups[0].parts[0]}/{nameGroups[0].parts[1]}\",\"purviewDataType\":\"azure_synapse_dedicated_sql_table\",\"purviewPrefix\":\"mssql\"},{\"name\":\"azureSQLNonDbo\",\"parserConditions\":[{\"op1\":\"prefix\",\"compare\":\"=\",\"op2\":\"sqlserver\"},{\"op1\":\"nameGroups\",\"compare\":\">\",\"op2\":\"1\"}],\"qualifiedName\":\"mssql://{nameSpcBodyParts[0]}/{nameSpcNameVals['database']}/{nameGroups[0]}/{nameGroups[1]}\",\"purviewDataType\":\"azure_sql_table\",\"purviewPrefix\":\"mssql\"},{\"name\":\"synapseSql\",\"parserConditions\":[{\"op1\":\"prefix\",\"compare\":\"=\",\"op2\":\"sqlserver\"},{\"op1\":\"nameSpcBodyParts[0]\",\"compare\":\"contains\",\"op2\":\"azuresynapse\"}],\"qualifiedName\":\"mssql://{nameSpcBodyParts[0]}/{nameSpcNameVals['database']}/dbo/{nameGroups[0].parts[0]}\",\"purviewDataType\":\"azure_synapse_dedicated_sql_table\",\"purviewPrefix\":\"mssql\"},{\"name\":\"azureSQL\",\"parserConditions\":[{\"op1\":\"prefix\",\"compare\":\"=\",\"op2\":\"sqlserver\"}],\"qualifiedName\":\"mssql://{nameSpcBodyParts[0]}/{nameSpcNameVals['database']}/dbo/{nameGroups[0]}\",\"purviewDataType\":\"azure_sql_table\",\"purviewPrefix\":\"mssql\"},{\"name\":\"db2Jdbc\",\"parserConditions\":[{\"op1\":\"prefix\",\"compare\":\"=\",\"op2\":\"jdbc:db2\"}],\"qualifiedName\":\"db2://servers/{nameSpcBodyParts[0]}:{nameSpcBodyParts[1]}/databases/{nameSpcNameVals['Database']}/schemas/{nameSpcNameVals['CurrentSchema']}/tables/{nameGroups[0]}\",\"purviewDataType\":\"db2_table\",\"purviewPrefix\":\"db2\"},{\"name\":\"msqlJdbc\",\"parserConditions\":[{\"op1\":\"prefix\",\"compare\":\"=\",\"op2\":\"jdbc:mysql\"}],\"qualifiedName\":\"mysql://servers/{nameSpcBodyParts[0]}:{nameSpcBodyParts[1]}/dbs/{nameSpcNameVals['Database']}/tables/{nameGroups[0]}\",\"purviewDataType\":\"mysql_table\",\"purviewPrefix\":\"mysql\"},{\"name\":\"oracleJdbc\",\"parserConditions\":[{\"op1\":\"prefix\",\"compare\":\"=\",\"op2\":\"jdbc:oracle:thin:\"}],\"qualifiedName\":\"oracle://{nameSpcBodyParts[0]}/{nameSpcNameVals['Database']}/{nameGroups[0]}\",\"purviewDataType\":\"oracle_table\",\"purviewPrefix\":\"oracle\"}]}";
        }
    }
}