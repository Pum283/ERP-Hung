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


# Số page.tsx thực tế dưới src/app/app/<module> (rà 06/08/2026) — không đồng nghĩa 1 page = đủ UC
FE_PAGE_COUNT: dict[str, int] = {
    "SYS": 10,
    "HRM": 18,
    "LMS": 8,
    "CRM": 7,
    "POS": 6,
    "PUR": 5,
    "INV": 6,
    "LOG": 4,
    "MFG": 3,
    "FSM": 4,
    "PJM": 3,
    "FIN": 9,
    "AST": 5,
    "WF": 5,
    "BI": 3,
    "PRT": 3,
}

# Rủi ro chất lượng còn lại dù [x] DoD khung (xem Rà xoát UC.md)
MODULE_RISK: dict[str, str] = {
    "SYS": "Cao hơn Day-1; Email/SMS stub+IntegrationCallLog wired; 2FA/trusted-device còn Dev",
    "HRM": "Cap-2 dày; sync máy chấm công wired thật; một phần Day-1 khung",
    "LMS": "Cert/thanh toán còn mock",
    "CRM": "Auto-intake + báo giá Email/PDF text thật + marketing Cap-2; còn Should omni",
    "POS": "BOM→INV + stock alerts + đóng ca→FIN + sync catalog INV→POS + in HĐ/BC ca wired thật",
    "PUR": "Đẩy INV/AP + xuất PO CSV wired thật; RFQ/hợp đồng còn thiếu",
    "INV": "Cap-2 FEFO/hold/HSD có; một phần UC còn thiếu",
    "LOG": "GPS/route entity mỏng",
    "MFG": "Đẩy giá thành INV + JE WIP→TP thật; ca/báo cáo nâng cao còn thiếu",
    "FSM": "Cap-2 parts/ticket; APP mobile Must ngoài scope",
    "PJM": "Cap-2 progress/cost; FE mỏng (~3 trang)",
    "FIN": "BT Auto + JE thu/NH/AR/AP wired thật; cash-flow còn đơn giản hóa",
    "AST": "Đẩy BT KH → FIN JE thật (Posted cân Nợ/Có); IoT/thanh lý nâng cao còn thiếu",
    "WF": "Cap-1/2 duyệt; mobile/WF nâng cao còn thiếu",
    "BI": "KPI actual còn stub một phần; dataset/widget/export Cap-2 đã live",
    "PRT": "Login/forgot stub; portal mỏng (~3 trang)",
}


def _avg_pct(rows: list[dict], progress: dict) -> float:
    vals: list[float] = []
    for r in rows:
        st = progress.get(r["id"], {})
        if st.get("done"):
            vals.append(float(st.get("pct", 100)))
        elif st.get("partial"):
            vals.append(float(st.get("pct", 50)))
    if not vals:
        return 0.0
    return round(sum(vals) / len(vals), 1)


def render(catalog: list[dict], progress: dict) -> str:
    today = date.today().strftime("%d/%m/%Y")
    total = len(catalog)
    done_n = sum(1 for r in catalog if progress.get(r["id"], {}).get("done"))
    partial_n = sum(
        1
        for r in catalog
        if not progress.get(r["id"], {}).get("done")
        and progress.get(r["id"], {}).get("partial")
    )
    todo_n = total - done_n - partial_n
    pct_all = round(100 * done_n / total, 1) if total else 0
    must_left_all = sum(
        1
        for r in catalog
        if r["prio"] == "Must" and not progress.get(r["id"], {}).get("done")
    )
    must_done_all = sum(
        1
        for r in catalog
        if r["prio"] == "Must" and progress.get(r["id"], {}).get("done")
    )
    must_total = sum(1 for r in catalog if r["prio"] == "Must")

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
    lines.append("| Rà soát chất lượng | [`Rà xoát UC.md`](./Rà%20xoát%20UC.md) |")
    lines.append("| Tổng UC | **{:,}** |".format(total).replace(",", "."))
    lines.append(
        f"| Đã xong DoD khung `[x]` | **{done_n}** ({pct_all}%) — API/UI đủ dùng Cap-1/2, **không** = production chỉnh chu |"
    )
    lines.append(f"| Partial `[~]` | **{partial_n}** |")
    lines.append(f"| Chưa `[ ]` | **{todo_n}** |")
    lines.append(
        f"| Must (DoD khung) | **{must_done_all}/{must_total}** `[x]` · còn **{must_left_all}** "
        f"(một phần vẫn stub — xem cột Rủi ro / Rà xoát UC) |"
    )
    lines.append(
        "| Test BE (xUnit chạy được) | **570+** pass — nhiều case Batch assert giả; slice Cap-2 mới = InMemory thật |"
    )
    lines.append(
        "| Test FE | **~133** node:test (helpers CRM/POS/PUR/AST/BI/SYS…) — chưa Jest/Vitest/Playwright E2E |"
    )
    lines.append("| FE `page.tsx` (app) | **~99** (kể cả redirect / catch-all) |")
    lines.append("| Kế hoạch giai đoạn | [CHECKLIST_TIEN_DO_GIAI_DOAN.md](../CHECKLIST_TIEN_DO_GIAI_DOAN.md) |")
    lines.append("")
    lines.append(
        "> Living checklist — **mỗi UC một dòng**. Cập nhật `uc_progress.json` rồi chạy lại script. "
        "**Không** ghi đè tay hàng loạt / không đánh 100% khi còn stub. "
        "Cột `[x]` = DoD khung; xem cột **Rủi ro** và [`Rà xoát UC.md`](./Rà%20xoát%20UC.md)."
    )
    lines.append("")
    lines.append("### Quy ước cột")
    lines.append("")
    lines.append("| Cột | Nghĩa |")
    lines.append("| --- | --- |")
    lines.append("| Ưu tiên | Must ← Bắt buộc · Should ← Cao · Could ← Trung bình · Won't ← Thấp |")
    lines.append("| Xong? | `[x]` DoD khung · `[~]` partial · `[ ]` chưa |")
    lines.append("| % | 0–100 độ sâu (Day-1/stub thường <100) |")
    lines.append("| `[x]` / `[~]` / `[ ]` (bảng A) | Đếm theo module từ `uc_progress.json` |")
    lines.append("| Must [x] / Must còn | Must đã đánh xong vs còn lại |")
    lines.append("| Should còn | Should chưa `[x]` |")
    lines.append("| Khác còn | Could + Won't chưa `[x]` |")
    lines.append("| Avg % | Trung bình `pct` của UC đã `[x]`/`[~]` trong module |")
    lines.append("| FE pages | Số `page.tsx` thực tế (không = số UC) |")
    lines.append("| Rủi ro | Ghi chú rà soát 06/08/2026 (stub / API chết / mỏng) |")
    lines.append("")
    lines.append("## A. Tổng hợp theo module")
    lines.append("")
    lines.append(
        "| Module | Tổng | [x] | [~] | [ ] | % | Must [x] | Must còn | Should còn | Khác còn | Avg % | FE pages | Rủi ro |"
    )
    lines.append("| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |")

    tot_all = tot_x = tot_t = tot_z = 0
    tot_must_x = tot_must_l = tot_should_l = tot_other_l = 0
    tot_fe = 0
    avg_acc: list[float] = []

    for mod, rows in by_mod.items():
        n = len(rows)
        x = sum(1 for r in rows if progress.get(r["id"], {}).get("done"))
        t = sum(
            1
            for r in rows
            if not progress.get(r["id"], {}).get("done")
            and progress.get(r["id"], {}).get("partial")
        )
        z = n - x - t
        p = round(100 * x / n, 1) if n else 0
        must_x = sum(
            1 for r in rows if r["prio"] == "Must" and progress.get(r["id"], {}).get("done")
        )
        must_l = sum(
            1
            for r in rows
            if r["prio"] == "Must" and not progress.get(r["id"], {}).get("done")
        )
        should_l = sum(
            1
            for r in rows
            if r["prio"] == "Should" and not progress.get(r["id"], {}).get("done")
        )
        other_l = sum(
            1
            for r in rows
            if r["prio"] in ("Could", "Won't") and not progress.get(r["id"], {}).get("done")
        )
        avg = _avg_pct(rows, progress)
        fe = FE_PAGE_COUNT.get(mod, 0)
        risk = MODULE_RISK.get(mod, "")
        lines.append(
            f"| {mod} | {n} | {x} | {t} | {z} | {p}% | {must_x} | {must_l} | {should_l} | {other_l} | {avg} | {fe} | {risk} |"
        )
        tot_all += n
        tot_x += x
        tot_t += t
        tot_z += z
        tot_must_x += must_x
        tot_must_l += must_l
        tot_should_l += should_l
        tot_other_l += other_l
        tot_fe += fe
        if avg:
            avg_acc.append(avg)

    tot_p = round(100 * tot_x / tot_all, 1) if tot_all else 0
    tot_avg = round(sum(avg_acc) / len(avg_acc), 1) if avg_acc else 0
    formatted_tot_all = "{:,}".format(tot_all).replace(",", ".")
    lines.append(
        f"| **TỔNG** | **{formatted_tot_all}** | **{tot_x}** | **{tot_t}** | **{tot_z}** | "
        f"**{tot_p}%** | **{tot_must_x}** | **{tot_must_l}** | **{tot_should_l}** | **{tot_other_l}** | "
        f"**{tot_avg}** | **{tot_fe}** | Xem [`Rà xoát UC.md`](./Rà%20xoát%20UC.md) |"
    )

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
    lines.append("| 04/08/2026 | Sinh checklist từ catalog (1092 UC); seed tiến độ M1 Day-1 SYS/HRM/WF |")
    lines.append(
        "| 05–06/08/2026 | Cap-2 lần lượt (INV FEFO/hold, PJM/FSM/LOG, HRM/WF…) · tiến độ máy trước claim 100% |"
    )
    lines.append(
        f"| {today} | **Hiệu chỉnh sau rà soát:** khôi phục `uc_progress` (bỏ đánh dấu 1092/1092 giả); "
        f"bảng A thêm cột `[x]`/`[~]`/`[ ]`/Must/Should/Avg%/FE pages/Rủi ro — "
        f"xem [`Rà xoát UC.md`](./Rà%20xoát%20UC.md) |"
    )
    lines.append(
        f"| {today} | Cap-2 CRM marketing/promo **wired**: `/crm/campaigns`+`/crm/promotions` · "
        f"UC `016,019,023,026,029,031,032–035,037` (rewire thật) |"
    )
    lines.append(
        f"| {today} | Cap-2 POS BOM+alerts **wired**: PaySale→INV Issue · stock-alerts · "
        f"UC `054,055` (rewire thật) |"
    )
    lines.append(
        f"| {today} | Cap-2 CRM sync POS + BC voucher + POS đóng ca→FIN: "
        f"UC `036,038,059` · BE 15 InMemory + FE helpers node:test |"
    )
    lines.append(
        f"| {today} | Cap-2 báo cáo POS: top SP · so sánh điểm bán · cost variance BOM vs INV: "
        f"UC `065,066,067` · BE 11 InMemory + FE 14 node:test |"
    )
    lines.append(
        f"| {today} | Cap-2 vận hành chuỗi POS: chain-live vs target + target DT store (migration): "
        f"UC `069,072` · BE 4 InMemory + FE 7 node:test |"
    )
    lines.append(
        f"| {today} | **Hoàn thiện UC dang dở PUR:** đẩy GRN→INV thật + HĐ→FIN AP thật (idempotent) + xuất PO CSV: "
        f"UC `033,037,043` 75→90 · BE 10 InMemory + FE 16 node:test |"
    )
    lines.append(
        f"| {today} | **Hoàn thiện UC dang dở CRM:** giữ tồn INV reservation thật (ATP) + đẩy đơn→LOG lệnh giao thật: "
        f"UC `082,088` 75→90 · BE 10 InMemory + FE 14 node:test |"
    )
    lines.append(
        f"| {today} | **Hoàn thiện UC dang dở POS:** sync catalog INV→POS thật + hóa đơn text + báo cáo ca thật: "
        f"UC `015,037,048` 75→90 · BE 8 InMemory + FE 7 node:test |"
    )
    lines.append(
        f"| {today} | **Hoàn thiện UC dang dở AST:** đẩy BT khấu hao → FIN JE thật (Posted cân Nợ/Có, auto-resolve TK/kỳ): "
        f"UC `012` 80→90 · BE 8 InMemory + FE 8 node:test |"
    )
    lines.append(
        f"| {today} | **Hoàn thiện UC dang dở MFG/FIN/CRM:** JE WIP→TP thật + BT Auto (filter Source) + báo giá text/Email: "
        f"UC `MFG_031,FIN_015,CRM_074` →90 · BE 13 InMemory + FE 9 node:test |"
    )
    lines.append(
        f"| {today} | **Hoàn thiện UC dang dở FIN/CRM/HRM:** JE thu·NH·AR·AP luôn thật + auto-intake dedup + sync máy chi tiết: "
        f"UC `FIN_019/025/030/039,CRM_050,HRM_118` →90–95 · BE 7 InMemory + FE 6 node:test · **{done_n}/{total}** ({pct_all}%) |"
    )
    lines.append(
        f"| {today} | **Hoàn thiện UC dang dở BI:** refresh nguồn module thật + widget DT/LN live FIN + chạy BC filter + tải CSV/text: "
        f"UC `002,008,014,016` →95 · BE 10 InMemory + FE 8 node:test · **{done_n}/{total}** ({pct_all}%) |"
    )
    lines.append(
        f"| {today} | **Hoàn thiện UC dang dở SYS:** Email/SMS stub (template+IntegrationCallLog+outbox) + forgot OTP + invite user: "
        f"UC `060,061,004,019` →90–95 · BE 10 InMemory + FE 7 node:test · **{done_n}/{total}** ({pct_all}%) |"
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
    print(f"Stub: {LEGACY_OUT_PATH.name} -> {OUT_PATH.name}")
    print(f"Progress: {PROGRESS_PATH.name}")


if __name__ == "__main__":
    main()
