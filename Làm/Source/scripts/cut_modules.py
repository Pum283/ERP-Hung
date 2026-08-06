#!/usr/bin/env python3
"""
Cắt module khỏi source khi clone bán cho khách.

Nguồn sự thật: ../MODULES.json
Docs: ../MODULES.md

Ví dụ:
  python cut_modules.py --keep SYS,HRM,WF --dry-run
  python cut_modules.py --keep SYS,HRM,WF --apply
"""
from __future__ import annotations

import argparse
import json
import shutil
import sys
from pathlib import Path

HERE = Path(__file__).resolve().parent
SOURCE = HERE.parent
MANIFEST = SOURCE / "MODULES.json"
BE_SRC = SOURCE / "backend" / "src"
API = BE_SRC / "Erp.Api"
DOMAIN = BE_SRC / "Erp.Domain"
APP = BE_SRC / "Erp.Application"
INFRA = BE_SRC / "Erp.Infrastructure"
FE = SOURCE / "frontend" / "src"

FOLDER = {
    "SYS": "Sys",
    "HRM": "Hrm",
    "WF": "Wf",
    "MOD": "Mod",
    "LMS": "Lms",
    "CRM": "Crm",
    "POS": "Pos",
    "PUR": "Pur",
    "INV": "Inv",
    "LOG": "Log",
    "MFG": "Mfg",
    "FSM": "Fsm",
    "PJM": "Pjm",
    "FIN": "Fin",
    "AST": "Ast",
    "BI": "Bi",
    "PRT": "Prt",
}


def load_manifest() -> dict:
    return json.loads(MANIFEST.read_text(encoding="utf-8"))


def resolve_keep(raw: str, mods: list[dict]) -> set[str]:
    keep = {c.strip().upper() for c in raw.split(",") if c.strip()}
    by_code = {m["code"].upper(): m for m in mods}
    keep.add("SYS")
    keep.add("MOD")

    changed = True
    while changed:
        changed = False
        for code in list(keep):
            m = by_code.get(code)
            if not m:
                continue
            for dep in m.get("depends_on") or []:
                d = dep.upper()
                if d not in keep:
                    keep.add(d)
                    changed = True

    bad = [c for c in keep if c not in by_code]
    if bad:
        print(f"WARN: mã không có trong MODULES.json: {bad}", file=sys.stderr)
    return keep


def candidate_paths(code: str, m: dict) -> list[Path]:
    folder = FOLDER.get(code, code.title())
    paths: list[Path] = [
        API / "Controllers" / folder,
        DOMAIN / "Entities" / folder,
        DOMAIN / "Enums" / folder,
        APP / "DTOs" / folder,
        APP / "Interfaces" / "Services" / folder,
        INFRA / "Implementations" / "Services" / folder,
        INFRA / "Persistence" / "Configurations" / folder,
    ]
    fe = m.get("fe") or {}
    for rel in fe.get("routes") or []:
        paths.append(SOURCE / "frontend" / rel)
    # Dedup
    out: list[Path] = []
    seen: set[str] = set()
    for p in paths:
        key = str(p).lower()
        if key in seen:
            continue
        seen.add(key)
        out.append(p)
    return out


def scrub_fe_day1(code: str, apply: bool) -> None:
    page = FE / "app" / "app" / "[module]" / "page.tsx"
    if not page.exists():
        return
    key = code.lower()
    lines = page.read_text(encoding="utf-8").splitlines(keepends=True)
    new_lines = [ln for ln in lines if not ln.lstrip().startswith(f"{key}:")]
    if new_lines == lines:
        print(f"  FE Day-1: không thấy key `{key}` trong DEFAULT_TYPES")
        return
    print(f"  FE Day-1: gỡ DEFAULT_TYPES.{key}")
    if apply:
        page.write_text("".join(new_lines), encoding="utf-8")


def scrub_module_meta(code: str, apply: bool) -> None:
    meta = FE / "shared" / "modules" / "module-meta.ts"
    if not meta.exists():
        return
    text = meta.read_text(encoding="utf-8")
    marker = f"  {code}: {{"
    if marker not in text:
        print(f"  FE meta: không thấy MODULE_META.{code}")
        return
    start = text.index(marker)
    i = start + len(marker)
    depth = 1
    while i < len(text) and depth:
        if text[i] == "{":
            depth += 1
        elif text[i] == "}":
            depth -= 1
        i += 1
    end = i
    if end < len(text) and text[end] == ",":
        end += 1
    if end < len(text) and text[end] == "\n":
        end += 1
    print(f"  FE meta: gỡ MODULE_META.{code}")
    if apply:
        meta.write_text(text[:start] + text[end:], encoding="utf-8")


def print_manual_checklist(m: dict) -> None:
    cut = m.get("cut") or {}
    for s in cut.get("manual_steps") or []:
        print(f"  [ ] {s}")
    di = (m.get("be") or {}).get("di")
    if di:
        print(f"  [ ] DependencyInjection: bỏ services.{di}()")


def main() -> int:
    ap = argparse.ArgumentParser(description="Cắt module source theo MODULES.json")
    ap.add_argument("--keep", required=True, help="CSV mã giữ lại, vd SYS,HRM,WF")
    ap.add_argument("--dry-run", action="store_true", help="Chỉ in, không xóa")
    ap.add_argument("--apply", action="store_true", help="Thực sự xóa / scrub")
    args = ap.parse_args()

    if args.apply and args.dry_run:
        print("Không dùng đồng thời --apply và --dry-run", file=sys.stderr)
        return 2
    apply = bool(args.apply)
    if not apply:
        print("Chế độ dry-run (thêm --apply để cắt thật).\n")

    man = load_manifest()
    mods = man["modules"]
    keep = resolve_keep(args.keep, mods)
    print(f"KEEP (kèm depends_on): {', '.join(sorted(keep))}\n")

    to_cut = [m for m in mods if m["code"].upper() not in keep]
    for m in to_cut:
        code = m["code"].upper()
        cut = m.get("cut") or {}
        if cut.get("allowed") is False:
            print(f"## {code} — BỎ QUA: {cut.get('reason')}")
            continue

        print(f"## CUT {code} ({m.get('maturity')})")
        paths = [p for p in candidate_paths(code, m) if p.exists()]
        if paths:
            for p in paths:
                try:
                    rel = p.relative_to(SOURCE)
                except ValueError:
                    rel = p
                print(f"  DEL {rel}")
                if apply:
                    if p.is_dir():
                        shutil.rmtree(p)
                    else:
                        p.unlink()
        else:
            print("  (không có folder trên disk)")

        if m.get("fe", {}).get("day1_catch_all"):
            scrub_fe_day1(code, apply)
        if code not in ("SYS", "MOD"):
            scrub_module_meta(code, apply)

        if m.get("maturity") == "full" or not cut.get("auto"):
            print("  Checklist thủ công:")
            print_manual_checklist(m)
        print()

    print("Xong." + (" (dry-run)" if not apply else " (applied)"))
    print("Tiếp: dotnet build · pnpm build · xem MODULES.md")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
