import json
import os

PURVIEW_NAME = os.environ.get("PURVIEW_INTEGRATION_TARGET", "purview-to-adb-purview")

from pyapacheatlas.core.client import PurviewClient
from azure.identity import AzureCliCredential


cred = AzureCliCredential()
client = PurviewClient(
    account_name=PURVIEW_NAME,
    authentication=cred
)


resp = client.discovery.search_entities(
            "*", search_filter={"or": [
                {"entityType": "spark_process"}, 
                {"entityType": "spark_application"}, 
                {"entityType": "databricks_job"},
                {"entityType": "databricks_notebook"},
                {"entityType": "databricks_notebook_task"},
                {"entityType": "databricks_process"},
            ]})
results = [e["qualifiedName"] for e in resp]

print(json.dumps(results, indent=2))
