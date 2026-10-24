#!/opt/homebrew/bin/python3
"""Read-only, table-driven LOC-LIC-ALT-M-09 preflight checker."""

from __future__ import annotations

import csv
import hashlib
import os
import platform
import stat
import sys
from collections import OrderedDict
from pathlib import Path, PurePosixPath


HEADER = [
    "setId",
    "mode",
    "root",
    "relativePath",
    "authorityPath",
    "expectedBytes",
    "expectedSha256",
    "comparison",
]


class CheckFailure(Exception):
    pass


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        while chunk := stream.read(1024 * 1024):
            digest.update(chunk)
    return digest.hexdigest()


def require_regular(path: Path) -> None:
    try:
        mode = os.lstat(path).st_mode
    except FileNotFoundError as exc:
        raise CheckFailure(f"missing:{path}") from exc
    if not stat.S_ISREG(mode):
        raise CheckFailure(f"not-regular:{path}")


def require_followed_regular(path: Path) -> None:
    try:
        mode = os.stat(path, follow_symlinks=True).st_mode
    except FileNotFoundError as exc:
        raise CheckFailure(f"missing-or-broken-link:{path}") from exc
    if not stat.S_ISREG(mode):
        raise CheckFailure(f"target-not-regular:{path}")


def strict_lines(path: Path) -> list[str]:
    raw = path.read_bytes()
    if raw.startswith(b"\xef\xbb\xbf"):
        raise CheckFailure(f"bom:{path}")
    if b"\r" in raw:
        raise CheckFailure(f"non-lf-line-ending:{path}")
    if not raw.endswith(b"\n"):
        raise CheckFailure(f"missing-final-lf:{path}")
    try:
        text = raw.decode("utf-8", errors="strict")
    except UnicodeDecodeError as exc:
        raise CheckFailure(f"invalid-utf8:{path}") from exc
    return text[:-1].split("\n")


def read_table(path: Path) -> list[dict[str, str]]:
    lines = strict_lines(path)
    rows = list(csv.reader(lines, delimiter="\t", strict=True))
    if not rows or rows[0] != HEADER:
        raise CheckFailure("table-header")
    if len(rows) < 2:
        raise CheckFailure("table-empty")
    result: list[dict[str, str]] = []
    for line_number, row in enumerate(rows[1:], 2):
        if len(row) != 8:
            raise CheckFailure(f"table-field-count:{line_number}:{len(row)}")
        result.append(dict(zip(HEADER, row)))
    return result


def target_for(row: dict[str, str]) -> Path:
    root = Path(row["root"])
    relative = row["relativePath"]
    if relative in ("", "."):
        return root
    pure = PurePosixPath(relative)
    if pure.is_absolute() or ".." in pure.parts:
        raise CheckFailure(f"unsafe-relative-path:{relative}")
    return root.joinpath(*pure.parts)


def check_version(path: Path, expected: str) -> None:
    if path.name == "python3":
        if Path(sys.executable).resolve() != path.resolve():
            raise CheckFailure(f"python-executable:{sys.executable}")
        if not expected.startswith("Python-"):
            raise CheckFailure(f"python-version-token:{expected}")
        expected = expected.removeprefix("Python-")
        actual = platform.python_version()
        if actual != expected:
            raise CheckFailure(f"python-version:{actual}")
        return
    if path.name == "dotnet":
        sdk_root = path.parent / "sdk" / expected
        if not sdk_root.is_dir():
            raise CheckFailure(f"dotnet-sdk-version-missing:{expected}")
        version_file = sdk_root / ".version"
        require_regular(version_file)
        if expected not in version_file.read_text(encoding="utf-8", errors="strict"):
            raise CheckFailure(f"dotnet-sdk-version-file:{expected}")
        return
    raise CheckFailure(f"unsupported-version-target:{path}")


def compare_file(row: dict[str, str], allow_symlink: bool) -> str:
    path = target_for(row)
    if allow_symlink:
        require_followed_regular(path)
    else:
        require_regular(path)
    tokens = row["comparison"].split(";")
    for token in tokens:
        if token == "bytes+sha256":
            if row["expectedBytes"] == "" or row["expectedSha256"] == "":
                raise CheckFailure(f"missing-authority-field:{path}")
            actual_bytes = path.stat().st_size
            if actual_bytes != int(row["expectedBytes"]):
                raise CheckFailure(f"bytes:{path}:{actual_bytes}")
            actual_sha = sha256(path)
            if actual_sha != row["expectedSha256"]:
                raise CheckFailure(f"sha256:{path}:{actual_sha}")
        elif token == "sha256-only" or token == "follow-target-sha256-only":
            if row["expectedSha256"] == "":
                raise CheckFailure(f"missing-sha-authority:{path}")
            actual_sha = sha256(path)
            if actual_sha != row["expectedSha256"]:
                raise CheckFailure(f"sha256:{path}:{actual_sha}")
        elif token.startswith("version-"):
            check_version(path, token.removeprefix("version-"))
        else:
            raise CheckFailure(f"unsupported-file-comparison:{token}")
    link = f":link={os.readlink(path)}" if path.is_symlink() else ""
    return f"{path.stat().st_size}:{sha256(path)}{link}"


def enumerate_regular(root: Path) -> dict[str, Path]:
    if not root.is_dir():
        raise CheckFailure(f"not-directory:{root}")
    result: dict[str, Path] = {}
    for directory, dirnames, filenames in os.walk(root, followlinks=False):
        directory_path = Path(directory)
        for name in list(dirnames):
            path = directory_path / name
            if path.is_symlink():
                raise CheckFailure(f"symlink-directory:{path}")
        for name in filenames:
            path = directory_path / name
            require_regular(path)
            relative = path.relative_to(root).as_posix()
            if relative in result:
                raise CheckFailure(f"duplicate-relative-path:{relative}")
            result[relative] = path
    return result


def check_exact_file_set(rows: list[dict[str, str]]) -> tuple[list[str], str]:
    root = Path(rows[0]["root"])
    actual = enumerate_regular(root)
    expected = {row["relativePath"] for row in rows}
    if len(expected) != len(rows):
        raise CheckFailure("duplicate-authority-relative-path")
    actual_set = set(actual)
    if actual_set != expected:
        missing = sorted(expected - actual_set)
        unexpected = sorted(actual_set - expected)
        raise CheckFailure(f"set-drift:missing={missing}:unexpected={unexpected}")
    details = [compare_file(row, allow_symlink=False) for row in rows]
    return details, f"files={len(actual)}"


def check_manifest7(row: dict[str, str]) -> tuple[str, str]:
    authority = Path(row["authorityPath"])
    require_regular(authority)
    tokens = row["comparison"].split(";")
    if tokens != ["authority-bytes+sha256", "target-exact-2021-first-three-columns"]:
        raise CheckFailure(f"manifest7-comparison:{tokens}")
    if authority.stat().st_size != int(row["expectedBytes"]):
        raise CheckFailure(f"manifest7-authority-bytes:{authority.stat().st_size}")
    authority_sha = sha256(authority)
    if authority_sha != row["expectedSha256"]:
        raise CheckFailure(f"manifest7-authority-sha:{authority_sha}")
    parsed = list(csv.reader(strict_lines(authority), delimiter="\t", strict=True))
    if not parsed or parsed[0][:3] != ["m01RelativePath", "m01Bytes", "m01Sha256"]:
        raise CheckFailure("manifest7-header")
    data = parsed[1:]
    if len(data) != 2021 or any(len(item) != 7 for item in data):
        raise CheckFailure(f"manifest7-shape:{len(data)}")
    expected: dict[str, tuple[int, str]] = {}
    for item in data:
        relative, byte_text, digest = item[:3]
        if relative in expected:
            raise CheckFailure(f"manifest7-duplicate:{relative}")
        expected[relative] = (int(byte_text), digest)
    root = target_for(row)
    actual = enumerate_regular(root)
    if set(actual) != set(expected):
        missing = sorted(set(expected) - set(actual))
        unexpected = sorted(set(actual) - set(expected))
        raise CheckFailure(f"manifest7-set-drift:missing={missing}:unexpected={unexpected}")
    for relative in sorted(expected, key=lambda value: value.encode("utf-8")):
        expected_bytes, expected_sha = expected[relative]
        path = actual[relative]
        if path.stat().st_size != expected_bytes:
            raise CheckFailure(f"manifest7-bytes:{relative}:{path.stat().st_size}")
        actual_sha = sha256(path)
        if actual_sha != expected_sha:
            raise CheckFailure(f"manifest7-sha:{relative}:{actual_sha}")
    return f"{authority.stat().st_size}:{authority_sha}", f"files={len(actual)}"


def main() -> int:
    if len(sys.argv) != 2:
        print("usage: preflight.py TABLE", file=sys.stderr)
        return 64
    table_path = Path(sys.argv[1])
    try:
        rows = read_table(table_path)
    except Exception as exc:
        print(f"TOTAL\tFAIL\t{type(exc).__name__}:{exc}")
        return 1

    groups: OrderedDict[tuple[str, str, str], list[tuple[int, dict[str, str]]]] = OrderedDict()
    for index, row in enumerate(rows, 1):
        groups.setdefault((row["setId"], row["mode"], row["root"]), []).append((index, row))

    output: list[str] = []
    failed = False
    m01_files = 0
    for key, indexed_rows in groups.items():
        set_id, mode, root = key
        group_errors: list[str] = []
        row_details: dict[int, tuple[str, str]] = {}
        try:
            if mode == "file" or mode == "runtime-file":
                for index, row in indexed_rows:
                    try:
                        row_details[index] = ("PASS", compare_file(row, allow_symlink=mode == "runtime-file"))
                    except Exception as exc:
                        row_details[index] = ("FAIL", f"{type(exc).__name__}:{exc}")
                        group_errors.append(row_details[index][1])
            elif mode == "exact-file-set":
                details, group_detail = check_exact_file_set([row for _, row in indexed_rows])
                for (index, _), detail in zip(indexed_rows, details):
                    row_details[index] = ("PASS", detail)
                output.append(f"GROUP\t{set_id}\t{mode}\t{root}\tPASS\t{group_detail}")
            elif mode == "manifest7":
                if len(indexed_rows) != 1:
                    raise CheckFailure(f"manifest7-group-size:{len(indexed_rows)}")
                index, row = indexed_rows[0]
                row_detail, group_detail = check_manifest7(row)
                row_details[index] = ("PASS", row_detail)
                m01_files = int(group_detail.split("=", 1)[1])
                output.append(f"GROUP\t{set_id}\t{mode}\t{root}\tPASS\t{group_detail}")
            elif mode == "absent":
                for index, row in indexed_rows:
                    target = target_for(row)
                    if target.exists() or target.is_symlink():
                        row_details[index] = ("FAIL", f"present:{target}")
                        group_errors.append(row_details[index][1])
                    else:
                        row_details[index] = ("PASS", f"absent:{target}")
            else:
                raise CheckFailure(f"unsupported-mode:{mode}")
        except Exception as exc:
            group_errors.append(f"{type(exc).__name__}:{exc}")
            for index, _ in indexed_rows:
                row_details.setdefault(index, ("FAIL", group_errors[-1]))

        for index, row in indexed_rows:
            verdict, detail = row_details[index]
            output.append(
                f"ROW\t{index}\t{row['setId']}\t{row['mode']}\t{row['relativePath']}\t{verdict}\t{detail}"
            )
            if verdict != "PASS":
                failed = True
        if mode not in ("exact-file-set", "manifest7"):
            verdict = "FAIL" if group_errors else "PASS"
            detail = "|".join(group_errors) if group_errors else f"rows={len(indexed_rows)}"
            output.append(f"GROUP\t{set_id}\t{mode}\t{root}\t{verdict}\t{detail}")
            if group_errors:
                failed = True

    for line in output:
        print(line)
    verdict = "FAIL" if failed else "PASS"
    print(f"TOTAL\t{verdict}\trows={len(rows)}\tgroups={len(groups)}\tm01Files={m01_files}")
    return 1 if failed else 0


if __name__ == "__main__":
    raise SystemExit(main())
