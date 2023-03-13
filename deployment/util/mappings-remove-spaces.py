import argparse


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("mappings_json", help="File path of the mappings json")
    parser.add_argument("output_path", help="File path where the oneline json file should land")
    args, unknown_args = parser.parse_known_args()

    with open(args.mappings_json, 'r') as fp:
        mappings = fp.read()

    oneliner = mappings.replace("\n", "").replace(" ", "")
    with open(args.output_path, 'w') as output:
        output.write(oneliner)
