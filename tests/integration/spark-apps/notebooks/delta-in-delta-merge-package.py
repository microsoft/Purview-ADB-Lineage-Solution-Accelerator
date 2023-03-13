# Databricks notebook source
# MAGIC %md
# MAGIC # Delta Table using Python Package rather than SQL

# COMMAND ----------

import os
storageServiceName = os.environ.get("STORAGE_SERVICE_NAME")
storageContainerName = "rawdata"
abfssRootPath = "abfss://"+storageContainerName+"@"+storageServiceName+".dfs.core.windows.net"

storageKey = dbutils.secrets.get("purview-to-adb-kv", "storage-service-key")

spark.conf.set("fs.azure.account.auth.type."+storageServiceName+".dfs.core.windows.net", "OAuth")
spark.conf.set("fs.azure.account.oauth.provider.type."+storageServiceName+".dfs.core.windows.net", "org.apache.hadoop.fs.azurebfs.oauth2.ClientCredsTokenProvider")
spark.conf.set("fs.azure.account.oauth2.client.id."+storageServiceName+".dfs.core.windows.net", dbutils.secrets.get("purview-to-adb-kv", "clientIdKey"))
spark.conf.set("fs.azure.account.oauth2.client.secret."+storageServiceName+".dfs.core.windows.net", dbutils.secrets.get("purview-to-adb-kv", "clientSecretKey"))
spark.conf.set("fs.azure.account.oauth2.client.endpoint."+storageServiceName+".dfs.core.windows.net", "https://login.microsoftonline.com/"+dbutils.secrets.get("purview-to-adb-kv", "tenant-id")+"/oauth2/token")

# COMMAND ----------

from delta.tables import *

exampleInputA = DeltaTable.forPath(spark, abfssRootPath+"/testcase/delta-merge-using-delta-package/subfolder-a/productA/")
exampleInputB = DeltaTable.forPath(spark, abfssRootPath+"/testcase/delta-merge-using-delta-package/subfolder-b/productB/")

dfUpdates = exampleInputB.toDF()

# COMMAND ----------

(
    exampleInputA.alias('a')
    .merge(
        dfUpdates.alias('updates'),
        'a.id = updates.id'
    )
    .whenMatchedUpdate(
        set = {
            "id": "updates.id",
            "postalCode": "updates.postalCode",
            "streetAddress": "updates.streetAddress"
        }
    )
    .whenNotMatchedInsert(
        values = {
            "id": "updates.id",
            "postalCode": "updates.postalCode",
            "streetAddress": "updates.streetAddress"
        }
    )
    .execute()
)

# COMMAND ----------

# MAGIC %md
# MAGIC # For Experimenting

# COMMAND ----------

# %scala
# val exampleA = (
#   spark.read.format("delta")
#   .load(abfssRootPath+"/testcase/sixteen/exampleInputA")
# )

# val exampleB = (
#   spark.read.format("delta")
#   .load(abfssRootPath+"/testcase/sixteen/exampleInputB")
# )
# val outputDf = exampleA.join(exampleB, exampleA("id") === exampleB("id"), "inner").drop(exampleB("id"))
# outputDf.createOrReplaceTempView("outputDf")

# COMMAND ----------

# dfUpdates.createOrReplaceTempView("updates")

# COMMAND ----------

# %sql
# MERGE INTO deltadestination_tbl
# USING updates
# ON deltadestination_tbl.id = updates.id
# WHEN MATCHED THEN
#   UPDATE SET
#     id = updates.id,
#     postalcode = updates.postalcode,
#     streetAddress = updates.streetAddress
# WHEN NOT MATCHED
#   THEN INSERT *

# COMMAND ----------


