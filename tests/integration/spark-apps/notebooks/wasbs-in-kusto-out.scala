// Databricks notebook source
spark.sparkContext.setLogLevel("DEBUG")

// COMMAND ----------

import org.apache.commons.lang3.reflect.FieldUtils
import org.apache.commons.lang3.reflect.MethodUtils
import org.apache.spark.sql.execution.datasources.LogicalRelation
import com.microsoft.kusto.spark.datasink.KustoSinkOptions
import org.apache.spark.sql.{SaveMode, SparkSession}
import com.microsoft.kusto.spark.datasource.KustoSourceOptions
import org.apache.spark.SparkConf
import org.apache.spark.sql._
import com.microsoft.azure.kusto.data.ClientRequestProperties
import com.microsoft.kusto.spark.sql.extension.SparkExtension._
import com.microsoft.azure.kusto.data.ClientRequestProperties

// COMMAND ----------

// MAGIC %md
// MAGIC ### Write

// COMMAND ----------

val appId = dbutils.secrets.get("purview-to-adb-kv", "azurekusto-appid")
val appKey = dbutils.secrets.get("purview-to-adb-kv", "azurekusto-appsecret")
val uri = dbutils.secrets.get("purview-to-adb-kv", "azurekusto-uri")
val authorityId = dbutils.secrets.get("purview-to-adb-kv", "tenant-id")
val cluster = uri.replaceAll(".kusto.windows.net", "").replaceAll("https://", "")
val database = "database01" // this is hardcoded - so if changed in the bicep template, also needs to be changed here.
val table = "table01"

// COMMAND ----------

val storageServiceName = sys.env("STORAGE_SERVICE_NAME")
val storageContainerName = "rawdata"
val wasbsRootPath = "wasbs://"+storageContainerName+"@"+storageServiceName+".blob.core.windows.net"

val storageKey = dbutils.secrets.get("purview-to-adb-kv", "storage-service-key")

spark.conf.set("fs.azure.account.key."+storageServiceName+".blob.core.windows.net", storageKey)

val file_location = wasbsRootPath + "/testcase/one/exampleInputA/exampleInputA.csv"

// COMMAND ----------

val df = spark.read.option("header","true").csv(file_location)


// COMMAND ----------

df.write
  .format("com.microsoft.kusto.spark.datasource")
  .option(KustoSinkOptions.KUSTO_CLUSTER, cluster)
  .option(KustoSinkOptions.KUSTO_DATABASE, database)
  .option(KustoSinkOptions.KUSTO_TABLE, table)
  .option(KustoSinkOptions.KUSTO_AAD_APP_ID, appId)
  .option(KustoSinkOptions.KUSTO_AAD_APP_SECRET, appKey)
  .option(KustoSinkOptions.KUSTO_AAD_AUTHORITY_ID, authorityId)
  .option(KustoSinkOptions.KUSTO_TABLE_CREATE_OPTIONS, "CreateIfNotExist")
  .mode(SaveMode.Append)
  .save()

// COMMAND ----------


