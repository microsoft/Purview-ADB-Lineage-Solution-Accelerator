// Databricks notebook source
// MAGIC %md
// MAGIC # SETUP
// MAGIC 
// MAGIC * Need to create the delta table based on the outputDf (command 4)
// MAGIC * Need to be sure the delta location is valid (e.g. testcase/X/) since it isn't variable driven

// COMMAND ----------

val storageServiceName = sys.env("STORAGE_SERVICE_NAME")
val storageContainerName = "rawdata"
val abfssRootPath = "abfss://"+storageContainerName+"@"+storageServiceName+".dfs.core.windows.net"

val storageKey = dbutils.secrets.get("purview-to-adb-scope", "example-sa-key")

spark.conf.set("fs.azure.account.auth.type."+storageServiceName+".dfs.core.windows.net", "OAuth")
spark.conf.set("fs.azure.account.oauth.provider.type."+storageServiceName+".dfs.core.windows.net", "org.apache.hadoop.fs.azurebfs.oauth2.ClientCredsTokenProvider")
spark.conf.set("fs.azure.account.oauth2.client.id."+storageServiceName+".dfs.core.windows.net", dbutils.secrets.get("purview-to-adb-scope", "project-spn-client-id"))
spark.conf.set("fs.azure.account.oauth2.client.secret."+storageServiceName+".dfs.core.windows.net", dbutils.secrets.get("purview-to-adb-scope", "project-spn-secret"))
spark.conf.set("fs.azure.account.oauth2.client.endpoint."+storageServiceName+".dfs.core.windows.net", "https://login.microsoftonline.com/"+dbutils.secrets.get("purview-to-adb-scope", "tenant-id")+"/oauth2/token")

// COMMAND ----------

val exampleA = (
  spark.read.format("delta")
  .load(abfssRootPath+"/testcase/sixteen/exampleInputA")
)

val exampleB = (
  spark.read.format("delta")
  .load(abfssRootPath+"/testcase/sixteen/exampleInputB")
)
val outputDf = exampleA.join(exampleB, exampleA("id") === exampleB("id"), "inner").drop(exampleB("id"))
outputDf.createOrReplaceTempView("outputDf")

// COMMAND ----------

// This is ran only once
// %sql
// CREATE TABLE testcasesixteen
// USING DELTA
// LOCATION "abfss://outputdata@STORAGEACCOUNTNAME.dfs.core.windows.net/testcase/sixteen/exampleOutput/"
// AS
// SELECT * FROM outputDf

// COMMAND ----------

// MAGIC %sql
// MAGIC MERGE INTO default.testcasesixteen
// MAGIC USING outputDf
// MAGIC ON outputDf.id = testcasesixteen.id
// MAGIC WHEN MATCHED THEN
// MAGIC   UPDATE SET
// MAGIC     id = outputDf.id,
// MAGIC     postalcode = outputDf.postalcode,
// MAGIC     streetaddress = outputDf.streetaddress,
// MAGIC     city = outputDf.city,
// MAGIC     stateAbbreviation = outputDf.stateAbbreviation
// MAGIC WHEN NOT MATCHED
// MAGIC   THEN INSERT *

// COMMAND ----------

