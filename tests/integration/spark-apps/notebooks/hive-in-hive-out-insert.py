# Datricks notebook source
# %sql
# CREATE TABLE IF NOT EXISTS default.hiveExampleA000 (
# tableId INT,
# x INT
# );

# CREATE TABLE default.hiveExampleOutput000(
# tableId INT,
# x INT
# )

# COMMAND ----------

# %sql
# INSERT INTO default.hiveExampleA000 (tableId, x) VALUES(1,2)

# COMMAND ----------

# MAGIC %sql
# MAGIC INSERT INTO default.hiveExampleOutput000 (tableId, x)
# MAGIC SELECT tableId, x
# MAGIC FROM default.hiveExampleA000

# COMMAND ----------


