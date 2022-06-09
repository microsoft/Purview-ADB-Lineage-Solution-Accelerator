// Databricks notebook source
// MAGIC %run ./nested-child

// COMMAND ----------

outputDf.repartition(1).write.mode("overwrite").format("csv").save(outputRootPath+"/testcase/eight/nested-parent-folder/")

// COMMAND ----------

