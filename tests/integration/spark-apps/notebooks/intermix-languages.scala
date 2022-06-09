// Databricks notebook source
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

// MAGIC %python
// MAGIC storageServiceName = sys.env("STORAGE_SERVICE_NAME")
// MAGIC storageContainerName = "rawdata"
// MAGIC ouptutContainerName = "outputdata"
// MAGIC abfssRootPath = "abfss://"+storageContainerName+"@"+storageServiceName+".dfs.core.windows.net"
// MAGIC outputRootPath = "abfss://"+ouptutContainerName+"@"+storageServiceName+".dfs.core.windows.net"

// COMMAND ----------

val exampleASchema = StructType(
     StructField("id", IntegerType, true) ::
     StructField("postalCode", StringType, false) ::
     StructField("streetAddress", StringType, false) :: Nil)

val exampleA = (
    spark.read.format("csv")
  .schema(exampleASchema)
  .option("header", true)
  .load(abfssRootPath+"/testcase/fifteen/exampleInputA/exampleInputA.csv")
)


val exampleBSchema = StructType(
     StructField("id", IntegerType, true) ::
     StructField("city", StringType, false) ::
     StructField("stateAbbreviation", StringType, false) :: Nil)

val exampleB = (
    spark.read.format("csv")
  .schema(exampleBSchema)
  .option("header", true)
  .load(abfssRootPath+"/testcase/fifteen/exampleInputB/exampleInputB.csv")
)

// COMMAND ----------

val outputDf = exampleA.join(exampleB, exampleA("id") === exampleB("id"), "inner").drop(exampleB("id"))
outputDf.createOrReplaceTempView("temptable")

// COMMAND ----------

// MAGIC %python
// MAGIC pyOutputDf = spark.sql("SELECT * FROM temptable")
// MAGIC pyOutputDf.repartition(1).write.mode("overwrite").format("csv").save(outputRootPath+"/testcase/fifteen/abfss-in-abfss-out-folder/")

// COMMAND ----------

