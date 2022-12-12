# Databricks notebook source
# MAGIC %sql
# MAGIC CREATE DATABASE IF NOT EXISTS notdefault;

# COMMAND ----------

# MAGIC %sql
# MAGIC CREATE TABLE IF NOT EXISTS notdefault.hiveExampleA (
# MAGIC tableId INT,
# MAGIC x INT
# MAGIC );

# MAGIC CREATE TABLE IF NOT EXISTS notdefault.hiveExampleOutput(
# MAGIC tableId INT,
# MAGIC x INT
# MAGIC )

# COMMAND ----------

# MAGIC %sql
# MAGIC INSERT INTO notdefault.hiveExampleA (tableId, x) VALUES(1,2)

# COMMAND ----------

# MAGIC %sql
# MAGIC INSERT INTO notdefault.hiveExampleOutput (tableId, x)
# MAGIC SELECT tableId, x
# MAGIC FROM notdefault.hiveExampleA

# COMMAND ----------
