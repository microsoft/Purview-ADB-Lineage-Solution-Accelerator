# Running the Integration Tests

The `run-test.sh` script will run each spark job as defined in the jobdefs folder.

A related `*-expectations.json` file should exist for every test definition. It provides the expected qualified names to search for.


## Notes

* Synapse Output does not test the Process currently as the output asset is always different due to the TempFolder naming convention
* Azure Data Factory testing is not yet automated

## Coverage

|Name|Input|Output|Spark 2|Spark 3|Note|
|----|----|----|----|----|----|
|abfss-in-abfss-out-ouath.scala|ABFS|ABFS|✅|✅|Uses Oauth|
|abfss-in-abfss-out-root.scala|ABFS|ABFS||✅|Writes to root of container|
|abfss-in-abfss-out.scala|ABFS|ABFS|✅|✅||
|azuresql-in-azuresql-out.scala|AzSQL|AzSQL||✅||
|call-via-adf-spark2.scala|ABFS|ABFS|✅||Called via Azure Data Factory|
|call-via-adf-spark3.scala|ABFS|ABFS||✅|Called via Azure Data Factory|
|delta-in-delta-merge.scala|DELTA|DELTA|❌|❌|Uses a Merge Statement|
|delta-in-delta-out-abfss.scala|DELTA|DELTA||✅||
|delta-in-delta-out-fs.scala|DELTA|DELTA||✅||
|delta-in-delta-out-mnt.scala|DELTA|DELTA||✅|Uses a Mount Point|
|intermix-languages.scala|ABFS|ABFS||✅|Intermixes scala and Python|
|mnt-in-mnt-out.scala|ABFS|ABFS|✅|✅|Uses a Mount Point as Output|
|nested-child.scala|N/A|N/A|N/A|N/A|Called by nested-parent.scala|
|nested-parent.scala|ABFS|ABFS||✅|Calls nested-child.scala|
|spark-sql-table-in-abfss-out.scala|SparkSQL|ABFS|❌|❌|Queries Spark SQL table|
|synapse-in-synapse-out.scala|Synapse|Synapse|❌|❌||
|synapse-in-wasbs-out.scala|Synapse|WASB||✅||
|synapse-wasbs-in-synapse-out.scala|Synapse, WASB|Synapse|✅|✅|Joins Synapse and WASB data|
|wasbs-in-wasbs-out-with-param.py|WASB|WASB||✅|Passes in a parameter to job|
|wasbs-in-wasbs-out.scala|WASB|WASB||✅||
|JarJob - spark_jar_task|ABFS|ABFS||✅|spark_jar_task|
|pythonscript|ABFS|ABFS||✅|spark_python_task|
|pythonwheel|ABFS|ABFS||✅|python_wheel_task|
|spark submit task|ABFS|ABFS|❌|❌|spark_submit_task|
