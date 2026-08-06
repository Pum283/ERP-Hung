#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Sinh SRS v1.1 chỉnh chu (chuẩn SYS) cho các module nghiệp vụ từ TREE + META + UC author.
Mặc định bỏ qua SYS (đã có bản tay riêng).
"""
from __future__ import annotations

import argparse
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
DATA = Path(__file__).resolve().parents[2] / "00. Tổng quan"
TOOLS = Path(__file__).resolve().parent
sys.path.insert(0, str(DATA))
sys.path.insert(0, str(TOOLS))

from cay_chuc_nang_data import TREE  # noqa: E402
from srs_module_meta import FOLDERS, META  # noqa: E402
from srs_module_meta_rest import REST  # noqa: E402
from srs_v11_core import build_srs_markdown, write_srs_file  # noqa: E402
from uc_author_v11 import author_uc, group_description  # noqa: E402

META.update(REST)

DEFAULT_MODULES = [
    "HRM", "LMS", "CRM", "POS", "PUR", "INV", "LOG",
    "MFG", "FSM", "PJM", "FIN", "AST", "WF", "BI", "PRT",
]


def infer_actor(meta: dict, code: str, group_code: str) -> str | None:
    m = meta.get("default_actors_by_group") or {}
    if group_code in m:
        # may be "A / B" — take first
        return str(m[group_code]).split("/")[0].strip()
    actors = meta.get("actors") or []
    if actors:
        return actors[0][0]
    return None


def build_module_groups(code: str) -> list:
    meta = META[code]
    tree = TREE[code]
    groups = []
    uc_i = 1
    for group_code, group_name, funcs in tree:
        ucs = []
        actor = infer_actor(meta, code, group_code)
        for ten, mota, uu in funcs:
            ucs.append(
                author_uc(
                    code=code,
                    group_code=group_code,
                    group_name=group_name,
                    uc_index=uc_i,
                    ten=ten,
                    mota=mota or ten,
                    uu_tien=uu,
                    default_actor=actor,
                )
            )
            uc_i += 1
        desc = group_description(code, group_name, len(ucs))
        groups.append((group_code, group_name, desc, ucs))
    return groups


def generate_module(code: str, also_docx: bool = True) -> Path:
    if code not in TREE:
        raise KeyError(f"Module {code} not in TREE")
    if code not in META:
        raise KeyError(f"Module {code} not in META")
    meta = dict(META[code])
    meta["ma_tai_lieu"] = f"SRS-{code}-v1.1"
    meta["phien_ban"] = "1.1"
    folder = ROOT / FOLDERS[code]
    groups = build_module_groups(code)
    md = build_srs_markdown(code=code, meta=meta, groups=groups)
    out_md = folder / f"SRS_{code}_v1.1.md"
    write_srs_file(out_md, md)

    if also_docx:
        from build_srs_docx_pro import build, extract_meta_from_sys_md

        out_docx = folder / f"SRS_{code}_v1.1.docx"
        build(out_md, out_docx, meta=extract_meta_from_sys_md(out_md))
    return out_md


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument(
        "modules",
        nargs="*",
        default=DEFAULT_MODULES,
        help="Mã module (mặc định 15 module, không gồm SYS)",
    )
    ap.add_argument("--no-docx", action="store_true")
    ap.add_argument("--skip", nargs="*", default=[], help="Bỏ qua module")
    args = ap.parse_args()
    skip = set(args.skip or [])
    skip.add("SYS")  # always skip SYS
    mods = [m.upper() for m in args.modules if m.upper() not in skip]
    print(f"Generating v1.1 for: {', '.join(mods)}")
    for code in mods:
        print(f"\n=== {code} ===")
        generate_module(code, also_docx=not args.no_docx)
    print("\nDone.")


if __name__ == "__main__":
    main()
