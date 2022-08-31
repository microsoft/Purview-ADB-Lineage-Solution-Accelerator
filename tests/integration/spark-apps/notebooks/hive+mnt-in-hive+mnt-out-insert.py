# Databricks notebook source
# spark.sparkContext.setLogLevel("DEBUG")

# COMMAND ----------

# MAGIC %sql
# MAGIC CREATE TABLE IF NOT EXISTS default.hiveExampleMnt001 (
# MAGIC tableId INT,
# MAGIC x INT
# MAGIC )
# MAGIC LOCATION '/mnt/rawdata/testcase/twentyone/exampleInputA/'
# MAGIC ;
# MAGIC 
# MAGIC CREATE TABLE IF NOT EXISTS default.hiveExampleMntOutput001(
# MAGIC tableId INT,
# MAGIC x INT
# MAGIC )
# MAGIC LOCATION '/mnt/rawdata/testcase/twentyone/exampleOutput/'
# MAGIC ;

# COMMAND ----------

# %sql
# INSERT INTO default.hiveExampleMnt001 (tableId, x) VALUES(1,2)

# COMMAND ----------

# MAGIC %sql
# MAGIC INSERT INTO default.hiveExampleMntOutput001 (tableId, x)
# MAGIC SELECT tableId, x
# MAGIC FROM default.hiveExampleMnt001

# COMMAND ----------

# spark.read.table("default.hiveExampleOutput001").inputFiles()

# COMMAND ----------


