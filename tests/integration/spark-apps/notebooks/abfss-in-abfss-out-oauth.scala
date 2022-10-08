// Databricks notebook source
import org.apache.spark.sql.types.{StructType, StructField, IntegerType, StringType}

// COMMAND ----------

val storageServiceName = sys.env("STORAGE_SERVICE_NAME")
val storageContainerName = "rawdata"
val ouptutContainerName = "outputdata"
val abfssRootPath = "abfss://"+storageContainerName+"@"+storageServiceName+".dfs.core.windows.net"
val outputRootPath = "abfss://"+ouptutContainerName+"@"+storageServiceName+".dfs.core.windows.net"

spark.conf.set("fs.azure.account.auth.type."+storageServiceName+".dfs.core.windows.net", "OAuth")
spark.conf.set("fs.azure.account.oauth.provider.type."+storageServiceName+".dfs.core.windows.net", "org.apache.hadoop.fs.azurebfs.oauth2.ClientCredsTokenProvider")
spark.conf.set("fs.azure.account.oauth2.client.id."+storageServiceName+".dfs.core.windows.net", dbutils.secrets.get("purview-to-adb-scope", "project-spn-client-id"))
spark.conf.set("fs.azure.account.oauth2.client.secret."+storageServiceName+".dfs.core.windows.net", dbutils.secrets.get("purview-to-adb-scope", "project-spn-secret"))
spark.conf.set("fs.azure.account.oauth2.client.endpoint."+storageServiceName+".dfs.core.windows.net", "https://login.microsoftonline.com/"+dbutils.secrets.get("purview-to-adb-scope", "tenant-id")+"/oauth2/token")


// COMMAND ----------

val exampleASchema = StructType(
     StructField("id", IntegerType, true) ::
     StructField("postalCode", StringType, false) ::
     StructField("streetAddress", StringType, false) :: Nil)

val exampleA = (
    spark.read.format("csv")
  .schema(exampleASchema)
  .option("header", true)
  .load(abfssRootPath+"/testcase/two/exampleInputA/exampleInputA.csv")
)


val exampleBSchema = StructType(
     StructField("id", IntegerType, true) ::
     StructField("city", StringType, false) ::
     StructField("stateAbbreviation", StringType, false) :: Nil)

val exampleB = (
    spark.read.format("csv")
  .schema(exampleBSchema)
  .option("header", true)
  .load(abfssRootPath+"/testcase/two/exampleInputB/exampleInputB.csv")
)

// COMMAND ----------

val outputDf = exampleA.join(exampleB, exampleA("id") === exampleB("id"), "inner").drop(exampleB("id"))

outputDf.repartition(1).write.mode("overwrite").format("csv").save(outputRootPath+"/testcase/two/abfss-in-abfss-out-oauth-folder/")

// COMMAND ----------

