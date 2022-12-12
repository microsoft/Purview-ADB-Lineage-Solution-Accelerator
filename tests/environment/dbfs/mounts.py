# Databricks notebook source
import os

storage_acct_name = os.environ.get("STORAGE_SERVICE_NAME")
configs = {"fs.azure.account.auth.type": "OAuth",
          "fs.azure.account.oauth.provider.type": "org.apache.hadoop.fs.azurebfs.oauth2.ClientCredsTokenProvider",
          "fs.azure.account.oauth2.client.id": dbutils.secrets.get("purview-to-adb-kv", 'clientIdKey'),
          "fs.azure.account.oauth2.client.secret": dbutils.secrets.get("purview-to-adb-kv", 'clientSecretKey'),
          "fs.azure.account.oauth2.client.endpoint": f"https://login.microsoftonline.com/{dbutils.secrets.get('purview-to-adb-kv', 'tenant-id')}/oauth2/token"}

# COMMAND ----------

# Optionally, you can add <directory-name> to the source URI of your mount point.
try:
    dbutils.fs.mount(
      source = f"abfss://rawdata@{storage_acct_name}.dfs.core.windows.net/",
      mount_point = "/mnt/rawdata",
      extra_configs = configs)
except Exception as e:
    print(e)

# COMMAND ----------

try:
    dbutils.fs.mount(
      source = f"abfss://outputdata@{storage_acct_name}.dfs.core.windows.net/",
      mount_point = "/mnt/outputdata",
      extra_configs = configs)
except Exception as e:
    print(e)

# COMMAND ----------


