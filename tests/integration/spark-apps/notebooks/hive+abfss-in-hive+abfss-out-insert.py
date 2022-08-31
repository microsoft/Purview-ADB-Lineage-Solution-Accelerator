# Databricks notebook source
import os

storageServiceName = os.environ.get("STORAGE_SERVICE_NAME")
storageContainerName = "rawdata"
ouptutContainerName = "outputdata"
abfssRootPath = "abfss://"+storageContainerName+"@"+storageServiceName+".dfs.core.windows.net"
outputRootPath = "abfss://"+ouptutContainerName+"@"+storageServiceName+".dfs.core.windows.net"

storageKey = dbutils.secrets.get("purview-to-adb-scope", "storage-service-key")

spark.conf.set("fs.azure.account.key."+storageServiceName+".dfs.core.windows.net", storageKey)
spark.conf.set('spark.query.rootPath',abfssRootPath)
spark.conf.set('query.outputPath',outputRootPath)

# COMMAND ----------

# MAGIC %sql
# MAGIC CREATE TABLE IF NOT EXISTS default.hiveExampleA001 (
# MAGIC tableId INT,
# MAGIC x INT
# MAGIC )
# MAGIC LOCATION 'abfss://rawdata@<STORAGE_ACCT_NAME>.dfs.core.windows.net/testcase/twentyone/exampleInputA/'
# MAGIC ;
# MAGIC 
# MAGIC CREATE TABLE IF NOT EXISTS default.hiveExampleOutput001(
# MAGIC tableId INT,
# MAGIC x INT
# MAGIC )
# MAGIC LOCATION 'abfss://rawdata@<STORAGE_ACCT_NAME>.dfs.core.windows.net/testcase/twentyone/exampleOutput/'
# MAGIC ;

# COMMAND ----------

# %sql
# INSERT INTO default.hiveExampleA001 (tableId, x) VALUES(1,2)

# COMMAND ----------

# MAGIC %sql
# MAGIC INSERT INTO default.hiveExampleOutput001 (tableId, x)
# MAGIC SELECT tableId, x
# MAGIC FROM default.hiveExampleA001

# COMMAND ----------

spark.read.table("default.hiveExampleOutput001").inputFiles()

# COMMAND ----------

dbutils.fs.ls("abfss://rawdata@<STORAGE_ACCT_NAME>.dfs.core.windows.net/testcase/twentyone/exampleInputA/")

# COMMAND ----------


