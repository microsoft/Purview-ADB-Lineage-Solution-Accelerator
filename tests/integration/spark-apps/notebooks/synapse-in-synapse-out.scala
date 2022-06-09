// Databricks notebook source
//Defining the service principal credentials for the Azure storage account
val tenantid = dbutils.secrets.get("purview-to-adb-scope", "tenant-id")
val synapseStorageAccount = sys.env("SYNAPSE_STORAGE_SERVICE_NAME")

spark.conf.set("fs.azure.account.auth.type", "OAuth")
spark.conf.set("fs.azure.account.oauth.provider.type",  "org.apache.hadoop.fs.azurebfs.oauth2.ClientCredsTokenProvider")
spark.conf.set("fs.azure.account.oauth2.client.id", dbutils.secrets.get("purview-to-adb-scope", "project-spn-client-id"))
spark.conf.set("fs.azure.account.oauth2.client.secret", dbutils.secrets.get("purview-to-adb-scope", "project-spn-secret"))
spark.conf.set("fs.azure.account.oauth2.client.endpoint", "https://login.microsoftonline.com/" + tenantid + "/oauth2/token")

//# Defining a separate set of service principal credentials for Azure Synapse Analytics (If not defined, the connector will use the Azure storage account credentials)
spark.conf.set("spark.databricks.sqldw.jdbc.service.principal.client.id", dbutils.secrets.get("purview-to-adb-scope", "project-spn-client-id"))
spark.conf.set("spark.databricks.sqldw.jdbc.service.principal.client.secret", dbutils.secrets.get("purview-to-adb-scope", "project-spn-secret"))
spark.conf.set("fs.azure.account.key."+synapseStorageAccount+".blob.core.windows.net", dbutils.secrets.get("purview-to-adb-scope", "synapse-storage-key"))

// COMMAND ----------

//Azure Synapse related settings
val dwDatabase = "SQLPool1"
val dwServer = sys.env("SYNAPSE_SERVICE_NAME")+".sql.azuresynapse.net"
val dwUser = dbutils.secrets.get("purview-to-adb-scope", "synapse-query-username")
val dwPass = dbutils.secrets.get("purview-to-adb-scope", "synapse-query-password")
val dwJdbcPort =  "1433"
val dwJdbcExtraOptions = "encrypt=true;trustServerCertificate=true;hostNameInCertificate=*.database.windows.net;loginTimeout=30;"
val sqlDwUrl = "jdbc:sqlserver://" + dwServer + ":" + dwJdbcPort + ";database=" + dwDatabase + ";user=" + dwUser+";password=" + dwPass + ";" + dwJdbcExtraOptions

val blobStorage = synapseStorageAccount+".blob.core.windows.net"
val blobContainer = "temp"
val blobAccessKey =  dbutils.secrets.get("purview-to-adb-scope", "synapse-storage-key")
val tempDir = "wasbs://" + blobContainer + "@" + blobStorage +"/tempfolder"

// COMMAND ----------

val exampleA = (
  spark.read.format("com.databricks.spark.sqldw")
  .option("url", sqlDwUrl)
  .option("tempDir", tempDir)
  .option("forwardSparkAzureStorageCredentials", "true")
  .option("dbTable", "exampleInputA")
  .load()
)

//Thread.sleep(50000)

val exampleB = (
  spark.read.format("com.databricks.spark.sqldw")
  .option("url", sqlDwUrl)
  .option("tempDir", tempDir)
  .option("forwardSparkAzureStorageCredentials", "true")
  .option("dbTable", "Sales.Region")
  .load()
)

// COMMAND ----------

val outputDf = exampleA.join(exampleB, exampleA("id") === exampleB("id"), "inner").drop(exampleB("id"))
outputDf
.write
.mode("overwrite")
.format("com.databricks.spark.sqldw")
.option("url", sqlDwUrl)
.option("forwardSparkAzureStorageCredentials", "true")
.option("dbTable", "exampleOutput")
.option("tempDir", tempDir)
.save()



// COMMAND ----------

