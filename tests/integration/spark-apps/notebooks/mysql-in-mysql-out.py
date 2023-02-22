# Databricks notebook source
# MAGIC %scala
# MAGIC Class.forName("com.mysql.cj.jdbc.Driver")

# COMMAND ----------

host = dbutils.secrets.get("purview-to-adb-kv", "mysql-hostname")
user = dbutils.secrets.get("purview-to-adb-kv", "mysql-user")
password = dbutils.secrets.get("purview-to-adb-kv", "mysql-password")
database = "mydatabase" # hardcoded based on populate-data-mysql notebook.
table = "people" # hardcoded based on populate-data-mysql notebook.
port = "3306" # update if you use a non-default port
driver = "com.mysql.cj.jdbc.Driver"

# COMMAND ----------

url = f"jdbc:mysql://{host}:{port}/{database}"

df = (spark.read
  .format("jdbc")
  .option("driver", driver)
  .option("url", url)
  .option("dbtable", table)
  .option("user", user)
  .option("ssl", False)
  .option("password", password)
  .load()
)

# COMMAND ----------

df.show()

# COMMAND ----------

df=df.withColumn("age", df.age-100)

# COMMAND ----------

df.show()

# COMMAND ----------

df.write \
  .format("jdbc") \
  .option("driver", driver) \
  .option("url", url) \
  .option("dbtable", "fruits") \
  .option("user", user) \
  .option("ssl", False) \
  .mode("overwrite") \
  .option("password", password) \
  .save()

# COMMAND ----------


