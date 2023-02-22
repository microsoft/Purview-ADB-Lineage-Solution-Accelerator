# Supported Data Sources and Limitations for the Azure Databricks to Purview Solution Accelerator Connector

This Databricks to Purview Solution Accelerator supports extracting information from Spark's logical plans, emits them in the standardized OpenLineage format, and translates that standard format to Apache Atlas / Microsoft Purview types.

The solution accelerator supports a limited set of data sources to be ingested into Microsoft Purview and can be extended further.

* [Azure Blob File System / Data Lake Gen2](#azure-blob-file-system-abfs)
* [Azure Databricks Mount Points](#azure-databricks-mount-points)
* [Azure Storage Blob](#azure-storage-blobs-wasb)
* [Azure Synapse SQL Pools](#azure-synapse-sql-pools)
* [Azure SQL DB](#azure-sql-db)
* [Delta Lake](#delta-lake-file-format)
<<<<<<< HEAD
* [Azure MySQL](#azure-mysql)
* [PostgreSQL](#postgresql)
* [Azure Data Explorer](#azure-data-explorer)
* [Azure Cosmos DB]()
=======
* [Azure Cosmos DB](#azure-cosmos-db)
>>>>>>> 85ddab3 (Update LIMITATIONS)
* [Other Data Sources and Limitations](#other-data-sources-and-limitations)
* [Column Level Mapping Supported Sources](#column-level-mapping-supported-sources)

## Connecting to Assets in Purview

For supported databases listed above, the Databricks to Purview Solution Accelerator will connect to a scanned asset present in Microsoft Purview.

For supported filestores listed above, the Databricks to Purview Solution Accelerator will connect to folders or resource sets.
* If you are reading a specific file in a folder (e.g. `container/path/to/some.csv`), the asset this solution connects to will be the folder and not the specific file (e.g. `container/path/to/`).
* If the folder contains a resource set, this solution will link to the resource set asset instead of the folder.

## Azure Blob File System (ABFS)

Supports Azure Data Lake Gen 2 through the [ABFS](https://hadoop.apache.org/docs/stable/hadoop-azure/abfs.html) connection built-into Apache Spark.

* [Databricks Credential Passthrough](https://docs.microsoft.com/en-us/azure/databricks/security/credential-passthrough/adls-passthrough) is not currently supported.

## Azure Databricks Mount Points

Supports mapping [Databricks Mount Points](https://docs.microsoft.com/en-us/azure/databricks/data/databricks-file-system#--mount-object-storage-to-dbfs) to the underlying storage location.

* Only mount points with ABFS(S) or WASB(S) storage locations can be mapped to their appropriate Purview type.
* [Databricks Credential Passthrough](https://docs.microsoft.com/en-us/azure/databricks/security/credential-passthrough/adls-passthrough) is not currently supported.

## Azure Storage Blobs (WASB)

Supports Azure Blob Storage through the [WASB](https://hadoop.apache.org/docs/stable/hadoop-azure/index.html) connection built-into Apache Spark.

* Only WASB paths with `blob.core.windows.net` in the host name will generate the correct Azure Blob Storage Purview type lineage.

## Azure Synapse SQL Pools

Supports querying Azure Synapse SQL Pools with the [Databricks Synapse Connector](https://docs.microsoft.com/en-us/azure/databricks/data/data-sources/azure/synapse-analytics)

Does not support:

* preAction / postAction (these sql statements are running in Synapse, NOT as a Spark job)
* query as data source: Lineage will show the input table as `dbo.COMPLEX`.
* Spark jobs that use Synapse tables in the same synapse workspace for input and output (all operations are done in Synapse in this scenario and no lineage is emitted).

Limited support for Azure Synapse as an output:

* Due to the implementation by Databricks, it will report output lineage to the staging folder path used to temporarily store the data before a Polybase / Copy command is executed inside of your Synapse SQL Pool.

## Azure SQL DB

Supports Azure SQL DB through the [Apache Spark Connector for Azure SQL DB](https://docs.microsoft.com/en-us/sql/connect/spark/connector?view=sql-server-ver15).

* If you specify the `dbTable` value without the database schema (e.g. `dbo`), the connector assumes you are using a default `dbo` schema.
  * For users and Service Principals with different default schemas, this may result in incorrect lineage.
  * This can be corrected by specifying the database schema in the Spark job.
* Does not support emitting lineage for cross-database table sources.
* Default configuration supports using multiple strings divided by dots to define a custom schema.  For example ```myschema.mytable```.  This will not function correctly if table names could contain dot characters in your organization.  In this case, you can delete the "azureSQLNonDboNoDotsInNames" section from the "OlToPurviewMappings" function configuration setting. Note that you would need to use bracket syntax to denote a custom schema.  For example ```[myschema].[my.table]```.

## Delta Lake File Format

Supports [Delta File Format](https://delta.io/).

* Does not support Delta on Spark 2 Databricks Runtimes.
* Does not currently support the MERGE INTO statement due to differences between proprietary Databricks and Open Source Delta implementations.
* Commands such as [Vacuum](https://docs.delta.io/latest/delta-utility.html#toc-entry-1) or [Optimize](https://docs.microsoft.com/en-us/azure/databricks/spark/latest/spark-sql/language-manual/delta-optimize) do not emit any lineage information and will not result in a Purview asset.

## Azure MySQL

Supports Azure MySQL through [JDBC](https://learn.microsoft.com/en-us/azure/databricks/external-data/jdbc).

## PostgreSQL

Supports both Azure PostgreSQL and on-prem/VM installations of PostgreSQL through [JDBC](https://learn.microsoft.com/en-us/azure/databricks/external-data/jdbc).

* If you specify the `dbTable` value without the database schema (e.g. `dbo`), the connector assumes you are using the default `public` schema.
  * For users and Service Principals with different default schemas, this may result in incorrect lineage.
  * This can be corrected by specifying the database schema in the Spark job.
* Default configuration supports using multiple strings divided by dots to define a custom schema.  For example ```myschema.mytable```.
* If you register and scan your postgres server as `localhost` in Microsoft Purview, but use the IP within the Databricks notebook, the assets will not be matched correctly. You need to use the IP when registering the Postgres server.  

## Azure Data Explorer

Supports Azure Data Explorer (aka Kusto) through the [Azure Data Explorer Connector for Apache Spark](https://learn.microsoft.com/en-us/azure/data-explorer/spark-connector)
## Azure Cosmos DB

Supports querying [Azure Cosmos DB (SQL API)](https://github.com/Azure/azure-sdk-for-java/tree/main/sdk/cosmos/azure-cosmos-spark_3_2-12)

## Other Data Sources and Limitations

### Lineage for Unsupported Data Sources

The OpenLineage project supports emitting lineage for other data sources, such as HDFS, S3, GCP, BigQuery, Apache Iceberg and more. However, this connector does not provide translation of these other data sources not mentioned in the list above.

Instead, any unknown data type will land in Microsoft Purview as a "dummy" type.

We welcome [contributions](./CONTRIBUTING.md) to help map those types to official Purview types as they become available. Alternatively, in your implementation, you may choose to extend this solution to map to your own custom types.

### Case Sensitivity

Microsoft Purview's Fully Qualified Names are case sensitive. Spark Jobs may have data sources connections that are not in the proper casing as on the data source (e.g. `dbo.InputTable` might be the physical table's name in the SQL db but a Spark query may reference the table as `dbo.iNpUtTaBlE`).

As a result, this solution attempts to find the best matching *existing* asset. If no existing asset is found to match based on qualified name, the data source name as found in the Spark query will be used toe create a dummy asset. On a subsequent scan of the data source in Purview and another run of the Spark query with the connector enabled will resolve the linkage.

### Data Factory

The solution currently reflects the unfriendly job name provided by Data Factory to Databricks as noted in [issue 72](https://github.com/microsoft/Purview-ADB-Lineage-Solution-Accelerator/issues/72#issuecomment-1211202405). You will see jobs with names similar to `ADF_<factory name>_<pipeline name>_<activity name>_<guid>`.

### Hive Metastore / Delta Table Names

The solution currently does not support emitting the Hive Metastore / Delta table SQL names. For example, if you have a Delta table name `default.events` and it's physical location is `abfss://container@storage/path`, the solution will report `abfss://container@storage/path`.

OpenLineage is considering adding this feature with [OpenLineage#435](https://github.com/OpenLineage/OpenLineage/issues/435).

### Spark Streaming

The solution does not currently support Spark Streaming. OpenLineage will emit events on Spark Streaming events, however, OpenLineage does not currently support retrieving the input and output data sources.

If you are using [forEachBatch](https://spark.apache.org/docs/latest/structured-streaming-programming-guide.html#foreachbatch) and use a supported output data source, it is possible to receive lineage. However, you will receive a lineage event for each execution of the `forEachBatch`. This may lead to increased Purview API calls per second and result in [increased capacity unit charges](https://azure.microsoft.com/en-us/pricing/details/azure-purview/).

At this time, we encourage clusters running Spark Structured Streaming jobs to not have the OpenLineage jar installed.

### Spark 2 Support

The solution supports Spark 2 job cluster jobs. Databricks has removed Spark 2 from it's Long Term Support program.

### Spark 3.3+ Support

The solution supports Spark 3.0, 3.1, 3.2, and 3.3 interactive and job clusters. The solution has been tested on the Databricks Runtime 11.3LTS version.

### Private Endpoints on Microsoft Purview

Currently, the solution does not support pushing lineage to a Private Endpoint backed Microsoft Purview service. The solution may be customized to deploy the Azure Function to connect to Microsoft Purview. Consider reviewing the documentation to [Connect privately and securely to your Microsoft Purview account](https://docs.microsoft.com/en-us/azure/purview/catalog-private-link-account-portal).

## Column Level Mapping Supported Sources

Starting with OpenLineage 0.18.0 and release 2.3.0 of the solution accelerator, we support emitting column level mapping from the following sources and their combinations:

* Read / Write to ABFSS file paths (mount or explicit path `abfss://`)
* Read / Write to WASBS file paths (mount or explicit path `wasbs://`)
* Read / Write to the default metastore in Azure Databricks
  * Does NOT support custom hive metastores

### Column Mapping Support for Delta Format

* Delta Merge statements are supported when the table is stored in the default metastore
* Delta to Delta is NOT supported at this time
