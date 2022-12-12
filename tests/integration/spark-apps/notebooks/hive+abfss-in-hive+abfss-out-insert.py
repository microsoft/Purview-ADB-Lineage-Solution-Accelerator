# Databricks notebook source
import os

storageServiceName = os.environ.get("STORAGE_SERVICE_NAME")
storageContainerName = "rawdata"
ouptutContainerName = "outputdata"
abfssRootPath = "abfss://"+storageContainerName+"@"+storageServiceName+".dfs.core.windows.net"
outputRootPath = "abfss://"+ouptutContainerName+"@"+storageServiceName+".dfs.core.windows.net"

storageKey = dbutils.secrets.get("purview-to-adb-kv", "storage-service-key")

spark.conf.set("fs.azure.account.key."+storageServiceName+".dfs.core.windows.net", storageKey)
spark.conf.set('spark.query.rootPath',abfssRootPath)
spark.conf.set('query.outputPath',outputRootPath)

# COMMAND ----------

spark.sql(f"""
CREATE TABLE IF NOT EXISTS default.testSample (
tableId INT,
x INT
)
LOCATION 'abfss://rawdata@{storageServiceName}.dfs.core.windows.net/testcase/twentyone/exampleInputA/'
;
"""
)

# COMMAND ----------

spark.sql(f"""
CREATE TABLE IF NOT EXISTS default.hiveExampleOutput001 (
tableId INT,
x INT
)
LOCATION 'abfss://rawdata@{storageServiceName}.dfs.core.windows.net/testcase/twentyone/exampleOutput/'
;
"""
)

# COMMAND ----------

# MAGIC %sql
# MAGIC INSERT INTO default.hiveExampleOutput001 (tableId, x)
# MAGIC SELECT tableId, x
# MAGIC FROM default.hiveExampleA001

# COMMAND ----------


