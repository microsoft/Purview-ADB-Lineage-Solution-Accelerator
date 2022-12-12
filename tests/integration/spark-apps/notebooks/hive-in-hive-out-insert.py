# Databricks notebook source
# MAGIC %sql
# MAGIC CREATE TABLE IF NOT EXISTS default.hiveExampleA000 (
# MAGIC tableId INT,
# MAGIC x INT
# MAGIC );
# MAGIC 
# MAGIC CREATE TABLE IF NOT EXISTS default.hiveExampleOutput000(
# MAGIC tableId INT,
# MAGIC x INT
# MAGIC )

# COMMAND ----------

# MAGIC %sql
# MAGIC INSERT INTO default.hiveExampleOutput000 (tableId, x)
# MAGIC SELECT tableId, x
# MAGIC FROM default.hiveExampleA000

# COMMAND ----------


