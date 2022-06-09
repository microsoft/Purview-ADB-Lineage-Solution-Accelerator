// Databricks notebook source
val storageServiceName = sys.env("STORAGE_SERVICE_NAME")
val storageContainerName = "rawdata"
val ouptutContainerName = "outputdata"
val abfssRootPath = "abfss://"+storageContainerName+"@"+storageServiceName+".dfs.core.windows.net"
val outputRootPath = "abfss://"+ouptutContainerName+"@"+storageServiceName+".dfs.core.windows.net"

val storageKey = dbutils.secrets.get("purview-to-adb-scope", "example-sa-key")

//spark.conf.set("fs.azure.account.key."+storageServiceName+".dfs.core.windows.net", storageKey)
spark.conf.set("fs.azure.account.auth.type."+storageServiceName+".dfs.core.windows.net", "OAuth")
spark.conf.set("fs.azure.account.oauth.provider.type."+storageServiceName+".dfs.core.windows.net", "org.apache.hadoop.fs.azurebfs.oauth2.ClientCredsTokenProvider")
spark.conf.set("fs.azure.account.oauth2.client.id."+storageServiceName+".dfs.core.windows.net", dbutils.secrets.get("purview-to-adb-scope", "project-spn-client-id"))
spark.conf.set("fs.azure.account.oauth2.client.secret."+storageServiceName+".dfs.core.windows.net", dbutils.secrets.get("purview-to-adb-scope", "project-spn-secret"))
spark.conf.set("fs.azure.account.oauth2.client.endpoint."+storageServiceName+".dfs.core.windows.net", "https://login.microsoftonline.com/"+dbutils.secrets.get("purview-to-adb-scope", "tenant-id")+"/oauth2/token")

// COMMAND ----------

val exampleA = (
  spark.read.format("delta")
  .load(abfssRootPath+"/testcase/six/exampleInputA")
)

val exampleB = (
  spark.read.format("delta")
  .load(abfssRootPath+"/testcase/six/exampleInputB")
)

// COMMAND ----------

val outputDf = exampleA.join(exampleB, exampleA("id") === exampleB("id"), "inner").drop(exampleB("id"))

//WORKS
outputDf.write.format("delta").mode("append").save("/mnt/outputdata/testcase/six/delta-in-delta-out-mnt-folder")