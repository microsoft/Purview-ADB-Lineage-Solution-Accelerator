# Integration Tests for Purview Spark Connector

The examples are intended for the internal team to verify the solution works correctly across our [limited set of scenarios](../LIMITATIONS.md).

However, they also offer a wide set of samples that you can use to test this solution with some modification.

## Cluster Environment Variables

Set the following environment variables on your cluster(s) to run the tests:
* STORAGE_SERVICE_NAME
* SYNAPSE_STORAGE_SERVICE_NAME
* SYNAPSE_SERVICE_NAME

## Notebook Jobs

Each notebook job tends to point to an Azure Blob Storage Account which should be created before running the sample.

In addition some mount points and spark sql tables should be instantiated in advance

```
/mnt/rawdata
/mnt/outputdata
```

```sql
CREATE TABLE testcasesixteen (
    id int,
    postalcode string,
    streetaddress string,
    city string,
    stateAbbreviation string
)
USING DELTA
LOCATION "abfss://CONTAINER@ACCOUNT.dfs.core.windows.net/PATH/"
```

## Wheel Jobs

The wheel job samples can be built and uploaded by running:

```bash 
# Run this command one time to create a wheels directory
# dbfs mkdirs dbfs:/wheels
cd wheeljobs/abfssInAbfssOut
python -m setup bdist_wheel
dbfs cp ./dist/abfssintest-0.0.3-py3-none-any.whl  dbfs:/wheels --overwrite
```

You will also need to create a databricks job definition for each wheel job. A sample is at `spark-tests\wheeljobs\abfssInAbfssOut\db-job-def.json`.

## Jar Jobs

The jar job can be built by 
```bash
cd jarjobs/abfssInAbfssOut/
./gradlew clean build
dbfs cp ./app/build/libs/app.jar  dbfs:/jars/abfssInAbfssOut.jar --overwrite
```

# Data Factory Jobs

Two notebooks are meant to be ran from Data Factory `call-via-adf-spark2` and `call-via-adf-spark3`.

You should create a pipeline that runs these two notebooks on separate Spark2 and Spark3 clusters.
