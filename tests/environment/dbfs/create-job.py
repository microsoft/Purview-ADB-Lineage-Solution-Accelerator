# https://learn.microsoft.com/en-us/azure/databricks/dev-tools/api/latest/workspace#--import
import argparse
import configparser
import json
import os

import requests


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--folder", default="./tests/integration/jobdefs")
    parser.add_argument("--ini", default="./tests/environment/config.ini")
    args = parser.parse_args()

    cfp = configparser.ConfigParser()

    cfp.read(args.ini)
    db_host_id = cfp["DEFAULT"]["databricks_workspace_host_id"]
    db_pat = cfp["DEFAULT"]["databricks_personal_access_token"]

    JOB_URL = f"https://{db_host_id}.azuredatabricks.net/api/2.1/jobs/create"
    for job_def in os.listdir(args.folder):
        if not job_def.endswith("-def.json"):
            continue
        
        print(job_def)
        with open(os.path.join(args.folder, job_def), 'r') as fp:
            job_json = json.load(fp)
        
        job_str = json.dumps(job_json)
        if job_def.startswith("spark2"):
            job_str = job_str.replace("<CLUSTER_ID>", cfp["DEFAULT"]["databricks_spark2_cluster"])
        else:
            job_str = job_str.replace("<CLUSTER_ID>", cfp["DEFAULT"]["databricks_spark3_cluster"])
        
        job_json_to_submit = json.loads(job_str)

        resp = requests.post(
            url=JOB_URL,
            json=job_json_to_submit,
            headers={
                "Authorization": f"Bearer {db_pat}"
            }
        )
        print(resp.content)


