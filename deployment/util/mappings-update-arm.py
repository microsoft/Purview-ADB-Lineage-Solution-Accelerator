import argparse
import json

if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("mappings_json", help="File path of the mappings json")
    parser.add_argument("template_file", help="File path to the ARM template to be updated")
    parser.add_argument("output_path", help="File path to the output")
    args, unknown_args = parser.parse_known_args()

    with open(args.mappings_json, 'r') as fp:
        mappings = json.load(fp)

    with open(args.template_file, 'r') as arm_input:
        arm = json.load(arm_input)
    
    for resource in arm["resources"]:
        if resource["type"] != "Microsoft.Web/sites":
            continue

        child_resources = resource["resources"]
        for child_resource in child_resources:
            if child_resource["type"] != "config":
                continue

            for setting in child_resource["properties"]["appSettings"]:
                if setting["name"] != "OlToPurviewMappings":
                    continue
                setting["value"] = json.dumps(mappings).replace(" ", "")
                print("Successfully updated mappings setting")


    with open(args.output_path, 'w') as output:
        json.dump(arm, output, indent="\t")
