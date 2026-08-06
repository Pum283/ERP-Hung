#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Sinh 1 file Thiết kế tổng hợp CSDL: danh mục bảng + chi tiết trường."""
from __future__ import annotations

import sys
from datetime import date
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
MODULES_TOOLS = Path(__file__).resolve().parents[2] / "01. Modules" / "_tools"
sys.path.insert(0, str(Path(__file__).resolve().parent))
sys.path.insert(0, str(MODULES_TOOLS))

from ddd_master_catalog import build_catalog  # noqa: E402
from build_srs_docx_pro import build, extract_meta_from_md  # noqa: E402

TODAY = date.today().strftime("%d/%m/%Y")
OUT_MD = ROOT / "DDD-MASTER_Thiet_ke_tong_hop_CSDL.md"
OUT_DOCX = ROOT / "DDD-MASTER_Thiet_ke_tong_hop_CSDL.docx"


def esc(s: str) -> str:
    return (s or "").replace("|", "\\|")


def build_markdown() -> str:
    catalog = build_catalog()
    lines: list[str] = []
    a = lines.append

    a("# DDD-MASTER-v1.0 — Thiết kế tổng hợp cơ sở dữ liệu")
    a("")
    a("> **Database Design Document — Master Consolidated**")
    a("> Tài liệu tổng hợp danh mục toàn bộ bảng và mô tả chi tiết từng trường dữ liệu.")
    a(f"> Phiên bản **1.0** · Ngày {TODAY} · Trạng thái: **Chờ duyệt Solution / DBA**.")
    a("> Nguồn: DDD-01…06 · SRS module v1.1 · INT v1.0. Generic — không gắn khách/ngành cứng.")
    a("")
    a("---")
    a("")
    a("## 0. Thông tin tài liệu")
    a("")
    a("| Thuộc tính | Giá trị |")
    a("|---|---|")
    a("| Mã tài liệu | `DDD-MASTER-v1.0` |")
    a("| Tên | Thiết kế tổng hợp cơ sở dữ liệu |")
    a("| Phiên bản | 1.0 |")
    a(f"| Ngày | {TODAY} |")
    a("| Số bảng | " + str(len(catalog)) + " |")
    a("| Số trường (ước lượng liệt kê) | " + str(sum(len(t["fields"]) for t in catalog)) + " |")
    a("| Định dạng bàn giao | Microsoft Word (`.docx`) |")
    a("")
    a("| Ver | Ngày | Mô tả | Trạng thái |")
    a("|---|---|---|---|")
    a(f"| 1.0 | {TODAY} | Tổng hợp danh mục bảng + chi tiết trường toàn hệ | Chờ duyệt |")
    a("")
    a("---")
    a("")
    a("## 1. Giới thiệu")
    a("")
    a("### 1.1. Mục đích")
    a(
        "Cung cấp **một tài liệu duy nhất** để tra cứu nhanh toàn bộ bảng CSDL ERP và chi tiết từng trường — "
        "phục vụ BA, Solution, DBA và Dev khi thiết kế migration / ORM."
    )
    a("")
    a("### 1.2. Cấu trúc tài liệu")
    a("1. **Phần A** — Danh mục tổng hợp tất cả bảng (module · nhóm · tên · chức năng).")
    a("2. **Phần B** — Mô tả chi tiết trường từng bảng (tên · kiểu · ý nghĩa · ghi chú).")
    a("")
    a("### 1.3. Quy ước")
    a("- Tên bảng dạng `schema.table` (PostgreSQL-oriented).")
    a("- Hầu hết bảng nghiệp vụ có cột chuẩn: `id`, `tenant_id`, `created_*`, `updated_*`, `is_deleted`, `row_version` (xem DDD-01).")
    a("- Kiểu dữ liệu là **gợi ý logic Phase 1**; có thể map sang SQL Server tương đương (DDD-06).")
    a("")
    a("### 1.4. Mô hình phân quyền (chốt sớm — hạn chế sửa lại)")
    a("")
    a("| Trục | Thực thể chính | Nội dung |")
    a("|---|---|---|")
    a("| Quyền chức năng | `role`, `permission`, `role_permission`, `user_role` | User được **làm gì** (`module.resource.action`) |")
    a("| Tổ chức người | `department`, `job_level`, `app_user`, `user_department` | Phòng ban + cấp bậc |")
    a("| Data scope 4 tầng | `job_level.default_scope_type` + `role.bypass_data_scope` | Own / Team / Department / All |")
    a("| Phạm vi đa điểm | `user_data_scope` | Chi nhánh / kho / cửa hàng / dự án |")
    a("| Bổ sung | `field_permission`, `menu_item` | Trường nhạy cảm + menu UI |")
    a("")
    a(
        "Chi tiết quan hệ và runtime: **DDD-01 §5** và **DDD-02 §2–3**. "
        "Tham chiếu mẫu: Digi ERP (`ScopeType` + Role/Permission/Department/JobLevel)."
    )
    a("")
    a("---")
    a("")
    a("## 2. Phần A — Danh mục tổng hợp tất cả bảng")
    a("")
    a(f"Tổng số: **{len(catalog)} bảng**.")
    a("")
    a("| STT | Module | Nhóm bảng | Tên bảng | Chức năng bảng |")
    a("|---:|---|---|---|---|")
    for i, tb in enumerate(catalog, 1):
        a(
            f"| {i} | {esc(tb['module'])} | {esc(tb['group'])} | `{esc(tb['name'])}` | {esc(tb['purpose'])} |"
        )
    a("")
    a("---")
    a("")
    a("## 3. Phần B — Mô tả chi tiết trường theo từng bảng")
    a("")
    a(
        "Mỗi bảng dưới đây có bảng con với các cột: **Tên trường**, **Kiểu dữ liệu**, **Ý nghĩa**, **Ghi chú**."
    )
    a("")

    # Group by module for headings
    current_module = None
    table_idx = 0
    for tb in catalog:
        if tb["module"] != current_module:
            current_module = tb["module"]
            a(f"### 3.{_module_ord(current_module)}. Module {current_module}")
            a("")
        table_idx += 1
        a(f"#### Bảng {table_idx}. `{tb['name']}` — {tb['purpose']}")
        a("")
        a(f"- **Module:** {tb['module']}")
        a(f"- **Nhóm bảng:** {tb['group']}")
        a("")
        a("| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |")
        a("|---|---|---|---|")
        for f in tb["fields"]:
            a(
                f"| `{esc(f['name'])}` | {esc(f['dtype'])} | {esc(f['meaning'])} | {esc(f['note'])} |"
            )
        a("")

    a("---")
    a("")
    a("## 4. Truy vết")
    a("")
    a("| Tài liệu liên quan | Vị trí |")
    a("|---|---|")
    a("| Chuẩn DDD | `00_CHUAN_TAI_LIEU_DDD.md` |")
    a("| DDD chi tiết theo nhóm | `DDD-01` … `DDD-06` |")
    a("| Tích hợp / sự kiện | `../02. Tích hợp liên module` |")
    a("| SRS module | `../01. Modules` |")
    a("")
    a("---")
    a("")
    a("*Hết DDD-MASTER-v1.0 — Thiết kế tổng hợp cơ sở dữ liệu.*")
    a("")
    return "\n".join(lines)


_MODULE_ORDER = [
    "SYS", "WF", "HRM", "LMS", "CRM", "POS", "PUR", "INV",
    "LOG", "MFG", "FIN", "AST", "FSM", "PJM", "BI", "PRT",
]


def _module_ord(code: str) -> int:
    try:
        return _MODULE_ORDER.index(code) + 1
    except ValueError:
        return 99


def main() -> None:
    print("Building master markdown…")
    md = build_markdown()
    OUT_MD.write_text(md, encoding="utf-8")
    print(f"Wrote {OUT_MD.name} ({len(md.splitlines())} lines, {OUT_MD.stat().st_size // 1024} KB)")

    print("Building professional DOCX (may take a few minutes)…")
    meta = extract_meta_from_md(OUT_MD)
    meta.update(
        {
            "org": "ERP MODULAR PRODUCT",
            "title": "Thiết kế tổng hợp cơ sở dữ liệu",
            "subtitle": "Database Design Document — Master Catalog (Tables & Fields)",
            "doc_family_en": "DATABASE DESIGN DOCUMENT",
            "doc_family_vi": "TÀI LIỆU THIẾT KẾ CƠ SỞ DỮ LIỆU TỔNG HỢP",
            "doc_code": "DDD-MASTER-v1.0",
            "module_code": "ALL",
            "module_label": "Phạm vi",
            "module_name": "Thiết kế tổng hợp CSDL (16 module)",
            "version": "1.0",
            "status": "Chờ duyệt — Database Design Master",
            "classification": "Nội bộ dự án — Solution / DBA",
            "footer_note": "Tài liệu tổng hợp danh mục bảng + chi tiết trường — dùng trước khi viết migration / ORM.",
            "history": [
                ["1.0", TODAY, "Solution / DBA", "Tổng hợp toàn bộ bảng + chi tiết trường", "Chờ duyệt"],
            ],
        }
    )
    build(OUT_MD, OUT_DOCX, meta=meta)
    print(f"Done: {OUT_DOCX} ({OUT_DOCX.stat().st_size // 1024} KB)")


if __name__ == "__main__":
    main()
