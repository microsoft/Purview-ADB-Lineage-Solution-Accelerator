// Databricks notebook source
import org.apache.spark.sql.types.{StructType, StructField, IntegerType, StringType}

// COMMAND ----------

val storageServiceName = spark.conf.get("spark.openlineage.samplestorageaccount")
val storageContainerName = spark.conf.get("spark.openlineage.samplestoragecontainer")
val adlsRootPath = "wasbs://"+storageContainerName+"@"+storageServiceName+".blob.core.windows.net"

val storageKey = dbutils.secrets.get("purview-to-adb-kv", "storageAccessKey")

spark.conf.set("fs.azure.account.key."+storageServiceName+".blob.core.windows.net", storageKey)


// COMMAND ----------

val exampleASchema = StructType(
     StructField("id", IntegerType, true) ::
     StructField("postalCode", StringType, false) ::
     StructField("streetAddress", StringType, false) :: Nil)

val exampleA = (
    spark.read.format("csv")
  .schema(exampleASchema)
  .option("header", true)
  .load(adlsRootPath+"/examples/data/csv/exampleInputA/exampleInputA.csv")
)


val exampleBSchema = StructType(
     StructField("id", IntegerType, true) ::
     StructField("city", StringType, false) ::
     StructField("stateAbbreviation", StringType, false) :: Nil)

val exampleB = (
    spark.read.format("csv")
  .schema(exampleBSchema)
  .option("header", true)
  .load(adlsRootPath+"/examples/data/csv/exampleInputB/exampleInputB.csv")
)

// COMMAND ----------

val outputDf = exampleA.join(exampleB, exampleA("id") === exampleB("id"), "inner").drop(exampleB("id"))

outputDf.repartition(1).write.mode("overwrite").format("csv").save(adlsRootPath+"/examples/data/csv/exampleOutput/")
