#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Khung chung SRS v1.1 — bảng UC 8 trường + shell tài liệu (chuẩn SYS)."""
from __future__ import annotations

from datetime import date
from pathlib import Path
from typing import Any

PRIORITY_MOSCOW = {
    "Bắt buộc": "Must",
    "Cao": "Should",
    "Trung bình": "Could",
    "Thấp": "Later",
}

TODAY = date.today().strftime("%d/%m/%Y")


def uc(
    ma: str,
    ten: str,
    prio: str,
    actor: str,
    mo_ta: str,
    tien: list[str],
    luong: list[str],
    ngoai: list[str],
    hau: list[str],
    br: list[str],
    ac: list[str],
) -> dict[str, Any]:
    return {
        "ma": ma,
        "ten": ten,
        "prio": prio,
        "actor": actor,
        "mo_ta": mo_ta,
        "tien": tien,
        "luong": luong,
        "ngoai": ngoai,
        "hau": hau,
        "br": br,
        "ac": ac,
    }


def _br_bullets(items: list[str]) -> str:
    return "<br>".join(f"• {x}" for x in items if x)


def _br_numbered(items: list[str]) -> str:
    return "<br>".join(f"{i}. {x}" for i, x in enumerate(items, 1) if x)


def _format_preconditions(u: dict) -> str:
    items = ["Hệ thống ERP đang hoạt động bình thường."]
    actor = u["actor"]
    public_keys = ("đăng nhập", "quên mật khẩu", "đặt lại mật khẩu", "sso", "đăng ký cổng", "self-register")
    ten_l = u["ten"].lower()
    if any(k in ten_l for k in public_keys):
        items.append(
            f"Người dùng có định danh hợp lệ thuộc nhóm đối tượng [{actor}] "
            "(hoặc được cấp tài khoản tương ứng) để thực hiện chức năng."
        )
    else:
        items.append(
            f"Người dùng đã đăng nhập tài khoản thuộc vai trò [{actor}] "
            "và được cấp quyền RBAC tương ứng."
        )
    for x in u["tien"]:
        if x not in items:
            items.append(x)
    return _br_bullets(items)


def _format_requirements(u: dict) -> str:
    items = [
        "Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.",
        "Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).",
        "Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.",
        f"Ưu tiên triển khai (MoSCoW): **{u['prio']}**.",
    ]
    if u["br"]:
        items.append("Quy tắc nghiệp vụ liên quan: " + ", ".join(f"`{b}`" for b in u["br"]) + ".")
    if u["hau"]:
        items.append("Hậu điều kiện: " + " ".join(u["hau"]))
    for i, ac in enumerate(u["ac"], 1):
        items.append(f"Tiêu chí chấp nhận AC{i}: {ac}")
    return _br_bullets(items)


def _format_alt_flows(u: dict) -> str:
    parts = [
        "3.1. Người dùng nhấn nút [Hủy / Thoát]:",
        "  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.",
        "4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:",
        "  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.",
        "  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.",
        "5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:",
        "  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.",
    ]
    n = 6
    for x in u["ngoai"]:
        parts.append(f"{n}.1. {x}")
        n += 1
    return "<br>".join(parts)


def render_uc(u: dict, table_no: int, group_name: str, module_code: str, module_title: str) -> list[str]:
    mo_ta = (
        f"Cho phép {u['actor']} thực hiện chức năng \"{u['ten']}\" "
        f"thuộc nhóm {group_name} trong module {module_code} — {module_title}. "
        f"Mô tả chi tiết: {u['mo_ta']}"
    )
    rows = [
        ("**Use Case ID**", u["ma"]),
        ("**Tên Use Case**", u["ten"]),
        ("**Tác nhân**", u["actor"]),
        ("**Mô tả chức năng**", mo_ta),
        ("**Điều kiện tiên quyết**", _format_preconditions(u)),
        ("**Yêu cầu**", _format_requirements(u)),
        ("**Kịch bản chính**", _br_numbered(u["luong"])),
        ("**Kịch bản phụ**", _format_alt_flows(u)),
    ]
    lines = [
        f"**Bảng {table_no}. Đặc tả Use Case \"{u['ten']}\"**",
        "",
        "| Trường Thông Tin | Nội Dung Đặc Tả |",
        "| :--- | :--- |",
    ]
    for label, value in rows:
        safe = value.replace("|", "\\|")
        lines.append(f"| {label} | {safe} |")
    lines.append("")
    return lines


def esc(s: str) -> str:
    return (s or "").replace("|", "\\|")


def build_srs_markdown(
    *,
    code: str,
    meta: dict,
    groups: list[tuple[str, str, str, list[dict]]],
    folder_note: str = "",
) -> str:
    """
    groups: list of (group_code, group_name, group_desc, list[uc_dict])
    """
    total_uc = sum(len(g[3]) for g in groups)
    module_title = meta["ten"]
    lines: list[str] = []
    a = lines.append

    a(f"# SRS-{code}-v1.1 — {module_title}")
    a("")
    a(f"> **Software Requirements Specification — Module {code}**")
    a("> Phiên bản chỉnh chu theo chuẩn SYS v1.1; đặc tả UC bảng 8 trường, phục vụ chốt nghiệp vụ trước khi code.")
    a("> Trạng thái: **Chờ duyệt nghiệp vụ**. Nội dung generic, phục vụ bán ERP theo module.")
    if folder_note:
        a(f"> {folder_note}")
    a("")
    a("---")
    a("")
    a("## 0. Thông tin tài liệu & lịch sử")
    a("")
    a("| Thuộc tính | Giá trị |")
    a("|---|---|")
    a(f"| Mã tài liệu | `SRS-{code}-v1.1` |")
    a(f"| Module | `{code}` — {module_title} |")
    a("| Phiên bản | 1.1 |")
    a(f"| Ngày | {TODAY} |")
    a("| Phân loại | SRS nghiệp vụ (BA) |")
    a(f"| Lớp sản phẩm | {meta.get('lop', '—')} |")
    a(f"| Bán riêng | {meta.get('ban_rieng', '—')} |")
    deps = ", ".join(f"`{x}`" for x in meta.get("phu_thuoc") or []) or "—"
    a(f"| Phụ thuộc bắt buộc | {deps} |")
    rec = ", ".join(f"`{x}`" for x in meta.get("khuyen_nghi_kem") or []) or "—"
    a(f"| Khuyến nghị kèm | {rec} |")
    a(f"| Số nhóm / UC | {len(groups)} nhóm / {total_uc} UC |")
    a("| Nguồn catalog | `cay_chuc_nang_data.py` (generic v3) |")
    a("| Định dạng bàn giao | Microsoft Word (`.docx`) |")
    a("")
    a("| Ver | Ngày | Người thực hiện | Mô tả | Trạng thái |")
    a("|---|---|---|---|---|")
    a(f"| 1.0 | {TODAY} | BA / Generator | Sinh từ catalog + meta | Thay thế |")
    a(f"| 1.1 | {TODAY} | BA / Solution | Viết lại đặc tả UC chuyên sâu + chuẩn Word (như SYS) | Chờ duyệt |")
    a("")
    a("---")
    a("")
    a("## 1. Giới thiệu")
    a("")
    a("### 1.1. Mục đích")
    a(
        f"Tài liệu mô tả yêu cầu nghiệp vụ module **{module_title}** (`{code}`), "
        "làm căn cứ thống nhất phạm vi, ước lượng, thiết kế và nghiệm thu trước khi triển khai source."
    )
    a("")
    a("### 1.2. Vai trò sản phẩm")
    a(meta.get("tom_tat", ""))
    a("")
    a("### 1.3. Mục tiêu đo được")
    for i, t in enumerate(meta.get("muc_tieu") or [], 1):
        a(f"{i}. {t}")
    a("")
    a("### 1.4. Đối tượng đọc")
    a("- Chủ sản phẩm / PO, Ban dự án")
    a("- Business Analyst, Solution Architect")
    a("- Tech Lead / QA Lead")
    a("- Presales & triển khai (đóng gói bán module)")
    a("")
    a("---")
    a("")
    a("## 2. Phạm vi")
    a("")
    a("### 2.1. In Scope")
    for x in meta.get("in_scope") or []:
        a(f"- {x}")
    a("")
    a("### 2.2. Out of Scope")
    for x in meta.get("out_scope") or []:
        a(f"- {x}")
    a("")
    a("### 2.3. Đóng gói bán")
    a(f"- **Bán riêng:** {meta.get('ban_rieng', '—')}")
    a(f"- **Phụ thuộc bắt buộc:** {deps}")
    a(f"- **Khuyến nghị kèm (E2E):** {rec}")
    a("- Tính năng ngành hóa bằng cấu hình/template khi triển khai — không hard-code vào SRS gốc.")
    a("")
    a("---")
    a("")
    a("## 3. Tác nhân")
    a("")
    a("| Tác nhân | Trách nhiệm chính |")
    a("|---|---|")
    for name, role in meta.get("actors") or []:
        a(f"| {esc(name)} | {esc(role)} |")
    a("")
    a("### 3.1. Phân tách trách nhiệm gợi ý")
    a("- Cấu hình master / rule: Admin module.")
    a("- Vận hành chứng từ hàng ngày: Officer / User nghiệp vụ.")
    a("- Duyệt: Manager / Approver qua WF (nếu bật).")
    a("- Hệ thống: job nhắc hạn, tính toán batch, đồng bộ sự kiện.")
    a("")
    a("---")
    a("")
    a("## 4. Thuật ngữ")
    a("")
    a("| Thuật ngữ | Giải thích |")
    a("|---|---|")
    for term, meaning in meta.get("terms") or []:
        a(f"| {esc(term)} | {esc(meaning)} |")
    a("| Tenant | Không gian dữ liệu khách hàng trên SYS |")
    a("| RBAC | Phân quyền theo vai trò do SYS cấp |")
    a("| Data scope | Phạm vi dữ liệu (chi nhánh/đơn vị/kho…) được phép thao tác |")
    a("")
    a("---")
    a("")
    a("## 5. Ngữ cảnh kiến trúc nghiệp vụ")
    a("")
    a("```text")
    a("SYS (Auth · RBAC · License · Org · Audit · File · Notify · Event)")
    a("        |")
    a(f"        +-- {code} ({module_title})")
    a("                |-- Master / cấu hình")
    a("                |-- Chứng từ & quy trình")
    a("                +-- Báo cáo / sự kiện liên module")
    a("```")
    a("")
    a("### 5.1. Nguyên tắc phụ thuộc")
    a(f"1. Module `{code}` **bắt buộc** chạy trên SYS (identity, permission, license, audit).")
    a(f"2. Menu/API `{code}` chỉ mở khi license module active.")
    a("3. Duyệt liên phòng ban ưu tiên qua WF nếu khách mua WF; không thì dùng duyệt nội module.")
    a("4. Sự kiện liên module đi qua bus SYS (tránh gọi chéo cứng không kiểm soát).")
    a("")
    a("### 5.2. Tích hợp")
    a("")
    a("| Hướng | Hệ thống / Module | Nội dung |")
    a("|---|---|---|")
    for row in meta.get("integrations") or []:
        if len(row) == 2:
            a(f"| Tích hợp | {esc(row[0])} | {esc(row[1])} |")
        else:
            a(f"| {esc(row[0])} | {esc(row[1])} | {esc(row[2]) if len(row) > 2 else ''} |")
    a("")
    a("---")
    a("")
    a("## 6. Catalog chức năng")
    a("")
    a(f"**Tổng:** {len(groups)} nhóm · {total_uc} use case.")
    a("")
    a("| STT | Mã nhóm | Nhóm | Số UC | Must | Should | Could/Later |")
    a("|---:|---|---|---:|---:|---:|---:|")
    for i, (gcode, gname, _gd, ucs) in enumerate(groups, 1):
        must = sum(1 for u in ucs if u["prio"] == "Must")
        should = sum(1 for u in ucs if u["prio"] == "Should")
        other = len(ucs) - must - should
        a(f"| {i} | `{code}-{gcode}` | {esc(gname)} | {len(ucs)} | {must} | {should} | {other} |")
    a("")
    a("<details>")
    a("<summary>Bảng mã UC đầy đủ</summary>")
    a("")
    a("| Mã UC | Nhóm | Tên | Ưu tiên |")
    a("|---|---|---|---|")
    for gcode, gname, _gd, ucs in groups:
        for u in ucs:
            a(f"| `{u['ma']}` | {esc(gname)} | {esc(u['ten'])} | {u['prio']} |")
    a("")
    a("</details>")
    a("")
    a("### 6.1. Đề xuất Phase")
    a("| Phase | Phạm vi gợi ý |")
    a("|---|---|")
    a("| Phase 1 — Go-live | Toàn bộ **Must** |")
    a("| Phase 2 — Vận hành nâng cao | Các **Should** |")
    a("| Phase 3 — Mở rộng | **Could / Later** |")
    a("")
    a("---")
    a("")
    a("## 7. Đặc tả Use Case theo nhóm")
    a("")
    a(
        "Mỗi use case được đặc tả bằng **một bảng thống nhất** gồm 8 trường: "
        "Use Case ID, Tên Use Case, Tác nhân, Mô tả chức năng, Điều kiện tiên quyết, "
        "Yêu cầu, Kịch bản chính, Kịch bản phụ."
    )
    a("")

    table_no = 0
    for gcode, gname, gdesc, ucs in groups:
        a(f"### 7.{int(gcode)}. {gname} (`{code}-{gcode}`)")
        a("")
        a(gdesc)
        a("")
        a("| Chỉ số | Giá trị |")
        a("|---|---|")
        a(f"| Số UC | {len(ucs)} |")
        a(f"| Must | {sum(1 for u in ucs if u['prio'] == 'Must')} |")
        a("")
        for u in ucs:
            table_no += 1
            lines.extend(render_uc(u, table_no, gname, code, module_title))

    a("---")
    a("")
    a("## 8. Workflow end-to-end")
    a("")
    for wf in meta.get("workflows") or []:
        a(f"### {wf['ma']} — {wf['ten']}")
        a("")
        a(f"**Mục tiêu:** {wf['muc_tieu']}")
        a("")
        a("| Bước | Mô tả |")
        a("|---:|---|")
        for i, b in enumerate(wf.get("buoc") or [], 1):
            a(f"| {i} | {esc(b)} |")
        a("")
        a("**Điều kiện hoàn tất:** Các bước cốt lõi hoàn thành; trạng thái cuối nhất quán; có audit/trace.")
        a("")

    a("---")
    a("")
    a("## 9. Mô hình dữ liệu domain (logic)")
    a("")
    a("> Mức conceptual — chưa phải thiết kế CSDL vật lý.")
    a("")
    a("| Thực thể | Vai trò |")
    a("|---|---|")
    for ent, role in meta.get("entities") or []:
        a(f"| `{esc(ent)}` | {esc(role)} |")
    a("")
    a("### 9.1. Kiểm soát dữ liệu")
    a("- Master tham chiếu từ module sở hữu / SYS, không nhân bản lệch.")
    a("- Chứng từ có vòng đời trạng thái rõ (Draft → Submitted → Approved → Posted/Closed…).")
    a("- Soft-delete / ngưng dùng là mặc định; hạn chế xóa cứng.")
    a(f"- Mọi bản ghi nghiệp vụ gắn `TenantId` và data scope phù hợp module `{code}`.")
    a("")
    a("---")
    a("")
    a("## 10. Quy tắc nghiệp vụ tổng hợp")
    a("")
    for br in meta.get("business_rules") or []:
        a(f"- {br}")
    a(f"- BR-{code}-GEN-01: Thao tác thay đổi dữ liệu phải thuộc data scope của user.")
    a(f"- BR-{code}-GEN-02: Chứng từ có mã duy nhất theo Sequence SYS (nếu áp dụng).")
    a(f"- BR-{code}-GEN-03: Sau khóa kỳ/chốt sổ (nếu có), chỉ điều chỉnh có kiểm soát + audit.")
    a("")
    a("---")
    a("")
    a("## 11. Yêu cầu phi chức năng (NFR)")
    a("")
    a("| Nhóm | Yêu cầu |")
    a("|---|---|")
    for k, v in meta.get("nfr") or []:
        a(f"| {esc(k)} | {esc(v)} |")
    a("| Usability | Form validate rõ; bảng lọc/phân trang; tiếng Việt |")
    a("| Reliability | Giao dịch quan trọng atomic; không mất chứng từ đã post |")
    a("| Audit | Truy vết who/when/before/after với thay đổi trọng yếu |")
    a("")
    a("---")
    a("")
    a("## 12. Tích hợp & sự kiện liên module")
    a("")
    a(f"- Module `{code}` phát/nhận sự kiện qua SYS Event Bus theo catalog tích hợp mục 5.2.")
    a("- Lỗi đồng bộ phải retry được và có nhật ký; không im lặng nuốt lỗi.")
    a("- Khi tắt license module: chặn API/UI; **giữ dữ liệu** theo chính sách lưu trữ.")
    a("")
    a("---")
    a("")
    a("## 13. Phân quyền & bảo mật")
    a("")
    a("| Nhóm quyền gợi ý | Mô tả |")
    a("|---|---|")
    for p in meta.get("permissions") or []:
        a(f"| `{esc(p)}` | Quyền chức năng module |")
    a(f"| `{code.lower()}.*.view` | Xem trong data scope |")
    a(f"| `{code.lower()}.*.manage` | Tạo/sửa trong data scope |")
    a(f"| `{code.lower()}.*.approve` | Duyệt chứng từ (nếu có) |")
    a("")
    a("- Field-level security áp dụng cho dữ liệu nhạy cảm (lương, CCCD, giá vốn…).")
    a("- Mọi từ chối quyền ghi audit.")
    a("")
    a("---")
    a("")
    a("## 14. Báo cáo & KPI")
    a("")
    a("| KPI / Báo cáo | Mục đích |")
    a("|---|---|")
    for k in meta.get("kpis") or []:
        a(f"| {esc(k)} | Theo dõi vận hành module |")
    a("")
    a("---")
    a("")
    a("## 15. Giả định, rủi ro, câu hỏi mở")
    a("")
    a("### 15.1. Giả định")
    for x in meta.get("assumptions") or [
        "Khách hàng = một Tenant trên SYS (multi-company trong tenant nếu cấu hình).",
        f"Module `{code}` đăng ký permission/menu/sequence khi được bật license.",
    ]:
        a(f"- {x}")
    a("")
    a("### 15.2. Rủi ro")
    a("| Rủi ro | Mức | Hướng xử lý |")
    a("|---|---|---|")
    a("| Sai cấu hình data scope → lộ dữ liệu | Cao | Role template + test case scope |")
    a("| Tắt module làm gãy tích hợp E2E | Trung bình | Dependency check khi bật/tắt |")
    a("| Duyệt tắc nghẽn khi thiếu WF | Trung bình | Escalation / ủy quyền |")
    a("")
    a("### 15.3. Câu hỏi cần chốt")
    for i, q in enumerate(meta.get("open_questions") or [
        f"Phase 1 của `{code}` có bắt buộc kèm WF không?",
        "Chính sách giữ dữ liệu khi hủy license module?",
    ], 1):
        a(f"{i}. {q}")
    a("")
    a("---")
    a("")
    a("## 16. Tiêu chí nghiệm thu & truy vết")
    a("")
    a("### 16.1. Điều kiện chấp nhận module")
    a("1. 100% UC **Must** pass UAT.")
    a("2. Các workflow mục 8 chạy thành công trên môi trường demo.")
    a("3. Kiểm thử license: tắt module → menu mất + API 403; dữ liệu vẫn còn.")
    a("4. Kiểm thử RBAC + data scope với ≥ 2 role và ≥ 2 đơn vị/chi nhánh (nếu áp dụng).")
    a("5. Audit có before/after cho thao tác trọng yếu.")
    a("6. Không còn UC dùng luồng khuôn mẫu sai lệch nghiệp vụ.")
    a("")
    a("### 16.2. Truy vết")
    a("| Artifact | Vị trí |")
    a("|---|---|")
    a("| Catalog chức năng | `../../00. Tổng quan/cay_chuc_nang_data.py` |")
    a("| Excel tổng hợp | `../../00. Tổng quan/Danh_muc_Module_Chuc_nang_ERP_v3.xlsx` |")
    a("| Chuẩn SRS | `../00_CHUAN_VIET_SRS.md` |")
    a(f"| Bản SRS này | `SRS_{code}_v1.1.md` / `.docx` |")
    a(f"| UC IDs | `UC_{code}_001` … |")
    a("")
    a("---")
    a("")
    a(f"*Hết tài liệu SRS-{code}-v1.1. Sau khi duyệt, đóng băng phiên bản và chuyển sang thiết kế source.*")
    a("")
    return "\n".join(lines)


def write_srs_file(path: Path, content: str) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(content, encoding="utf-8")
    print(f"Wrote {path} ({len(content.splitlines())} lines)")
