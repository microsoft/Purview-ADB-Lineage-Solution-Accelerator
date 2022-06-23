# Supported Data Sources and Limitations for the Azure Databricks to Purview Solution Accelerator Connector

This Databricks to Purview Solution Accelerator supports extracting information from Spark's logical plans, emits them in the standardized OpenLineage format, and translates that standard format to Apache Atlas / Microsoft Purview types.

The solution accelerator supports a limited set of data sources to be ingested into Microsoft Purview and can be extended further.

* [Azure Blob File System / Data Lake Gen2](#azure-blob-file-system-abfs)
* [Azure Databricks Mount Points](#azure-databricks-mount-points)
* [Azure Storage Blob](#azure-storage-blobs-wasb)
* [Azure Synapse SQL Pools](#azure-synapse-sql-pools)
* [Azure SQL DB](#azure-sql-db)
* [Delta Lake](#delta-lake-file-format)
* [Other Data Sources and Limitations](#other-data-sources-and-limitations)

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

## Delta Lake File Format

Supports [Delta File Format](https://delta.io/).

* Does not support Delta on Spark 2 Databricks Runtimes.
* Does not currently support the MERGE INTO statement due to differences between proprietary Databricks and Open Source Delta implementations.
* Commands such as [Vacuum](https://docs.delta.io/latest/delta-utility.html#toc-entry-1) or [Optimize](https://docs.microsoft.com/en-us/azure/databricks/spark/latest/spark-sql/language-manual/delta-optimize) do not emit any lineage information and will not result in a Purview asset.

## Other Data Sources and Limitations

### Lineage for Unsupported Data Sources

The OpenLineage project supports emitting lineage for other data sources, such as HDFS, S3, GCP, BigQuery, Apache Iceberg and more. However, this connector does not provide translation of these other data sources not mentioned in the list above.

Instead, any unknown data type will land in Microsoft Purview as a "dummy" type.

We welcome [contributions](./CONTRIBUTING.md) to help map those types to official Purview types as they become available. Alternatively, in your implementation, you may choose to extend this solution to map to your own custom types.

### Case Sensitivity

Microsoft Purview's Fully Qualified Names are case sensitive. Spark Jobs may have data sources connections that are not in the proper casing as on the data source (e.g. `dbo.InputTable` might be the physical table's name in the SQL db but a Spark query may reference the table as `dbo.iNpUtTaBlE`).

As a result, this solution attempts to find the best matching *existing* asset. If no existing asset is found to match based on qualified name, the data source name as found in the Spark query will be used toe create a dummy asset. On a subsequent scan of the data source in Purview and another run of the Spark query with the connector enabled will resolve the linkage.

### Column Level Mapping

The solution currently does not provide column level mapping within the Microsoft Purview lineage tab.

### Hive Metastore / Delta Table Names

The solution currently does not support emitting the Hive Metastore / Delta table SQL names. For example, if you have a Delta table name `default.events` and it's physical location is `abfss://container@storage/path`, the solution will report `abfss://container@storage/path`.

OpenLineage is considering adding this feature with [OpenLineage#435](https://github.com/OpenLineage/OpenLineage/issues/435).

### Spark Streaming

The solution does not currently support Spark Streaming. OpenLineage will emit events on Spark Streaming events, however, OpenLineage does not currently support retrieving the input and output data sources.

If you are using [forEachBatch](https://spark.apache.org/docs/latest/structured-streaming-programming-guide.html#foreachbatch) and use a supported output data source, it is possible to receive lineage. However, you will receive a lineage event for each execution of the `forEachBatch`. This may lead to increased Purview API calls per second and result in [increased capacity unit charges](https://azure.microsoft.com/en-us/pricing/details/azure-purview/).

At this time, we encourage clusters running Spark Structured Streaming jobs to not have the OpenLineage jar installed.

### Spark 2 Support

The solution supports Spark 2 job cluster jobs. Databricks has removed Spark 2 from it's Long Term Support program.

### Spark 3.2+ Support

The solution supports Spark 3.0 and 3.1 interactive and job clusters. We are working with the OpenLineage community to enable support of Spark 3.2 on Databricks Runtime 10.4 and higher.
