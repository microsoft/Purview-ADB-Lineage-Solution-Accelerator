// Databricks notebook source
// MAGIC %md
// MAGIC ## This one fails with Spark 2.x currently in cell 6 - 
// MAGIC org.apache.spark.SparkException: Job aborted due to stage failure: Task 0 in stage 2.0 failed 4 times, most recent failure: Lost task 0.3 in stage 2.0 (TID 21, 10.139.64.8, executor 0): java.lang.NoSuchMethodError: com.microsoft.sqlserver.jdbc.SQLServerBulkCopy.writeToServer(Lcom/microsoft/sqlserver/jdbc/ISQLServerBulkData;)V

// COMMAND ----------

import org.apache.spark.sql.types.{StructType, StructField, IntegerType, StringType}

// COMMAND ----------

val server_name = "jdbc:sqlserver://FILL-IN-CONNECTION-STRING"
val database_name = "purview-to-adb-sqldb"
val url = server_name + ";" + "databaseName=" + database_name + ";"

val username = dbutils.secrets.get("purview-to-adb-scope", "azuresql-username")
val password = dbutils.secrets.get("purview-to-adb-scope", "azuresql-password")

// COMMAND ----------

val exampleA =(
  spark.read
        .format("com.microsoft.sqlserver.jdbc.spark")
        .option("url", url)
        .option("dbtable", "dbo.exampleInputA")
        .option("user", username)
        .option("password", password)
    .load()
)

// COMMAND ----------

val exampleB =(
  spark.read
        .format("com.microsoft.sqlserver.jdbc.spark")
        .option("url", url)
        .option("dbtable", "exampleInputB")
        .option("user", username)
        .option("password", password)
    .load()
)

// COMMAND ----------

val exampleC =(
  spark.read
        .format("com.microsoft.sqlserver.jdbc.spark")
        .option("url", url)
        .option("dbtable", "nondbo.exampleInputC")
        .option("user", username)
        .option("password", password)
    .load()
)

// COMMAND ----------

val outputDf = exampleA.join(exampleB, exampleA("id") === exampleB("id"), "inner")
                       .drop(exampleB("id"))
                       .join(exampleC, exampleA("id") === exampleC("id"), "inner")
                       .drop(exampleC("id"))

// COMMAND ----------

outputDf.write
 .format("com.microsoft.sqlserver.jdbc.spark")
 .mode("append")
 .option("url", url)
 .option("dbtable", "exampleOutput")
 .option("user", username)
 .option("password", password)
 .option("schemaCheckEnabled", false)
 .save()

// COMMAND ----------

