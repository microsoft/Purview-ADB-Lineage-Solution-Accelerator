from pyspark.sql import SparkSession


print("STARTING")
spark = SparkSession.builder.getOrCreate()

exampleA = (
    spark.read.format("csv")
    .option("header", True)
    .option("inferSchema", True)
    .load("/mnt/rawdata/testcase/twenty/exampleInputA")
)

exampleA.repartition(1).write.mode("overwrite").format("csv").save("/mnt/rawdata/testcase/twenty/output")
print("COMPLETED")
