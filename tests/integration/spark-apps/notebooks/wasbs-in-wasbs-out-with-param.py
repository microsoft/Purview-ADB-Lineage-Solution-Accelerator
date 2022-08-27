# Databricks notebook source
# MAGIC %md
# MAGIC # Sample Databricks Lineage Extraction witrh param

# COMMAND ----------

myval = dbutils.widgets.text('mayval','')
print(myval)

# COMMAND ----------

key = dbutils.secrets.get("purview-to-adb-scope", "storage-service-key")

spark.conf.set(
  "fs.azure.account.key.<STORAGEACCOUNTNAME>.blob.core.windows.net",
  key)

# COMMAND ----------

retail = (
    spark.read.csv("wasbs://rawdata@<STORAGE_ACCT_NAME>.blob.core.windows.net/retail/", inferSchema=True, header=True)
    .withColumnRenamed('Customer ID', 'CustomerId' )
    .drop("Invoice")
)
retail.write.mode("overwrite").parquet("wasbs://outputdata@<STORAGE_ACCT_NAME>.blob.core.windows.net/retail/wasbdemo")

# COMMAND ----------

display(retail.take(2))

# COMMAND ----------

retail2 = spark.read.parquet("wasbs://outputdata@<STORAGE_ACCT_NAME>.blob.core.windows.net/retail/wasbdemo")
retail2 = retail2.withColumnRenamed('Quantity', 'QuantitySold').drop('Country')
retail2.write.mode("overwrite").parquet("wasbs://outputdata@<STORAGE_ACCT_NAME>.blob.core.windows.net/retail/wasbdemo_updated")

# COMMAND ----------

# display(retail2.take(2))
