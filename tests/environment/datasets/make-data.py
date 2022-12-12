import argparse
import configparser
from io import BytesIO
import json
import pathlib
import re


from azure.identity import DefaultAzureCredential
from azure.storage.blob import BlobServiceClient, BlobClient

def make_or_get_connection_client(connection_string, cached_connections, **kwargs):
    if connection_string in cached_connections:
        return cached_connections[connection_string]

    elif re.search(r'EndpointSuffix=', connection_string): # Is Blob
        _client = BlobServiceClient.from_connection_string(connection_string)
        cached_connections[connection_string] = _client
        return _client
    else:
        raise NotImplementedError("Connection String not supported")


def make_and_upload_data(client, storage_path, dataset_name, storage_format, data):
    if isinstance(client, BlobServiceClient):
        blob_full_path = pathlib.Path(storage_path)
        container = blob_full_path.parts[0]
        blob_relative_path = '/'.join(list(blob_full_path.parts[1:])+[dataset_name, dataset_name+"."+storage_format])

        _blob_client = client.get_blob_client(container, blob_relative_path)
        blob_stream = BytesIO()
        with blob_stream as fp:
            for row in data["data"]:
                fp.write(bytes(','.join(str(r) for r in row), encoding="utf-8"))
            fp.seek(0)
            _blob_client.upload_blob(blob_stream.read(), blob_type="BlockBlob", overwrite=True)
    else:
        raise NotImplementedError(f"{type(client)} not supported")


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--test_case", "-t", type=str, action="append", help="Name of the test case(s) to be deployed. If not specified, upload all datasets")
    parser.add_argument("--config", type=str, help="Path to the json config file", default="./tests/environment/config.json")
    parser.add_argument("--ini", type=str, help="Path to the ini config file", default="./tests/environment/config.ini")
    args = parser.parse_args()

    # Datasets
    ## CSV
    ## Parquet
    ## Delta
    ## SQL
    ## COSMOS
    ## Kusto

    # Load Test Cases
    ## jobs and dataset
    _connections = configparser.ConfigParser()
    _connections.read(args.ini)

    with open(args.config, 'r') as fp:
        _config = json.load(fp)
        TEST_JOBS = _config["jobs"]
        TEST_DATASET = _config["dataset"]
    
    # Filter based on test cases provided
    if args.test_case:
        print(args.test_case)
        jobs_to_build_data = {k:v for k,v in TEST_JOBS.items() if k in args.test_case}
    else:
        jobs_to_build_data = TEST_JOBS
    
    
    # Make the data only one time
    cached_data = {}
    # Make the connections only one time
    cached_connections = {}
    # Iterate over every job and build the dataset
    for job_name, dataset_def in jobs_to_build_data.items():
        if len(dataset_def) == 0 or dataset_def[0] == ["noop"]:
            print(f"{job_name}: skipped")
            continue
        
        for dataset in dataset_def:
            _connection_name = dataset[0]
            _storage_format = dataset[1]
            _storage_path = dataset[2]
            _dataset_name = dataset[3]

            print(f"{job_name}: {_storage_path}")

            _connection_string = _connections["DEFAULT"][_connection_name+"_connection_string"]

            _client = make_or_get_connection_client(_connection_string, cached_connections)

            _data = TEST_DATASET[_dataset_name]

            make_and_upload_data(
                _client,
                _storage_path,
                _dataset_name,
                _storage_format,
                _data
            )



            # Check which storage engine is necessary
            # Check what format the data will be stored in
            # Check the pat
        
        