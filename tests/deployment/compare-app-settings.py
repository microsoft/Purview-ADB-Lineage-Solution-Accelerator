import argparse
import json
import os
import subprocess
import sys

if __name__ == "__main__":
    """
    Execution Azure CLI commands to update and delete app settings on a provided
    Azure Function. Compares against the
    `./deployment/infra/newdeploymenttemp.json` file.
    """
    parser = argparse.ArgumentParser()
    parser.add_argument(
        "func", help="If your function app is https://MyFunction.azurewebsites.net this value should be MyFunction")
    parser.add_argument(
        "rg", help="The name of the resource group that contains the function")
    parser.add_argument("--whatif", help="Compare deployed and template app settings.", action="store_true")
    args, _ = parser.parse_known_args()

    FUNCTION_APP_NAME = args.func
    RESOURCE_GROUP = args.rg

    # Receive the app settings currently deployed on the function
    response = subprocess.run(
        [
            "az", "functionapp", "config",
            "appsettings", "list",
            "--name", FUNCTION_APP_NAME,
            "--resource-group", RESOURCE_GROUP
        ],
        capture_output=True,
        check=True
    )
    if response.returncode != 0:
        print(response.stderr)
        exit(response.returncode)

    deployed_app_settings_input = response.stdout.decode("utf-8")
    deployed_app_settings_array = json.loads(deployed_app_settings_input)
    deployed_app_settings = {s["name"]: s["value"]
                             for s in deployed_app_settings_array}
    # Load the app settings in the template
    _repo_path = "/deployment/infra/newdeploymenttemp.json" 
    if "GITHUB_WORKSPACE" in os.environ:
        _repo_path = os.environ.get("GITHUB_WORKSPACE") + _repo_path
    else:
        _repo_path = "." + _repo_path

    with open(_repo_path, 'r') as fp:
        arm_template = json.load(fp)

    app_settings_in_template = None
    for resource in arm_template["resources"]:
        if resource["type"] == "Microsoft.Web/sites":
            app_settings_in_template = resource["properties"]["siteConfig"]["appSettings"]

    if app_settings_in_template is None:
        raise ValueError("Unable to extract the Microsoft.web/sites resources")

    # Filter out any app settings which are variables
    _app_keys_that_are_variables = []
    for setting in app_settings_in_template:
        value = str(setting["value"])
        key = setting["name"]
        if value.startswith("[") and value.endswith("]"):
            _app_keys_that_are_variables.append(key)

    # Update App Settings on Function
    settings_to_update = {}
    for template_setting in app_settings_in_template:
        _setting_name = template_setting["name"]
        if _setting_name in _app_keys_that_are_variables:
            # This is an app setting that is a variable
            # Can't really compare so assuming it's okay
            continue
        elif _setting_name not in deployed_app_settings:
            # The setting doesn't exist on the deployment
            settings_to_update[_setting_name] = template_setting["value"]

        elif template_setting["value"] != deployed_app_settings[_setting_name]:
            # The deployed app setting doesn't match the template
            # Store the template setting name and value for later update
            settings_to_update[_setting_name] = template_setting["value"]
        else:
            # It's the same value, we're good
            continue

    # Update the settings
    if len(settings_to_update) > 0:
        print(
            f"Updating the following app settings: {list(settings_to_update.keys())}")
        if not args.whatif:
            settings_to_update_strings = [
                f'{k}={v}' for k, v in settings_to_update.items()]
            response = subprocess.run(
                [
                    "az", "functionapp", "config",
                    "appsettings", "set",
                    "--name", FUNCTION_APP_NAME,
                    "--resource-group", RESOURCE_GROUP,
                    "--settings",
                ] + settings_to_update_strings,
                capture_output=True,
                check=True
            )
            if response.returncode != 0:
                print(response.stderr)
                exit(response.returncode)
            else:
                print("Succeeded in updating app settings")
    else:
        print("There were no app settings that needed to be updated")

    # Delete App Settings on Function
    settings_to_delete = []
    _template_names = [s["name"] for s in app_settings_in_template]

    for deployed_setting_name in deployed_app_settings.keys():
        if deployed_setting_name not in _template_names:
            settings_to_delete.append(deployed_setting_name)

    if len(settings_to_delete) > 0:
        print(f"Deleting the following app settings: {settings_to_delete}")
        if not args.whatif:
            settings_to_delete_string = ' '.join(settings_to_delete)
            response = subprocess.run(
                [
                    "az", "functionapp", "config",
                    "appsettings", "delete",
                    "--name", FUNCTION_APP_NAME,
                    "--resource-group", RESOURCE_GROUP,
                    "--setting-names", settings_to_delete_string
                ],
                capture_output=True,
                check=True
            )
            if response.returncode != 0:
                print(response.stderr)
                exit(response.returncode)
            else:
                print("Succeeded in deleting selected app settings")
    else:
        print("There were no app settings that needed to be deleted")
