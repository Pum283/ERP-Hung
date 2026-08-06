# -*- coding: utf-8 -*-
"""Đánh done các UC cấp 1 theo PHAN_NHOM_UC_CAC_MODULE.md rồi regenerate checklist."""
from __future__ import annotations

import json
from pathlib import Path

HERE = Path(__file__).resolve().parent
PROGRESS_PATH = HERE / "uc_progress.json"


def expand(spec: str) -> list[int]:
    """'001–004, 006, 012, 026–027' -> [1,2,3,4,6,12,26,27]"""
    out: list[int] = []
    for part in spec.replace("–", "-").replace("—", "-").split(","):
        part = part.strip()
        if not part:
            continue
        if "-" in part:
            a, b = part.split("-", 1)
            out.extend(range(int(a), int(b) + 1))
        else:
            out.append(int(part))
    return out


# Cấp 1 theo PHAN_NHOM_UC_CAC_MODULE.md
LEVEL1: dict[str, str] = {
    "HRM": "001–004, 006, 012, 026–027, 029–030, 032–036, 039–043, 045–046",
    "WF": "001, 004–007, 009–010, 014, 017, 023–024, 033, 038",
    "LMS": "001–006, 009–010, 012, 014",
    "CRM": "001–006, 008–011, 014–015",
    "POS": "001–003, 007, 009–010, 012, 014–016, 019",
    "PUR": "001, 003, 009–010, 014, 017–019, 026–028",
    "INV": "001–005, 007–008, 010–012, 014",
    "LOG": "001, 006, 008–014, 017",
    "MFG": "001–003, 006–008, 013, 017–020, 022",
    "FSM": "001–003, 005, 008–010, 013–015, 017",
    "PJM": "001–002, 004–009, 011–012",
    "FIN": "001–004, 006, 008–010, 012–015",
    "AST": "001–004, 008–012, 014",
    "BI": "001–003, 006–008, 013–014, 016",
    "PRT": "001–003, 007–008, 014–016, 019–020",
}

NOTES = {
    "HRM": "N1 · org/status/code/export/HĐ Day-1",
    "WF": "N1 · work-type/project/item/workload",
    "LMS": "N1 · mod_master/document Day-1",
    "CRM": "N1 · mod_master/document Day-1",
    "POS": "N1 · mod_master/document Day-1",
    "PUR": "N1 · mod_master/document Day-1",
    "INV": "N1 · mod_master/document Day-1",
    "LOG": "N1 · mod_master/document Day-1",
    "MFG": "N1 · mod_master/document Day-1",
    "FSM": "N1 · mod_master/document Day-1",
    "PJM": "N1 · mod_master/document Day-1",
    "FIN": "N1 · mod_master/document Day-1",
    "AST": "N1 · mod_master/document Day-1",
    "BI": "N1 · mod_master/document Day-1",
    "PRT": "N1 · mod_master/document Day-1",
}


def main() -> None:
    progress = json.loads(PROGRESS_PATH.read_text(encoding="utf-8")) if PROGRESS_PATH.exists() else {}
    added = 0
    for mod, spec in LEVEL1.items():
        note = NOTES[mod]
        pct = 80 if mod in ("HRM", "WF") else 70
        for n in expand(spec):
            key = f"UC_{mod}_{n:03d}"
            prev = progress.get(key)
            if prev and prev.get("done") and (prev.get("pct") or 0) >= pct:
                # giữ note cũ nếu đã done cao hơn; chỉ bổ sung tag N1 nếu thiếu
                if "N1" not in (prev.get("note") or ""):
                    prev["note"] = f"{prev.get('note', '')} · N1".strip(" ·")
                continue
            progress[key] = {"done": True, "pct": pct, "note": note}
            added += 1

    PROGRESS_PATH.write_text(json.dumps(progress, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"Updated {added} UC keys · total keys={len(progress)}")


if __name__ == "__main__":
    main()
