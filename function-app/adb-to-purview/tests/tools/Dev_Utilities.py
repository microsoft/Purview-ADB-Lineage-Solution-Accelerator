# PyApacheAtlas packages
# Connect to Atlas via a Service Principal
from pyapacheatlas.auth import ServicePrincipalAuthentication
from pyapacheatlas.core import PurviewClient
from sys import exc_info
from traceback import format_exception

# COMMAND ----------


import localsettingsdutils as lsdu


# COMMAND ----------

    # Authenticate against your Atlas server
oauth = ServicePrincipalAuthentication(
       tenant_id= lsdu.TENANT_ID,
       client_id=lsdu.CLIENT_ID, 
       client_secret=lsdu.CLIENT_SECRET 

  )

client = PurviewClient(
        account_name = lsdu.PURVIEW_NAME,
        authentication=oauth
    )

# COMMAND ----------

def delete_list_entities(attribute, value):
    search_filter = {attribute:value}
    results = client.discovery.search_entities("", search_filter=search_filter)
    for result in results:
        print(result['qualifiedName'])
        print(result['id'])
        print(result['entityType'])
        client.delete_entity(guid=result['id'])

def test_list_entities(attribute, value):
    search_filter = {attribute:value}
    results = client.discovery.search_entities("", search_filter=search_filter)
    cnt = 0
    for result in results:
        print(result['qualifiedName'])
        print(result['id'])
        print(result['entityType'])
        cnt+=1
    print(f"num items: {cnt}")

# COMMAND EXAMPLE ----------

# Note: may have to run this more than once if there are a large number of entities
# test_list_entities('assetType','Purview Custom Connector')
# test_list_entities('assetType','spark')
# test_list_entities('assetType','Databricks')

# Uncomment the below to delete all solution entities
# delete_list_entities('assetType','Purview Custom Connector')
# delete_list_entities('assetType','spark')
delete_list_entities('assetType','Databricks')

