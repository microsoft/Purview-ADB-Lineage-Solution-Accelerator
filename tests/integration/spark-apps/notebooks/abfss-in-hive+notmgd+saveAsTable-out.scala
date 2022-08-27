// Databricks notebook source
// Seq(
//   ("someId001", "Foo", 2, true), 
//   ("someId002", "Bar", 2, false)
// ).toDF("id","name","age","isAlive")
// .write.format("delta")
// .save(abfssRootPath+"/testcase/abfss-in-hive+notmgd+saveAsTable-out/exampleInputA/")

// COMMAND ----------

import org.apache.spark.sql.types.{StructType, StructField, IntegerType, StringType}

// COMMAND ----------

val storageServiceName = sys.env("STORAGE_SERVICE_NAME")
val storageContainerName = "rawdata"
val ouptutContainerName = "outputdata"
val abfssRootPath = "abfss://"+storageContainerName+"@"+storageServiceName+".dfs.core.windows.net"
val outputRootPath = "abfss://"+ouptutContainerName+"@"+storageServiceName+".dfs.core.windows.net"

val storageKey = dbutils.secrets.get("purview-to-adb-scope", "storage-service-key")

spark.conf.set("fs.azure.account.key."+storageServiceName+".dfs.core.windows.net", storageKey)

// COMMAND ----------

// %sql
// DROP TABLE IF EXISTS default.abfssInHiveNotMgdSaveAsTableOut;
// CREATE TABLE IF NOT EXISTS default.abfssInHiveNotMgdSaveAsTableOut (
// id String,
// name String,
// age Integer,
// isAlive Boolean
// )
// LOCATION 'abfss://outputdata@<STORAGE_ACCT_NAME>.dfs.core.windows.net/testcase/abfss-in-hive+notmgd+saveAsTable-out/notMgdTable/'

// COMMAND ----------

// spark.sparkContext.setLogLevel("DEBUG")

// COMMAND ----------

val exampleA = (
    spark.read.format("delta")
  .load(abfssRootPath+"/testcase/abfss-in-hive+notmgd+saveAsTable-out/exampleInputA/")
)

// COMMAND ----------

exampleA.write.mode("append").saveAsTable("abfssInHiveNotMgdSaveAsTableOut")

// COMMAND ----------

// MAGIC %md
// MAGIC # Explore file paths

// COMMAND ----------

// val df = spark.sql("SELECT * FROM abfssInHiveNotMgdSaveAsTableOut")
// df.inputFiles

// COMMAND ----------


