# Databricks notebook source
# %sql
# CREATE DATABASE IF NOT EXISTS notdefault;

# COMMAND ----------

# %sql
# CREATE TABLE IF NOT EXISTS notdefault.hiveExampleA (
# tableId INT,
# x INT
# );

# CREATE TABLE notdefault.hiveExampleOutput(
# tableId INT,
# x INT
# )

# COMMAND ----------

# %sql
# INSERT INTO notdefault.hiveExampleA (tableId, x) VALUES(1,2)

# COMMAND ----------

spark.sparkContext.setLogLevel("DEBUG")

# COMMAND ----------

# MAGIC %sql
# MAGIC INSERT INTO notdefault.hiveExampleOutput (tableId, x)
# MAGIC SELECT tableId, x
# MAGIC FROM notdefault.hiveExampleA

# COMMAND ----------

# MAGIC %md
# MAGIC # Exploring the File Path

# COMMAND ----------

# dbutils.fs.ls("/user/hive/warehouse/notdefault.db/hiveexamplea")
