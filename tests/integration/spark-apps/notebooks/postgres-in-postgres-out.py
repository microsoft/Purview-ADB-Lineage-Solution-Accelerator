# Databricks notebook source
host = dbutils.secrets.get("purview-to-adb-kv", "postgres-host")
port = "5432"
dbname = "postgres"
user = dbutils.secrets.get("purview-to-adb-kv", "postgres-admin-user")
password = dbutils.secrets.get("purview-to-adb-kv", "postgres-admin-password")
table_in = "people" # hardcoded based on populate-data-postgres.
table_out = "fruits"
sslmode = "require"

# COMMAND ----------

df = spark.read \
    .format("jdbc") \
    .option("url", f"jdbc:postgresql://{host}:{port}/{dbname}") \
    .option("dbtable", table_in) \
    .option("user", user) \
    .option("password", password) \
    .option("driver", "org.postgresql.Driver") \
    .option("ssl", False) \
    .load()

# COMMAND ----------

df.show()

# COMMAND ----------

df=df.withColumn("age", df.age-100)

# COMMAND ----------

df.show()

# COMMAND ----------

df.write \
    .format("jdbc") \
    .option("url", f"jdbc:postgresql://{host}:{port}/{dbname}") \
    .option("dbtable", table_out) \
    .option("user", user) \
    .option("password", password) \
    .option("driver", "org.postgresql.Driver") \
    .mode("overwrite") \
    .option("ssl", False) \
    .save()

# COMMAND ----------


