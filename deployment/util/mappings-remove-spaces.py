import argparse
import os


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("mappings_json", help="File path of the mappings json")
    args, unknown_args = parser.parse_known_args()

    with open(args.mappings_json, 'r') as fp:
        mappings = fp.read()

    oneliner = mappings.replace("\n", "").replace(" ", "")
    print(oneliner)
