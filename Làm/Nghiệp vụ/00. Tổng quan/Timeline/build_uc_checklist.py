# -*- coding: utf-8 -*-
"""
Sinh / cập nhật CHECKLIST_UC.md từ cay_chuc_nang_data.TREE.

- Nguồn UC: ../cay_chuc_nang_data.py (catalog chính thức)
- Tiến độ: uc_progress.json (giữ khi regenerate)
- Output chuẩn: CHECKLIST_UC.md
- File cũ CHECKLIST_UC_TOAN_BO.md: stub chuyển hướng
- Chạy: python build_uc_checklist.py

Quy ước cập nhật liên tục:
1. Code xong 1 UC → sửa uc_progress.json (done=true, pct, note)
2. Chạy lại script → CHECKLIST_UC.md được sinh lại
3. Không sửa tay hàng loạt trong MD — sửa JSON rồi regenerate
"""
from __future__ import annotations

import json
import sys
from collections import OrderedDict
from datetime import date
from pathlib import Path

HERE = Path(__file__).resolve().parent
PARENT = HERE.parent
sys.path.insert(0, str(PARENT))
from cay_chuc_nang_data import TREE  # noqa: E402

PROGRESS_PATH = HERE / "uc_progress.json"
OUT_PATH = HERE / "CHECKLIST_UC.md"
LEGACY_OUT_PATH = HERE / "CHECKLIST_UC_TOAN_BO.md"

PRIO_MAP = {
    "Bắt buộc": "Must",
    "Cao": "Should",
    "Trung bình": "Could",
    "Thấp": "Won't",
}

# Seed tiến độ M1 Day-1 (E2E-05) — chỉ ghi nếu key chưa có trong progress
M1_DAY1_DONE: dict[str, dict] = {
    # SYS
    "UC_SYS_001": {"done": True, "pct": 100, "note": "M1 · login JWT"},
    "UC_SYS_013": {"done": True, "pct": 80, "note": "M1 · API upsert + FE list"},
    "UC_SYS_014": {"done": True, "pct": 80, "note": "M1 · API upsert"},
    "UC_SYS_021": {"done": True, "pct": 80, "note": "M1 · list + data scope"},
    "UC_SYS_023": {"done": True, "pct": 90, "note": "M1 · API + FE roles"},
    "UC_SYS_025": {"done": True, "pct": 100, "note": "M1 · permission catalog"},
    "UC_SYS_026": {"done": True, "pct": 100, "note": "M1 · set role permissions"},
    "UC_SYS_027": {"done": True, "pct": 100, "note": "M1 · set user roles"},
    "UC_SYS_028": {"done": True, "pct": 80, "note": "M1 · data scope org"},
    "UC_SYS_030": {"done": True, "pct": 80, "note": "M1 · data scope dept"},
    "UC_SYS_035": {"done": True, "pct": 90, "note": "M1 · org-units API/FE"},
    "UC_SYS_037": {"done": True, "pct": 90, "note": "M1 · departments API/FE"},
    "UC_SYS_038": {"done": True, "pct": 80, "note": "M1 · job titles HRM + job levels SYS"},
    "UC_SYS_047": {"done": True, "pct": 100, "note": "M1 · license middleware"},
    "UC_SYS_051": {"done": True, "pct": 100, "note": "M1 · dynamic menu"},
    "UC_SYS_052": {"done": True, "pct": 100, "note": "M1 · API license gate"},
    "UC_SYS_068": {"done": True, "pct": 90, "note": "M1 · local file upload"},
    "UC_SYS_069": {"done": True, "pct": 80, "note": "M1 · download by key"},
    "UC_SYS_081": {"done": True, "pct": 90, "note": "M1 · AuditSaveChangesInterceptor"},
    # HRM — IDs depend on order; filled after build_catalog by title match below
}


def prio_code(vn: str) -> str:
    return PRIO_MAP.get(vn, vn)


def build_catalog() -> list[dict]:
    rows: list[dict] = []
    for mod, groups in TREE.items():
        seq = 0
        for gcode, gname, fns in groups:
            for title, en, prio_vn in fns:
                seq += 1
                uc_id = f"UC_{mod}_{seq:03d}"
                rows.append(
                    {
                        "id": uc_id,
                        "module": mod,
                        "group_code": gcode,
                        "group": gname,
                        "title": title,
                        "en": en,
                        "prio_vn": prio_vn,
                        "prio": prio_code(prio_vn),
                    }
                )
    return rows


def load_progress() -> dict:
    if PROGRESS_PATH.exists():
        return json.loads(PROGRESS_PATH.read_text(encoding="utf-8"))
    return {}


def seed_by_title(progress: dict, catalog: list[dict]) -> None:
    """Gán tiến độ M1 Day-1 theo tiêu đề UC (ổn định hơn số thứ tự nếu catalog đổi nhẹ)."""
    title_done = {
        ("SYS", "Đăng nhập hệ thống"): ("M1 · login JWT", 100),
        ("SYS", "Tạo người dùng"): ("M1 · API upsert + FE list", 80),
        ("SYS", "Cập nhật thông tin người dùng"): ("M1 · API upsert", 80),
        ("SYS", "Tìm kiếm / lọc người dùng"): ("M1 · list + data scope", 80),
        ("SYS", "Tạo / sửa / ngưng vai trò (Role)"): ("M1 · API + FE roles", 90),
        ("SYS", "Quản lý danh mục quyền (Permission)"): ("M1 · catalog", 100),
        ("SYS", "Gán quyền vào vai trò"): ("M1 · set role permissions", 100),
        ("SYS", "Gán người dùng vào vai trò"): ("M1 · set user roles", 100),
        ("SYS", "Phân quyền dữ liệu theo chi nhánh"): ("M1 · data scope", 80),
        ("SYS", "Phân quyền theo phòng ban"): ("M1 · data scope dept", 80),
        ("SYS", "Quản lý chi nhánh"): ("M1 · org-units FE", 90),
        ("SYS", "Quản lý phòng ban"): ("M1 · departments FE", 90),
        ("SYS", "Quản lý chức danh"): ("M1 · job level/title", 80),
        ("SYS", "Bật / tắt module theo tenant"): ("M1 · license middleware", 100),
        ("SYS", "Menu động theo module + quyền"): ("M1 · shell menu", 100),
        ("SYS", "Ẩn API module chưa mua"): ("M1 · license API gate", 100),
        ("SYS", "Upload file"): ("M1 · local storage", 90),
        ("SYS", "Tải xuống / xem trước file"): ("M1 · download API", 80),
        ("SYS", "Nhật ký thao tác người dùng"): ("M1 · audit interceptor", 90),
        ("SYS", "Khóa / mở khóa người dùng"): ("M1 · status Locked/Disabled trên SideSheet", 70),
        ("SYS", "Hàng đợi sự kiện liên module"): ("G4 · outbox_message + dispatcher", 70),
        ("SYS", "Tạo hội thoại 1-1"): ("G4.9 · Direct conversation API/FE", 100),
        ("SYS", "Gửi tin nhắn realtime"): ("G4.9 · send message REST", 100),
        ("SYS", "Nhận tin nhắn realtime (SignalR)"): ("G4.9 · SignalR /hubs/msg", 100),
        ("SYS", "Xem lịch sử hội thoại"): ("G4.9 · message history", 100),
        ("SYS", "Đánh dấu đã đọc / badge chưa đọc"): ("G4.9 · read + unread badge", 100),
        ("HRM", "Quản lý chức danh nhân sự"): ("M1 · job titles", 100),
        ("HRM", "Quản lý loại nhân sự"): ("M1 · employee types", 100),
        ("HRM", "Quản lý cấp bậc / level"): ("M1 · job levels SYS", 80),
        ("HRM", "Tạo hồ sơ nhân sự mới"): ("M1 · employee upsert", 100),
        ("HRM", "Cập nhật thông tin cá nhân"): ("M1 · employee upsert", 100),
        ("HRM", "Gắn nhân sự vào đơn vị chính"): ("M1 · org on employee", 100),
        ("HRM", "Gắn nhân sự vào bộ phận"): ("M1 · dept on employee", 100),
        ("HRM", "Gắn chức danh / level"): ("M1 · title/level", 100),
        ("HRM", "Gắn loại nhân sự"): ("M1 · employee type", 100),
        ("HRM", "Tìm kiếm nhân sự đa tiêu chí"): ("M1 · list q=", 90),
        ("HRM", "Xem hồ sơ theo quyền"): ("M1 · data scope", 90),
        ("HRM", "Tạo hợp đồng lao động"): ("M1 · contract upsert", 90),
        ("HRM", "Lịch sử hợp đồng theo nhân sự"): ("M1 · contract list", 80),
        ("HRM", "Danh mục loại nghỉ"): ("M1 · leave types", 100),
        ("HRM", "Tạo đơn xin nghỉ"): ("M1 · leave request", 100),
        ("HRM", "Duyệt đơn nghỉ đa cấp"): ("M1 · WF 1 cấp Approve/Reject", 70),
        ("HRM", "Xem quỹ phép còn lại"): ("M1 · leave balances", 100),
        ("WF", "Tạo mẫu workflow duyệt"): ("M1 · seed LEAVE_APPROVE", 70),
        ("WF", "Gắn workflow vào loại chứng từ"): ("M1 · leave_request", 80),
        ("WF", "Hộp chờ duyệt của tôi"): ("M1 · /app/wf/tasks", 100),
        ("WF", "Duyệt / từ chối / trả bổ sung"): ("M1 · Approve/Reject", 90),
        ("WF", "Lịch sử duyệt & comment"): ("M1 · WfTaskAction", 70),
        ("WF", "Task liên kết chứng từ ERP"): ("M1 · sourceDoc leave", 90),
    }
    by_key = {(r["module"], r["title"]): r["id"] for r in catalog}
    for key, (note, pct) in title_done.items():
        uc_id = by_key.get(key)
        if not uc_id:
            continue
        if uc_id in progress:
            continue
        progress[uc_id] = {"done": True, "pct": pct, "note": note}


def save_progress(progress: dict) -> None:
    ordered = OrderedDict(sorted(progress.items()))
    PROGRESS_PATH.write_text(
        json.dumps(ordered, ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8",
    )


def render(catalog: list[dict], progress: dict) -> str:
    today = date.today().strftime("%d/%m/%Y")
    total = len(catalog)
    done_n = sum(1 for r in catalog if progress.get(r["id"], {}).get("done"))
    pct_all = round(100 * done_n / total, 1) if total else 0

    by_mod: OrderedDict[str, list[dict]] = OrderedDict()
    for r in catalog:
        by_mod.setdefault(r["module"], []).append(r)

    lines: list[str] = []
    lines.append("# Checklist UC toàn bộ — Pum's ERP")
    lines.append("")
    lines.append("| Thuộc tính | Giá trị |")
    lines.append("| --- | --- |")
    lines.append("| Mã | `CHECKLIST-UC-v2` |")
    lines.append(f"| Cập nhật lần | {today} |")
    lines.append("| Nguồn catalog | [`cay_chuc_nang_data.py`](../cay_chuc_nang_data.py) |")
    lines.append("| Tiến độ máy | [`uc_progress.json`](./uc_progress.json) |")
    lines.append("| Sinh lại | `python Timeline/build_uc_checklist.py` → `CHECKLIST_UC.md` |")
    lines.append("| Tổng UC | **{:,}** |".format(total).replace(",", "."))
    lines.append(f"| Đã xong | **{done_n}** ({pct_all}%) |")
    lines.append("| Kế hoạch giai đoạn | [CHECKLIST_TIEN_DO_GIAI_DOAN.md](../CHECKLIST_TIEN_DO_GIAI_DOAN.md) |")
    lines.append("")
    lines.append("> Living checklist — **mỗi UC một dòng**. Khi implement xong: cập nhật `uc_progress.json` (hoặc đánh dấu rồi sync) rồi chạy lại script. Không ghi đè tay hàng loạt.")
    lines.append("")
    lines.append("### Quy ước cột")
    lines.append("")
    lines.append("| Cột | Nghĩa |")
    lines.append("| --- | --- |")
    lines.append("| Ưu tiên | Must ← Bắt buộc · Should ← Cao · Could ← Trung bình · Won't ← Thấp |")
    lines.append("| Xong? | `[x]` đạt DoD tối thiểu (API hoặc UI đủ dùng) · `[~]` partial · `[ ]` chưa |")
    lines.append("| % | 0–100 theo độ sâu (Day-1 khung có thể <100) |")
    lines.append("")
    lines.append("## A. Tổng hợp theo module")
    lines.append("")
    lines.append("| Module | Tổng UC | Xong | % | Must còn |")
    lines.append("| --- | ---: | ---: | ---: | ---: |")

    for mod, rows in by_mod.items():
        d = sum(1 for r in rows if progress.get(r["id"], {}).get("done"))
        p = round(100 * d / len(rows), 1) if rows else 0
        must_left = sum(
            1
            for r in rows
            if r["prio"] == "Must" and not progress.get(r["id"], {}).get("done")
        )
        lines.append(f"| {mod} | {len(rows)} | {d} | {p} | {must_left} |")

    lines.append("")
    lines.append("---")
    lines.append("")

    for mod, rows in by_mod.items():
        d = sum(1 for r in rows if progress.get(r["id"], {}).get("done"))
        lines.append(f"## {mod} ({d}/{len(rows)})")
        lines.append("")
        lines.append("| ID | Nhóm | UC | Ưu tiên | Xong? | % | Ghi chú |")
        lines.append("| --- | --- | --- | --- | --- | ---: | --- |")
        for r in rows:
            st = progress.get(r["id"], {})
            done = st.get("done", False)
            partial = st.get("partial", False)
            mark = "[x]" if done else ("[~]" if partial else "[ ]")
            pct = st.get("pct", 100 if done else 0)
            note = (st.get("note") or "").replace("|", "/")
            lines.append(
                f"| `{r['id']}` | {r['group']} | {r['title']} | {r['prio']} | {mark} | {pct} | {note} |"
            )
        lines.append("")

    lines.append("---")
    lines.append("")
    lines.append("## B. Nhật ký")
    lines.append("")
    lines.append("| Ngày | Thay đổi |")
    lines.append("| --- | --- |")
    # Giữ nhật ký ổn định (không ghi đè mỗi lần regenerate)
    lines.append("| 04/08/2026 | Sinh checklist từ catalog (1092 UC); seed tiến độ M1 Day-1 SYS/HRM/WF |")
    lines.append(
        f"| {today} | Cap-2 HRM gần đủ (tuyển→chấm công→lương→KT/KL→offboard→dashboard `182–187`, skip `174`) + WF `032`/`040` · "
        f"**{done_n}/{total}** UC (xem `uc_progress.json` / PHAN_NHOM_UC_CAC_MODULE.md) |"
    )
    lines.append("")
    return "\n".join(lines)


def main() -> None:
    catalog = build_catalog()
    progress = load_progress()
    seed_by_title(progress, catalog)
    # drop stale keys not in catalog
    valid = {r["id"] for r in catalog}
    progress = {k: v for k, v in progress.items() if k in valid}
    save_progress(progress)
    OUT_PATH.write_text(render(catalog, progress), encoding="utf-8")
    # Stub file cũ — tránh tab IDE giữ buffer lỗi; luôn trỏ sang file chuẩn
    LEGACY_OUT_PATH.write_text(
        "\n".join(
            [
                "# Checklist UC — đã chuyển file",
                "",
                f"> **Dùng file chuẩn:** [`CHECKLIST_UC.md`](./CHECKLIST_UC.md)",
                "",
                "File `CHECKLIST_UC_TOAN_BO.md` giữ lại chỉ để tương thích link cũ.",
                "Mỗi lần cập nhật UC: sửa `uc_progress.json` rồi chạy `python build_uc_checklist.py`.",
                "",
            ]
        ),
        encoding="utf-8",
    )
    done_n = sum(1 for r in catalog if progress.get(r["id"], {}).get("done"))
    print(f"Wrote {OUT_PATH.name}: {len(catalog)} UC, {done_n} done")
    print(f"Stub: {LEGACY_OUT_PATH.name} → {OUT_PATH.name}")
    print(f"Progress: {PROGRESS_PATH.name}")


if __name__ == "__main__":
    main()
