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

val exampleASchema = StructType(
     StructField("id", IntegerType, true) ::
     StructField("postalCode", StringType, false) ::
     StructField("streetAddress", StringType, false) :: Nil)

val exampleA = (
    spark.read.format("csv")
  .schema(exampleASchema)
  .option("header", true)
  .load(abfssRootPath+"/testcase/namewithperiods/exampleInputA/exampleInputA.csv")
)

// COMMAND ----------

val outputDf = exampleA.drop(exampleA("id"))

outputDf.repartition(1).write.mode("overwrite").format("parquet").save(outputRootPath+"/testcase/namewithperiods/badname.parquet")

// COMMAND ----------


