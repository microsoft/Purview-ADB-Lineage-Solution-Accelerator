// Databricks notebook source
// %md
// ## This one fails with Spark 2.x currently in cell 6 - 
// org.apache.spark.SparkException: Job aborted due to stage failure: Task 0 in stage 2.0 failed 4 times, most recent failure: Lost task 0.3 in stage 2.0 (TID 21, 10.139.64.8, executor 0): java.lang.NoSuchMethodError: com.microsoft.sqlserver.jdbc.SQLServerBulkCopy.writeToServer(Lcom/microsoft/sqlserver/jdbc/ISQLServerBulkData;)V

// COMMAND ----------

import org.apache.spark.sql.types.{StructType, StructField, IntegerType, StringType}
import java.lang.{ClassNotFoundException}

// COMMAND ----------

val server_name = "jdbc:sqlserver://FILL-IN-CONNECTION-STRING"
val database_name = "purview-to-adb-sqldb"
val url = server_name + ";" + "database=" + database_name + ";"

val username = dbutils.secrets.get("purview-to-adb-scope", "azuresql-username")
val password = dbutils.secrets.get("purview-to-adb-scope", "azuresql-password")

// COMMAND ----------

var exampleA = spark.emptyDataFrame;
var exampleB = spark.emptyDataFrame;
var exampleC = spark.emptyDataFrame;
try{
  exampleA =(
    spark.read
          .format("com.microsoft.sqlserver.jdbc.spark")
          .option("url", url)
          .option("dbtable", "dbo.exampleInputA")
          .option("user", username)
          .option("password", password)
      .load()
  )
  exampleB =(
    spark.read
          .format("com.microsoft.sqlserver.jdbc.spark")
          .option("url", url)
          .option("dbtable", "exampleInputB")
          .option("user", username)
          .option("password", password)
      .load()
  )
  exampleC =(
    spark.read
          .format("com.microsoft.sqlserver.jdbc.spark")
          .option("url", url)
          .option("dbtable", "nondbo.exampleInputC")
          .option("user", username)
          .option("password", password)
      .load()
  )
}catch{
  case e: ClassNotFoundException => {
    exampleA =(
      spark.read
            .format("jdbc")
            .option("url", url)
            .option("dbtable", "dbo.exampleInputA")
            .option("user", username)
            .option("password", password)
        .load()
    ) 
    exampleB =(
      spark.read
            .format("jdbc")
            .option("url", url)
            .option("dbtable", "exampleInputB")
            .option("user", username)
            .option("password", password)
        .load()
    ) 
    exampleC =(
      spark.read
            .format("jdbc")
            .option("url", url)
            .option("dbtable", "nondbo.exampleInputC")
            .option("user", username)
            .option("password", password)
        .load()
    ) 
  }
}
exampleA.printSchema
exampleB.printSchema
exampleC.printSchema

// COMMAND ----------

val outputDf = exampleA.join(exampleB, exampleA("id") === exampleB("id"), "inner")
                       .drop(exampleB("id"))
                       .join(exampleC, exampleA("id") === exampleC("id"), "inner")
                       .drop(exampleC("id"))

// COMMAND ----------

try{
  outputDf.write
 .format("com.microsoft.sqlserver.jdbc.spark")
 .mode("append")
 .option("url", url)
 .option("dbtable", "exampleOutput")
 .option("user", username)
 .option("password", password)
 .option("schemaCheckEnabled", false)
 .save()
}catch{
  case e: ClassNotFoundException => {
    outputDf.select("id", "city", "stateAbbreviation").write
     .format("jdbc")
     .mode("append")
     .option("url", url)
     .option("dbtable", "exampleOutput")
     .option("user", username)
     .option("password", password)
     .option("schemaCheckEnabled", false)
     .save()
  }
}

// COMMAND ----------

