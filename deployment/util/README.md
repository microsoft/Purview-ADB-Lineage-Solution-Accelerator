# Utilities for Deployment

## mappings-remove-spaces

Used in the Github Action for creating a deployment artifact that is easier to copy / paste or upload into an app setting for Azure Functions.

```
usage: mappings-remove-spaces.py [-h] mappings_json output_path

positional arguments:
  mappings_json  File path of the mappings json
  output_path    File path where the oneline json file should land
```

Sample:
```
python ./deployment/util/mappings-remove-spaces.py ./deployment/infra/OlToPurviewMappings.json ./test.json
```

## mappings-update-arm

Used to update the ARM template in a standardized way

```
usage: mappings-update-arm.py [-h] mappings_json template_file output_path              

positional arguments:
  mappings_json  File path of the mappings json
  template_file  File path to the ARM template to be updated
  output_path    File path to the output
```

Sample:
```
python ./deployment/util/mappings-update-arm.py ./deployment/infra/OlToPurviewMappings.json ./deployment/infra/newdeploymenttemp.json ./deployment/infra/newdeploymenttemp.json
```
