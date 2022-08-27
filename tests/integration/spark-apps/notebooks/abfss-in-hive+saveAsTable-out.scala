// Databricks notebook source
// Seq(
//   ("someId001", "Foo", 2, true), 
//   ("someId002", "Bar", 2, false)
// ).toDF("id","name","age","isAlive")
// .write.format("delta")
// .save(abfssRootPath+"/testcase/abfss-in-hive+saveAsTable-out/exampleInputA/")

// COMMAND ----------

//spark.sparkContext.setLogLevel("DEBUG")

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

val exampleA = (
    spark.read.format("delta")
  .load(abfssRootPath+"/testcase/abfss-in-hive+saveAsTable-out/exampleInputA/")
)

// COMMAND ----------

exampleA.write.mode("overwrite").saveAsTable("abfssInHiveSaveAsTableOut")
