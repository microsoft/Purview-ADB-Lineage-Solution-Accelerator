import json

if __name__ == "__main__":
    """
    Confirm that the OlToPurviewMappings.json matches the arm template app settings
    """
    ARM_AND_OlToPurviewMappings_MATCHES = False

    with open('./deployment\infra\OlToPurviewMappings.json', 'r') as mapping:
        mapping_json = json.load(mapping)

    with open('./deployment/infra/newdeploymenttemp.json', 'r') as arm:
        arm_body = json.load(arm)

    for resource in arm_body.get("resources", []):
        if resource["type"] != "Microsoft.Web/sites":
            continue

        if resource["name"] != "[variables('functionAppName')]":
            continue

        web_config={}
        for child_resource in resource.get("resources", []):
            if child_resource.get("type") == "config" and child_resource.get("name") == "web":
                web_config = child_resource
                break

        app_settings = web_config.get("properties", {}).get("appSettings", [])
        for setting in app_settings:
            if setting["name"] != "OlToPurviewMappings":
                continue

            arm_mappings_value = json.loads(setting["value"])

            if arm_mappings_value == mapping_json:
                ARM_AND_OlToPurviewMappings_MATCHES = True
                break

        if ARM_AND_OlToPurviewMappings_MATCHES == True:
            break

    print(ARM_AND_OlToPurviewMappings_MATCHES)
    if ARM_AND_OlToPurviewMappings_MATCHES == False:
        exit(1)
