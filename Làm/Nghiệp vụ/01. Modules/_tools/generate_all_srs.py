#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Sinh 16 SRS module chuyên nghiệp từ TREE + META.
"""
from __future__ import annotations

import sys
from datetime import date
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]  # 01. Modules
DATA = Path(__file__).resolve().parents[2] / "00. Tổng quan"
sys.path.insert(0, str(DATA))
sys.path.insert(0, str(Path(__file__).resolve().parent))

from cay_chuc_nang_data import TREE  # noqa: E402
from srs_module_meta import FOLDERS, META  # noqa: E402
from srs_module_meta_rest import REST  # noqa: E402

META.update(REST)

TODAY = date.today().strftime("%d/%m/%Y")

PRIORITY_MOSCOW = {
    "Bắt buộc": "Must",
    "Cao": "Should",
    "Trung bình": "Could",
    "Thấp": "Won't / Later",
}


def esc(s: str) -> str:
    return s.replace("|", "\\|")


def infer_actor(meta: dict, nhom_code: str) -> str:
    m = meta.get("default_actors_by_group") or {}
    return m.get(nhom_code) or (meta["actors"][0][0] if meta.get("actors") else "Người dùng")


def gen_steps(ten: str, mota: str) -> list[str]:
    t = ten.lower()
    if any(k in t for k in ["đăng nhập", "login"]):
        return [
            "Người dùng mở màn hình đăng nhập",
            "Nhập thông tin xác thực theo phương thức được cấu hình",
            "Hệ thống kiểm tra credential / policy / trạng thái tài khoản",
            "Cấp phiên làm việc và điều hướng trang chủ theo quyền",
        ]
    if any(k in t for k in ["duyệt", "phê duyệt", "từ chối"]):
        return [
            "Người duyệt mở chứng từ từ hộp chờ hoặc liên kết thông báo",
            "Xem nội dung, lịch sử và ràng buộc nghiệp vụ",
            "Chọn Duyệt / Từ chối / Trả bổ sung kèm lý do nếu cần",
            "Hệ thống cập nhật trạng thái và phát sự kiện cho module nguồn",
        ]
    if any(k in t for k in ["báo cáo", "dashboard", "xuất"]):
        return [
            "Người dùng chọn báo cáo/dashboard và bộ lọc",
            "Hệ thống kiểm tra quyền + data scope",
            "Truy vấn dữ liệu và hiển thị kết quả",
            "Người dùng xem chi tiết hoặc xuất Excel/PDF (nếu có quyền)",
        ]
    if any(k in t for k in ["cấu hình", "khai báo", "danh mục", "tạo "]):
        return [
            "Người dùng mở chức năng tương ứng trong module",
            "Nhập/chọn các trường bắt buộc theo form",
            "Hệ thống validate dữ liệu và ràng buộc duy nhất/tham chiếu",
            "Lưu bản ghi; ghi audit; hiển thị kết quả thành công",
        ]
    if any(k in t for k in ["đồng bộ", "đẩy", "post", "kết nối"]):
        return [
            "Kích hoạt đồng bộ thủ công hoặc theo sự kiện/job",
            "Hệ thống lấy dữ liệu nguồn và ánh xạ sang đích",
            "Ghi nhận kết quả/ thành công/ lỗi có thể retry",
            "Cập nhật trạng thái đồng bộ trên bản ghi liên quan",
        ]
    return [
        f"Người dùng khởi tạo thao tác: {ten}",
        f"Hệ thống kiểm tra quyền, license module và tiền điều kiện ({mota or 'theo rule module'})",
        "Thực hiện xử lý nghiệp vụ / cập nhật dữ liệu",
        "Ghi nhận kết quả, thông báo (nếu có) và audit trail",
    ]


def render_srs(code: str) -> str:
    meta = META[code]
    tree = TREE[code]
    lines: list[str] = []
    a = lines.append

    total_groups = len(tree)
    total_fn = sum(len(g[2]) for g in tree)

    a(f"# {meta['ma_tai_lieu']} — {meta['ten']}")
    a("")
    a("> Tài liệu đặc tả yêu cầu phần mềm (Software Requirements Specification) cho module ERP bán độc lập.")
    a("> Trạng thái: **Đề xuất / chờ duyệt nghiệp vụ**. Không gắn khách hàng hay ngành cụ thể.")
    a("")
    a("---")
    a("")
    a("## 0. Thông tin tài liệu & lịch sử thay đổi")
    a("")
    a("| Thuộc tính | Giá trị |")
    a("|---|---|")
    a(f"| Mã tài liệu | `{meta['ma_tai_lieu']}` |")
    a(f"| Module | `{code}` — {meta['ten']} |")
    a(f"| Phiên bản | {meta['phien_ban']} |")
    a(f"| Ngày lập | {TODAY} |")
    a("| Ngôn ngữ | Tiếng Việt |")
    a("| Phân loại | Nghiệp vụ / BA |")
    a(f"| Lớp sản phẩm | {meta['lop']} |")
    a(f"| Bán riêng | {meta['ban_rieng']} |")
    a(f"| Phụ thuộc bắt buộc | {', '.join(meta['phu_thuoc']) if meta['phu_thuoc'] else '—'} |")
    a(f"| Khuyến nghị kèm | {', '.join(meta['khuyen_nghi_kem']) if meta['khuyen_nghi_kem'] else '—'} |")
    a(f"| Số nhóm chức năng | {total_groups} |")
    a(f"| Số use case / chức năng | {total_fn} |")
    a("")
    a("| Phiên bản | Ngày | Người thực hiện | Mô tả | Trạng thái |")
    a("|---|---|---|---|---|")
    a(f"| 1.0 | {TODAY} | BA / Solution | Sinh SRS từ danh mục chức năng generic v3 + meta nghiệp vụ | Chờ duyệt |")
    a("")
    a("---")
    a("")
    a("## 1. Giới thiệu")
    a("")
    a("### 1.1. Mục đích tài liệu")
    a(
        "Tài liệu này mô tả đầy đủ yêu cầu nghiệp vụ và yêu cầu hệ thống của module "
        f"**{meta['ten']}**, làm cơ sở để thống nhất phạm vi, ước lượng, thiết kế và nghiệm thu "
        "trước khi triển khai cấu trúc source code."
    )
    a("")
    a("### 1.2. Tóm tắt module")
    a(meta["tom_tat"])
    a("")
    a("### 1.3. Mục tiêu nghiệp vụ")
    for i, t in enumerate(meta["muc_tieu"], 1):
        a(f"{i}. {t}")
    a("")
    a("### 1.4. Đối tượng đọc")
    a("- Chủ sản phẩm / Ban giám đốc dự án")
    a("- Business Analyst, Solution Architect")
    a("- Trưởng nhóm Dev/QA")
    a("- Đội triển khai & Presales (đóng gói bán module)")
    a("")
    a("---")
    a("")
    a("## 2. Phạm vi")
    a("")
    a("### 2.1. In Scope")
    for x in meta["in_scope"]:
        a(f"- {x}")
    a("")
    a("### 2.2. Out of Scope")
    for x in meta["out_scope"]:
        a(f"- {x}")
    a("")
    a("### 2.3. Nguyên tắc đóng gói bán")
    a(f"- **Bán riêng:** {meta['ban_rieng']}")
    if meta["phu_thuoc"]:
        a(f"- **Phụ thuộc bắt buộc:** {', '.join(f'`{x}`' for x in meta['phu_thuoc'])}.")
    else:
        a("- **Phụ thuộc bắt buộc:** không (module nền).")
    if meta["khuyen_nghi_kem"]:
        a(f"- **Khuyến nghị kèm** để có giá trị E2E: {', '.join(meta['khuyen_nghi_kem'])}.")
    a("- Tính năng ngành (F&B, sản xuất rời rạc, phân phối…) cấu hình bằng template khi triển khai, không hard-code vào SRS gốc.")
    a("")
    a("---")
    a("")
    a("## 3. Tác nhân & stakeholder")
    a("")
    a("| Tác nhân | Trách nhiệm chính |")
    a("|---|---|")
    for name, role in meta["actors"]:
        a(f"| {esc(name)} | {esc(role)} |")
    a("")
    a("---")
    a("")
    a("## 4. Thuật ngữ & viết tắt")
    a("")
    a("| Thuật ngữ | Định nghĩa |")
    a("|---|---|")
    for term, meaning in meta["terms"]:
        a(f"| {esc(term)} | {esc(meaning)} |")
    a("| UC | Use Case / chức năng nguyên tử trong catalog |")
    a("| MoSCoW | Must / Should / Could / Won't (ưu tiên) |")
    a("| Data scope | Phạm vi dữ liệu theo tổ chức/kho/… do SYS kiểm soát |")
    a("")
    a("---")
    a("")
    a("## 5. Ngữ cảnh module & phụ thuộc")
    a("")
    a("### 5.1. Vị trí trong kiến trúc sản phẩm")
    a(
        f"Module `{code}` thuộc lớp **{meta['lop']}**. Mọi truy cập đi qua lớp nền `SYS` "
        "(xác thực, RBAC, license, audit, file, thông báo)."
    )
    a("")
    a("### 5.2. Phụ thuộc & tích hợp")
    a("")
    a("| Hướng | Hệ thống / Module | Nội dung |")
    a("|---|---|---|")
    for row in meta["integrations"]:
        if len(row) == 2:
            a(f"| Tích hợp | {esc(row[0])} | {esc(row[1])} |")
        else:
            a(f"| {esc(row[0])} | {esc(row[1])} | {esc(row[2]) if len(row)>2 else ''} |")
    a("")
    a("### 5.3. Ràng buộc license")
    a(f"- API/UI của `{code}` chỉ mở khi license module active.")
    a("- Dataset BI liên quan module chỉ mở khi vừa có license `BI` vừa có license module nguồn.")
    a("")
    a("---")
    a("")
    a("## 6. Catalog chức năng (Module → Nhóm → UC)")
    a("")
    a(f"**Tổng hợp:** {total_groups} nhóm | {total_fn} chức năng/use case.")
    a("")
    a("| STT | Mã nhóm | Nhóm chức năng | Số UC |")
    a("|---:|---|---|---:|")
    stt = 1
    for nhom_code, ten_nhom, funcs in tree:
        a(f"| {stt} | `{code}-{nhom_code}` | {esc(ten_nhom)} | {len(funcs)} |")
        stt += 1
    a("")
    a("<details>")
    a("<summary>Bảng đầy đủ mã UC (bấm để mở)</summary>")
    a("")
    a("| Mã UC | Nhóm | Tên chức năng | Ưu tiên | MoSCoW |")
    a("|---|---|---|---|---|")
    uc_i = 1
    for nhom_code, ten_nhom, funcs in tree:
        for ten, mota, uu in funcs:
            a(
                f"| `UC_{code}_{uc_i:03d}` | {esc(ten_nhom)} | {esc(ten)} | {uu} | "
                f"{PRIORITY_MOSCOW.get(uu, 'Could')} |"
            )
            uc_i += 1
    a("")
    a("</details>")
    a("")
    a("---")
    a("")
    a("## 7. Đặc tả chức năng theo nhóm")
    a("")
    a(
        "Mỗi UC bên dưới gồm: mô tả, tác nhân, tiền/hậu điều kiện, luồng chính, "
        "quy tắc, tiêu chí chấp nhận và ưu tiên. Đây là mức đặc tả BA để chốt phạm vi; "
        "chi tiết UI/API sẽ bổ sung ở giai đoạn thiết kế."
    )
    a("")

    uc_i = 1
    for nhom_code, ten_nhom, funcs in tree:
        a(f"### 7.{int(nhom_code)}. {ten_nhom} (`{code}-{nhom_code}`)")
        a("")
        a(
            f"Nhóm này gồm **{len(funcs)}** chức năng. "
            f"Tác nhân mặc định: **{infer_actor(meta, nhom_code)}**."
        )
        a("")
        for idx, (ten, mota, uu) in enumerate(funcs, 1):
            uc = f"UC_{code}_{uc_i:03d}"
            actor = infer_actor(meta, nhom_code)
            steps = gen_steps(ten, mota)
            a(f"#### {uc} — {ten}")
            a("")
            a(f"- **Mô tả:** {mota or ten}")
            a(f"- **Tác nhân chính:** {actor}")
            a(f"- **Ưu tiên danh mục:** {uu} → **MoSCoW:** {PRIORITY_MOSCOW.get(uu, 'Could')}")
            a("- **Tiền điều kiện:**")
            a(f"  - User đã đăng nhập và có permission tương ứng trong `{code}`.")
            a(f"  - License module `{code}` đang hiệu lực (trừ khi là chức năng nền SYS).")
            a("  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.")
            a("- **Luồng chính:**")
            for si, step in enumerate(steps, 1):
                a(f"  {si}. {step}")
            a("- **Luồng thay thế / ngoại lệ:**")
            a("  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.")
            a("  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.")
            a("  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.")
            a("- **Hậu điều kiện:**")
            a("  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.")
            a("  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.")
            a("- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.")
            a("- **Tiêu chí chấp nhận (AC):**")
            a(f"  - AC1: Thực hiện thành công thao tác “{ten}” với dữ liệu hợp lệ.")
            a("  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).")
            a("  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).")
            if uu == "Bắt buộc":
                a("  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.")
            a("")
            uc_i += 1

    a("---")
    a("")
    a("## 8. Workflow end-to-end")
    a("")
    for wf in meta["workflows"]:
        a(f"### {wf['ma']} — {wf['ten']}")
        a("")
        a(f"**Mục tiêu:** {wf['muc_tieu']}")
        a("")
        a("| Bước | Mô tả |")
        a("|---:|---|")
        for i, b in enumerate(wf["buoc"], 1):
            a(f"| {i} | {esc(b)} |")
        a("")
        a("**Điều kiện hoàn tất:** Các bước cốt lõi hoàn thành; trạng thái nghiệp vụ cuối nhất quán; có audit/trace.")
        a("")

    a("---")
    a("")
    a("## 9. Mô hình dữ liệu domain (logic)")
    a("")
    a("> Mức conceptual — chưa phải thiết kế CSDL vật lý.")
    a("")
    a("| Thực thể | Vai trò |")
    a("|---|---|")
    for ent, role in meta["entities"]:
        a(f"| `{esc(ent)}` | {esc(role)} |")
    a("")
    a("### 9.1. Xuất xứ & kiểm soát dữ liệu")
    a("- Master dùng chung (KH, SP, chi nhánh…) tham chiếu từ module sở hữu / SYS, không nhân bản lệch.")
    a("- Chứng từ nghiệp vụ có trạng thái vòng đời rõ ràng (Draft → Submitted → Approved → Posted/Closed…).")
    a("- Soft-delete hoặc trạng thái ngưng dùng là mặc định; hạn chế xóa cứng.")
    a("")
    a("---")
    a("")
    a("## 10. Quy tắc nghiệp vụ tổng hợp")
    a("")
    for br in meta["business_rules"]:
        a(f"- {br}")
    a(f"- BR-{code}-GEN-01: Mọi thao tác thay đổi dữ liệu phải thuộc data scope của user.")
    a(f"- BR-{code}-GEN-02: Mọi chứng từ có mã duy nhất theo rule Sequence của SYS.")
    a(f"- BR-{code}-GEN-03: Thao tác sau khi khóa kỳ/chốt sổ (nếu có) phải đi đường điều chỉnh có kiểm soát.")
    a("")
    a("---")
    a("")
    a("## 11. Yêu cầu phi chức năng (NFR)")
    a("")
    a("| Nhóm | Yêu cầu |")
    a("|---|---|")
    for k, v in meta["nfr"]:
        a(f"| {esc(k)} | {esc(v)} |")
    a("| Usability | Form có validate rõ; bảng có lọc/phân trang; hỗ trợ tiếng Việt |")
    a("| Reliability | Không mất chứng từ đã post; giao dịch quan trọng atomic |")
    a("| Maintainability | Permission và cấu hình không hard-code trong source nghiệp vụ |")
    a("| Observability | Có log ứng dụng + audit nghiệp vụ tách bạch |")
    a("")
    a("---")
    a("")
    a("## 12. Tích hợp & sự kiện")
    a("")
    a("### 12.1. Ma trận tích hợp")
    a("")
    a("| Thành phần | Mô tả |")
    a("|---|---|")
    for row in meta["integrations"]:
        a(f"| {esc(row[0])} | {esc(row[1])} |")
    a("")
    a("### 12.2. Sự kiện (logical)")
    a(f"- `{code}.EntityCreated` / `{code}.EntityUpdated` / `{code}.EntityStatusChanged`")
    a(f"- `{code}.DocumentSubmitted` / `{code}.DocumentApproved` / `{code}.DocumentPosted`")
    a("- Mapping cụ thể API/topic sẽ định nghĩa ở tài liệu Interface Spec sau khi chốt SRS.")
    a("")
    a("---")
    a("")
    a("## 13. Phân quyền & bảo mật")
    a("")
    a("### 13.1. Permission catalog (đề xuất)")
    a("")
    for p in meta["permissions"]:
        a(f"- `{p}`")
    a("")
    a("### 13.2. Nguyên tắc")
    a("- Deny by default; chỉ mở theo role.")
    a("- Data scope theo chi nhánh/kho/đơn vị do SYS quyết định.")
    a("- Field-level security cho dữ liệu nhạy cảm (lương, công nợ chi tiết, giá vốn…) khi áp dụng.")
    a("- Mọi thay đổi phân quyền và thao tác critical ghi audit.")
    a("")
    a("---")
    a("")
    a("## 14. Báo cáo & KPI")
    a("")
    a("| KPI / Báo cáo | Mục đích |")
    a("|---|---|")
    for k in meta["kpis"]:
        a(f"| {esc(k)} | Giám sát vận hành module `{code}` |")
    a("")
    a("Báo cáo chi tiết vận hành nằm trong từng nhóm “Báo cáo…” của Mục 7; tổng hợp điều hành nằm trên module `BI` khi khách mua thêm.")
    a("")
    a("---")
    a("")
    a("## 15. Giả định, rủi ro & câu hỏi mở")
    a("")
    a("### 15.1. Giả định")
    for x in meta["assumptions"]:
        a(f"- {x}")
    a("")
    a("### 15.2. Câu hỏi mở cần chốt")
    for x in meta["open_questions"]:
        a(f"- {x}")
    a("")
    a("### 15.3. Rủi ro")
    a("- Phụ thuộc module khác chưa mua → một số workflow E2E chỉ chạy được một phần (cần nêu rõ khi bán gói).")
    a("- Cấu hình quá linh hoạt có thể làm tăng effort QA; cần bộ template mặc định.")
    a("- Chưa chốt chuẩn kế toán/thuế chi tiết có thể ảnh hưởng FIN và posting.")
    a("")
    a("---")
    a("")
    a("## 16. Tiêu chí nghiệm thu & truy vết")
    a("")
    a("### 16.1. Điều kiện nghiệm thu module")
    a(f"1. 100% UC ưu tiên **Bắt buộc (Must)** của `{code}` pass UAT.")
    a("2. Các workflow E2E ở Mục 8 chạy thành công trên dữ liệu mẫu.")
    a("3. Phân quyền & data scope được kiểm thử với ít nhất 3 role.")
    a("4. Audit log ghi nhận các thao tác critical.")
    a("5. Tích hợp với `SYS` và các phụ thuộc bắt buộc hoạt động ổn định.")
    a("6. Tài liệu hướng dẫn cấu hình template mặc định đi kèm.")
    a("")
    a("### 16.2. Truy vết")
    a("| Artifact | Liên kết |")
    a("|---|---|")
    a("| Catalog chức năng | `../00. Tổng quan/cay_chuc_nang_data.py` |")
    a("| Excel tổng hợp | `../00. Tổng quan/Danh_muc_Module_Chuc_nang_ERP_v3.xlsx` |")
    a("| Chuẩn viết SRS | `../00_CHUAN_VIET_SRS.md` |")
    a(f"| Use case IDs | `UC_{code}_001` … `UC_{code}_{total_fn:03d}` |")
    a("")
    a("---")
    a("")
    a(f"*Hết tài liệu {meta['ma_tai_lieu']}.*")
    a("")
    return "\n".join(lines)


def main():
    order = [
        "SYS", "HRM", "LMS", "CRM", "POS", "PUR", "INV", "LOG",
        "MFG", "FSM", "PJM", "FIN", "AST", "WF", "BI", "PRT",
    ]
    missing = [c for c in order if c not in META or c not in TREE or c not in FOLDERS]
    if missing:
        raise SystemExit(f"Missing meta/tree/folder for: {missing}")

    index_lines = [
        "# Danh mục SRS theo Module",
        "",
        f"Ngày sinh: {TODAY}",
        "",
        "| STT | Module | Tài liệu | Nhóm | UC | Thư mục |",
        "|---:|---|---|---:|---:|---|",
    ]

    for i, code in enumerate(order, 1):
        folder = ROOT / FOLDERS[code]
        folder.mkdir(parents=True, exist_ok=True)
        content = render_srs(code)
        out = folder / f"SRS_{code}_v1.0.md"
        out.write_text(content, encoding="utf-8")
        groups = len(TREE[code])
        funcs = sum(len(g[2]) for g in TREE[code])
        index_lines.append(
            f"| {i} | `{code}` | [SRS_{code}_v1.0.md](./{FOLDERS[code]}/SRS_{code}_v1.0.md) | {groups} | {funcs} | `{FOLDERS[code]}` |"
        )
        print(f"OK {code}: {out} ({groups} groups, {funcs} UC, {len(content.splitlines())} lines)")

    index_lines += [
        "",
        "## Ghi chú",
        "- Chuẩn viết: [00_CHUAN_VIET_SRS.md](./00_CHUAN_VIET_SRS.md)",
        "- Nguồn chức năng: generic v3 (không cá nhân hóa theo 1 khách).",
        "- Trạng thái chung: chờ duyệt nghiệp vụ trước khi làm cấu trúc source.",
        "",
    ]
    (ROOT / "README.md").write_text("\n".join(index_lines), encoding="utf-8")
    print("Wrote README.md index")


if __name__ == "__main__":
    main()
