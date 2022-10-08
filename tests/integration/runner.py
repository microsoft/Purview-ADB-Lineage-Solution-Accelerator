# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

import argparse
import json
import os

PURVIEW_NAME = os.environ.get("PURVIEW_INTEGRATION_TARGET")

def eval_test_status(expected_qn:str, test_results:str, maximum_string_length:int) -> int:
    """
    Print ❌ for failures and ✅ for success while keeping a count of successes.
    Returns the number of successes.
    """
    success = 0
    is_found = "❌"
    if expected_qn in test_results:
        is_found = "✅"
        success += 1
    qn_len = len(expected_qn)
    padding = ((maximum_string_length - qn_len)+5)*" "
    print(expected_qn, padding, is_found)
    return success


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

    guids = []
    results = [e["qualifiedName"] for e in resp]

    searchable_success = 0
    expected_processes = []
    for expected_qn in expected:
        # Ignore the process entities since they don't show up in search
        # Save them for later querying via the get entities api
        if "/processes/" in expected_qn:
            expected_processes.append(expected_qn)
            continue
        searchable_success += eval_test_status(expected_qn, results, maximum_string_length)
    
    # Get the processes expected from Purview
    # If this ever gets larger than a hundred entities, may need to batch up calls
    processes_from_purview = client.get_entity(
        qualifiedName=expected_processes,
        typeName="databricks_process"
    )
    processes_from_purview_qualifiedNames = []
    for proc in processes_from_purview.get("entities", []):
        processes_from_purview_qualifiedNames.append(proc.get("attributes", {}).get("qualifiedName"))
    
    # Evaluate the processes
    process_success = 0
    for expected_proc_qn in expected_processes:
        process_success += eval_test_status(
            expected_qn=expected_proc_qn, 
            test_results=processes_from_purview_qualifiedNames,
            maximum_string_length=maximum_string_length
        )    

    total_successes = searchable_success + process_success
    print(f"Summary: {total_successes:0>2}/{len(expected):0>2}")
    print(total_successes == len(expected), end="")
