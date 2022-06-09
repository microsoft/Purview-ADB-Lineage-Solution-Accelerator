#!/bin/bash
# Run each test and wait for completion
# run-test.sh [ALL|regex] workspaceId
# Use ALL to run all tests or provide a regex to match the test definitions you want processed
# # default is def.json$

TEST_DEF_PATTERN="def.json$"
if [[ "$1" != "ALL" ]]
then
    echo "Recognized test defintion pattern being set"
    TEST_DEF_PATTERN=$1
fi
# Set the workspace id
WORKSPACE_ID=$2

# Get list of jobs on the cluster
JOB_ID_PATTERN="^[[:digit:]][[:digit:]]*$"
TESTS_DIRECTORY=./tests/integration/jobdefs
KNOWN_JOBS=$(databricks jobs list --output JSON)
JOB_NAME_ID_PAIRS=$(echo $KNOWN_JOBS | jq -r '.jobs[] | [.settings.name, .job_id] | @tsv')
declare -A jobnametoid
temp_job_name=""
for job in $JOB_NAME_ID_PAIRS; do
    clean_job_row=$(echo "$job" | xargs)
    if [[ $clean_job_row =~  $JOB_ID_PATTERN ]]
    then
        # echo "Adding $clean_job_row as $temp_job_name"
        jobnametoid[$temp_job_name]=$clean_job_row
    else
        # echo "Assigning $clean_job_row to temp_job_name"
        temp_job_name=$clean_job_row
    fi
done

echo "Iterating over saved job definitions in integration tests"
# List each file in tests/integration/jobdefs
for fn in `ls ./tests/integration/jobdefs`; do
    if [[ ! $fn =~ $TEST_DEF_PATTERN ]]
    then
        # echo "Skipping processing of $fn"
        continue
    fi

    # For each file, get the settings.name
    job_name=$(cat "$TESTS_DIRECTORY/$fn" | jq -r '.settings.name')
    echo "Preparing to run JobDef:$fn JobName:$job_name JobId:${jobnametoid[$job_name]}"
    temp_job_id=${jobnametoid[$job_name]}
    # Get the expectation file
    len_of_jobdef_file_name=${#fn}
    jobdef_file_name_root=$(echo $fn | head -c $(($len_of_jobdef_file_name-9)))
    expectation_file_name=$(echo $jobdef_file_name_root-expectations.json)

    # Call databricks API to run
    temp_run_now=$(databricks jobs run-now --job-id $temp_job_id)
    temp_run_id=$(echo $temp_run_now | jq '.run_id')
    echo "Run ID $temp_run_id is started. Checking status every one minute."
    # Sleep for a few minutes
    is_running="true"
    while [[ $is_running == "true" ]]
    do
        sleep 1m
        run_get_response=$(databricks runs get --run-id $temp_run_id)
        run_state=$(echo $run_get_response | jq -r '.state.life_cycle_state')
        echo $run_state
        if [[ $run_state == "TERMINATED" ]] || [[ $run_state == "SKIPPED" ]] || [[ $run_state == "INTERNAL_ERROR" ]]
        then
            is_running="false"
            if [[ $run_state == "SKIPPED" ]] || [[ $run_state == "INTERNAL_ERROR" ]]
            then
                echo $run_get_response
            fi
            
        fi
    done
    # Check to see if the Purview instance has our expectations
    test_checker=$(python ./tests/integration/runner.py $TESTS_DIRECTORY/$expectation_file_name $WORKSPACE_ID $temp_job_id)
    echo $test_checker
    is_sucessful=""
    for elem in $test_checker; do
        is_sucessful=$elem
    done
    
    if [[ $is_successful == "True" ]]
    then
        echo "$fn was successful!"
    else
        echo "$fn was NOT successful"
        # exit 125
    fi
    echo "Completed Analysis"
done

