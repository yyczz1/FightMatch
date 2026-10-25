#!/usr/bin/env python3
"""Add Luban's row-marker column to frozen chapter CSVs; preserve every data cell."""
import argparse
import csv
from pathlib import Path
import xml.etree.ElementTree as ET


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--source", required=True, type=Path)
    parser.add_argument("--output", required=True, type=Path)
    parser.add_argument("--schema", default="Config/FightMatch/Luban/ChapterOneTrial/schema.xml", type=Path)
    args = parser.parse_args()
    schema = ET.parse(args.schema).getroot()
    beans = {bean.attrib["name"]: [field.attrib["name"] for field in bean.findall("var")]
             for bean in schema.findall("bean")}
    staged = []
    for table in schema.findall("table"):
        name = table.attrib["input"]
        with (args.source / name).open(encoding="utf-8-sig", newline="") as stream:
            rows = list(csv.reader(stream))
        columns = beans[table.attrib["value"]]
        if not rows or rows[0] != columns or any(len(row) != len(columns) for row in rows):
            raise ValueError(f"{name}: source columns do not match the fixed Luban schema")
        staged.append((name, [["##var"] + rows[0]] + [[""] + row for row in rows[1:]]))
    args.output.mkdir(parents=True, exist_ok=False)
    for name, rows in staged:
        with (args.output / name).open("x", encoding="utf-8", newline="") as stream:
            writer = csv.writer(stream, lineterminator="\n")
            writer.writerows(rows)
    print(f"Prepared {len(staged)} marker-only CSV inputs for standard Luban generation.")


if __name__ == "__main__":
    main()
