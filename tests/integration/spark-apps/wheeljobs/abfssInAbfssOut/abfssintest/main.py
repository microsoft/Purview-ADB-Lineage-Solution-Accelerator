# Databricks notebook source
from pyspark.sql import SparkSession
from pyspark.sql.types import StructField, IntegerType, StringType, StructType
from pyspark.sql import functions as pyf
from pyspark.dbutils import DBUtils
import os


def runapp():
    print("STARTING")
    spark = SparkSession.builder.getOrCreate()
    dbutils = DBUtils(spark)

    storageServiceName = os.environ.get("STORAGE_SERVICE_NAME")
    storageContainerName = "rawdata"
    ouptutContainerName = "outputdata"
    abfssRootPath = "abfss://"+storageContainerName+"@"+storageServiceName+".dfs.core.windows.net"
    outputRootPath = "abfss://"+ouptutContainerName+"@"+storageServiceName+".dfs.core.windows.net"

    storageKey = dbutils.secrets.get("purview-to-adb-scope", "storage-service-key")

    spark.conf.set("fs.azure.account.key."+storageServiceName+".dfs.core.windows.net", storageKey)


    exampleASchema = StructType([
        StructField("id", IntegerType(), True),
        StructField("postalCode", StringType(), False),
        StructField("streetAddress", StringType(), False)
    ])

    exampleA = (
        spark.read.format("csv")
    .schema(exampleASchema)
    .option("header", True)
    .load(abfssRootPath+"/testcase/seventeen/exampleInputA/exampleInputA.csv")
    )


    exampleBSchema = StructType([
        StructField("id", IntegerType(), True),
        StructField("city", StringType(), False),
        StructField("stateAbbreviation", StringType(), False)
    ])

    exampleB = (
        spark.read.format("csv")
    .schema(exampleBSchema)
    .option("header", True)
    .load(abfssRootPath+"/testcase/seventeen/exampleInputB/exampleInputB.csv")
    )

    outputDf = exampleA.join(exampleB, ["id"], "inner")

    outputDf.repartition(1).write.mode("overwrite").format("csv").save(outputRootPath+"/testcase/seventeen/abfss-in-abfss-out-folder/")
    print("COMPLETED")

