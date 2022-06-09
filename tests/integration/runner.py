import argparse
import json
import os

PURVIEW_NAME = os.environ.get("PURVIEW_INTEGRATION_TARGET")

expected = [
  "notebook://shared/examples/abfss-in-abfss-out",
  "notebook://shared/examples/abfss-in-abfss-out-ouath",
  "notebook://shared/examples/abfss-in-abfss-out-root",
  "notebook://shared/examples/azuresql-in-azuresql-out",
  "notebook://shared/examples/delta-in-delta-merge",
  "notebook://shared/examples/delta-in-delta-out-abfss",
  "notebook://shared/examples/delta-in-delta-out-fs",
  "notebook://shared/examples/delta-in-delta-out-mnt",
  "notebook://shared/examples/intermix-languages",
  "notebook://shared/examples/mnt-in-mnt-out",
  "notebook://shared/examples/nested-parent",
  "notebook://shared/examples/spark-sql-table-in-abfss-out",
  "notebook://shared/examples/synapse-in-synapse-out",
  "notebook://shared/examples/synapse-in-wasbs-out",
  "notebook://shared/examples/synapse-wasbs-in-synapse-out",
  "notebook://shared/examples/wasbs-in-wasbs-out"
]


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("test_expectations", help="The json file that contains the expected assets qualified names.")
    parser.add_argument("workspace_id", help="The workspace id to be used in test expectations string replacement")
    parser.add_argument("job_id", help="The job id to be used in test expectations string replacement")
    parser.add_argument("--cleanup", action="store_true", help="Whether you should delete all the spark application and process type entities.")
    parser.add_argument("--dontwait", action="store_true", help="Don't wait for user input / approval, just exexcute cleanup command.")
    args, _ = parser.parse_known_args()

    from pyapacheatlas.core.client import PurviewClient
    from azure.identity import EnvironmentCredential
    

    cred = EnvironmentCredential()
    client = PurviewClient(
        account_name=PURVIEW_NAME,
        authentication=cred
    )

    type_search_filter = [
        {"entityType": "spark_process"}, 
        {"entityType": "spark_application"}, 
        {"entityType": "databricks_job"},
        {"entityType": "databricks_job_task"},
        {"entityType": "databricks_notebook"},
        {"entityType": "databricks_notebook_task"},
        {"entityType": "databricks_python_task"},
        {"entityType": "databricks_python_wheel_task"},
        {"entityType": "databricks_spark_jar_task"},
        {"entityType": "databricks_process"},
    ]


    if args.cleanup:
        resp = client.discovery.search_entities(
            "*", search_filter={"or": type_search_filter})
        counter = 0
        guids = []
        for e in resp:
            counter = counter+1
            # print(counter, e)
            guids.append(e["id"])

        if len(guids) > 0:
            if not args.dontwait:
                _ = input(f">>> Press enter to delete {len(guids)} entities. Ctrl+C to back out...")

            print(client.delete_entity(guids))
        else:
            print("No entities found.")
        exit()
    
    with open(args.test_expectations, 'r') as fp:
        expected = [e.replace("<WORKSPACE_ID>", args.workspace_id).replace("<JOB_ID>", args.job_id) for e in json.load(fp)]
        

    maximum_string_length = max([len(e) for e in expected])

    resp = client.discovery.search_entities(
        "*", search_filter={"or": type_search_filter})
    success = 0
    guids = []
    results = [e["qualifiedName"] for e in resp]

    for expected_qn in expected:
        is_found = "❌"
        if expected_qn in results:
            is_found = "✅"
            success += 1
        qn_len = len(expected_qn)
        padding = ((maximum_string_length - qn_len)+5)*" "
        print(expected_qn, padding, is_found)

    print(f"Summary: {success:0>2}/{len(expected):0>2}")
    print(success == len(expected), end="")
