// Databricks notebook source
// MAGIC %md
// MAGIC # SKip This

// COMMAND ----------

// MAGIC %sql
// MAGIC 
// MAGIC -- CREATE TABLE default.mysparktable(
// MAGIC -- id int
// MAGIC -- )

// COMMAND ----------

// MAGIC %python
// MAGIC 
// MAGIC # df = spark.createDataFrame([
// MAGIC #     (1,),(2,),(3,),(4,)
// MAGIC # ], ["id"]
// MAGIC # )
// MAGIC 
// MAGIC # df.createOrReplaceTempView("data")
// MAGIC 
// MAGIC # spark.sql("INSERT INTO default.mysparktable SELECT id from data")

// COMMAND ----------

// %sql
// SELECT *
// FROM default.mysparktable

// COMMAND ----------

// MAGIC %md
// MAGIC # Run from here

// COMMAND ----------

import org.apache.spark.sql.types.{StructType, StructField, IntegerType, StringType}

// COMMAND ----------

val storageServiceName = sys.env("STORAGE_SERVICE_NAME")
val storageContainerName = "rawdata"
val ouptutContainerName = "outputdata"
val abfssRootPath = "abfss://"+storageContainerName+"@"+storageServiceName+".dfs.core.windows.net"
val outputRootPath = "abfss://"+ouptutContainerName+"@"+storageServiceName+".dfs.core.windows.net"

val storageKey = dbutils.secrets.get("purview-to-adb-scope", "example-sa-key")

spark.conf.set("fs.azure.account.key."+storageServiceName+".dfs.core.windows.net", storageKey)

// COMMAND ----------

val exampleA = (
    spark.sql("SELECT id from default.mysparktable")
)


val exampleBSchema = StructType(
     StructField("id", IntegerType, true) ::
     StructField("city", StringType, false) ::
     StructField("stateAbbreviation", StringType, false) :: Nil)

val exampleB = (
    spark.read.format("csv")
  .schema(exampleBSchema)
  .option("header", true)
  .load(abfssRootPath+"/testcase/nine/exampleInputB/exampleInputB.csv")
)

// COMMAND ----------

val outputDf = exampleA.join(exampleB, exampleA("id") === exampleB("id"), "inner").drop(exampleB("id"))

outputDf.repartition(1).write.mode("overwrite").format("csv").save(outputRootPath+"/testcase/nine/spark-sql-table-in-abfss-out-folder/")

// COMMAND ----------

