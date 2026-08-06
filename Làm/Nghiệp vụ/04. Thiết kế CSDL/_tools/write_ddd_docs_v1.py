#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Sinh bộ Database Design Document (DDD) v1.0 — MD + Word chuyên nghiệp."""
from __future__ import annotations

import sys
from datetime import date
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
MODULES_TOOLS = Path(__file__).resolve().parents[2] / "01. Modules" / "_tools"
sys.path.insert(0, str(MODULES_TOOLS))

from build_srs_docx_pro import build, extract_meta_from_md  # noqa: E402

TODAY = date.today().strftime("%d/%m/%Y")


def header(doc_code: str, title: str, subtitle: str) -> list[str]:
    return [
        f"# {doc_code} — {title}",
        "",
        f"> **{subtitle}**",
        "> *Database Design Document (DDD)* — ERP bán theo module.",
        f"> Phiên bản **1.0** · Ngày {TODAY} · Trạng thái: **Chờ duyệt Solution / DBA**.",
        "> Mức thiết kế logic + hướng vật lý. Generic — không gắn khách/ngành cứng.",
        "",
        "---",
        "",
        "## 0. Thông tin tài liệu",
        "",
        "| Thuộc tính | Giá trị |",
        "|---|---|",
        f"| Mã tài liệu | `{doc_code}` |",
        f"| Tên | {title} |",
        "| Phiên bản | 1.0 |",
        f"| Ngày | {TODAY} |",
        "| Phân loại | Thiết kế CSDL (Solution / DBA) |",
        "| Định dạng bàn giao | Microsoft Word (`.docx`) |",
        "| Đầu vào | SRS module v1.1 · INT v1.0 |",
        "",
        "| Ver | Ngày | Mô tả | Trạng thái |",
        "|---|---|---|---|",
        f"| 1.0 | {TODAY} | Khởi tạo bộ Database Design Document | Chờ duyệt |",
        "",
        "---",
        "",
    ]


def write(path: Path, lines: list[str]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")
    print(f"Wrote {path.name} ({len(lines)} lines)")


def ent_table(rows: list[tuple[str, str, str, str]]) -> list[str]:
    """rows: (bảng, mô tả, PK, quan hệ chính)"""
    lines = [
        "| Bảng / Thực thể | Mô tả | PK | Quan hệ / FK chính |",
        "|---|---|---|---|",
    ]
    for r in rows:
        lines.append(f"| `{r[0]}` | {r[1]} | `{r[2]}` | {r[3]} |")
    lines.append("")
    return lines


def col_table(rows: list[tuple[str, str, str, str]]) -> list[str]:
    """rows: (cột, kiểu, bắt buộc, mô tả)"""
    lines = [
        "| Cột | Kiểu gợi ý | NN | Mô tả |",
        "|---|---|---|---|",
    ]
    for r in rows:
        lines.append(f"| `{r[0]}` | {r[1]} | {r[2]} | {r[3]} |")
    lines.append("")
    return lines


# =============================================================================
# DDD-01
# =============================================================================
def doc_01() -> list[str]:
    a: list[str] = []
    a += header(
        "DDD-01-v1.0",
        "Tổng quan & chuẩn thiết kế cơ sở dữ liệu",
        "Database Architecture & Design Standards",
    )
    a += [
        "## 1. Giới thiệu",
        "",
        "### 1.1. Mục đích",
        "Thiết lập **kiến trúc dữ liệu thống nhất** cho ERP đa module: quy ước schema, multi-tenant, khóa, audit, soft-delete — làm chuẩn bắt buộc trước khi viết migration.",
        "",
        "### 1.2. Phạm vi",
        "- Mô hình logic theo schema module.",
        "- Quy ước đặt tên, kiểu dữ liệu, ràng buộc chung.",
        "- Hướng dẫn vật lý Phase 1 (chưa thay thế script SQL chi tiết từng môi trường).",
        "",
        "### 1.3. Ngoài phạm vi",
        "- Tối ưu query cụ thể từng báo cáo BI.",
        "- Thiết kế kho dữ liệu analytic riêng (data warehouse) — thuộc BI nâng cao.",
        "",
        "---",
        "",
        "## 2. Kiến trúc CSDL Phase 1",
        "",
        "```text",
        "  ┌─────────────────────────────────────────────┐",
        "  │         Database (1 cluster / tenant)        │",
        "  │  ┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐   │",
        "  │  │ sys │ │ hrm │ │ crm │ │ inv │ │ fin │…  │",
        "  │  └─────┘ └─────┘ └─────┘ └─────┘ └─────┘   │",
        "  │         FK / Ref ID xuyên schema             │",
        "  └─────────────────────────────────────────────┘",
        "```",
        "",
        "| Quyết định | Lựa chọn Phase 1 | Ghi chú |",
        "|---|---|---|",
        "| Phân tách module | **Schema** theo mã module | `sys`, `hrm`, `crm`… |",
        "| Multi-tenant | Cột `tenant_id` trên hầu hết bảng | Có thể tách DB riêng tenant enterprise sau |",
        "| Engine gợi ý | PostgreSQL 15+ *(hoặc SQL Server tương đương)* | Chốt kỹ thuật khi vào Source |",
        "| Charset | UTF-8 | Tiếng Việt đầy đủ |",
        "| Time | Lưu UTC; hiển thị theo TZ tenant | `timestamptz` |",
        "",
        "### 2.1. Danh sách schema",
        "",
        "| Schema | Module |",
        "|---|---|",
        "| `sys` | SYS |",
        "| `hrm` | HRM |",
        "| `lms` | LMS |",
        "| `crm` | CRM |",
        "| `pos` | POS |",
        "| `pur` | PUR |",
        "| `inv` | INV |",
        "| `log` | LOG |",
        "| `mfg` | MFG |",
        "| `fsm` | FSM |",
        "| `pjm` | PJM |",
        "| `fin` | FIN |",
        "| `ast` | AST |",
        "| `wf` | WF |",
        "| `bi` | BI (metadata; dataset có thể read-only) |",
        "| `prt` | PRT |",
        "",
        "---",
        "",
        "## 3. Quy ước đặt tên",
        "",
        "| Đối tượng | Quy ước | Ví dụ |",
        "|---|---|---|",
        "| Schema | `snake` ngắn = mã module | `hrm` |",
        "| Bảng | `snake_case`, số ít hoặc danh từ nghiệp vụ | `employee`, `sales_order` |",
        "| Cột | `snake_case` | `full_name`, `ordered_at` |",
        "| PK | `{table}_id` hoặc `id` (UUID) | `employee_id` |",
        "| FK | `{ref}_id` | `customer_id`, `tenant_id` |",
        "| Unique | `uq_{table}_{cols}` | `uq_employee_code_tenant` |",
        "| Index | `ix_{table}_{cols}` | `ix_so_tenant_status` |",
        "| Check | `ck_{table}_{rule}` | `ck_qty_positive` |",
        "",
        "### 3.1. Khóa chính",
        "- Phase 1 khuyến nghị **UUID v7 / ULID** (hoặc UUID v4) cho PK phân tán & merge.",
        "- Mã nghiệp vụ (`code`) **không** dùng làm PK; có unique theo tenant.",
        "",
        "---",
        "",
        "## 4. Cột chuẩn (mọi bảng nghiệp vụ)",
        "",
    ]
    a += col_table(
        [
            ("id", "UUID", "YES", "Khóa chính"),
            ("tenant_id", "UUID", "YES", "FK → sys.tenant"),
            ("created_at", "timestamptz", "YES", "UTC"),
            ("created_by", "UUID", "NO", "FK → sys.app_user (nullable cho job)"),
            ("updated_at", "timestamptz", "YES", "UTC"),
            ("updated_by", "UUID", "NO", "User cập nhật cuối"),
            ("is_deleted", "boolean", "YES", "Mặc định false — soft delete"),
            ("deleted_at", "timestamptz", "NO", "Thời điểm xóa mềm"),
            ("row_version", "int/xid", "YES", "Optimistic concurrency"),
        ]
    )
    a += [
        "### 4.1. Cột chứng từ (document header)",
        "",
    ]
    a += col_table(
        [
            ("doc_no", "varchar(50)", "YES", "Số chứng từ theo sequence SYS"),
            ("doc_date", "date", "YES", "Ngày chứng từ"),
            ("status", "varchar(30)", "YES", "Vòng đời: Draft/Submitted/Approved/Posted/Cancelled…"),
            ("org_unit_id", "UUID", "NO", "Đơn vị/chi nhánh phát sinh"),
            ("currency_code", "char(3)", "NO", "ISO 4217"),
            ("remark", "text", "NO", "Ghi chú"),
            ("posted_at", "timestamptz", "NO", "Thời điểm post sổ/kho"),
            ("correlation_id", "UUID", "NO", "Trace liên module / event"),
        ]
    )
    a += [
        "---",
        "",
        "## 5. Multi-tenant & phân quyền (RBAC + data scope 4 tầng)",
        "",
        "### 5.1. Hai trục bắt buộc",
        "",
        "| Trục | Câu hỏi | Cơ chế |",
        "|---|---|---|",
        "| Functional RBAC | Được **làm gì**? | `User → user_role → role → role_permission → permission` |",
        "| Data scope | Được **thấy dữ liệu nào**? | `JobLevel.default_scope_type` (Own/Team/Department/All) + `user_data_scope` (chi nhánh/kho…) |",
        "",
        "### 5.2. Data scope 4 tầng (tham chiếu Digi `ScopeType`)",
        "",
        "| Giá trị | Ý nghĩa lọc dòng |",
        "|---|---|",
        "| `Own` | Chỉ bản ghi của chính user |",
        "| `Team` | Bản thân + user có `manager_user_id = current` |",
        "| `Department` | Phòng ban chính (+ kiêm nhiệm) và **cây con** `sys.department` |",
        "| `All` | Không lọc theo người/phòng; vẫn tôn trọng tenant |",
        "",
        "Thứ tự hiệu lực: nếu bất kỳ role active có `bypass_data_scope = true` → coi như `All`; ngược lại lấy `job_level.default_scope_type`. Phạm vi chi nhánh/kho/điểm bán bổ sung qua `sys.user_data_scope`.",
        "",
        "### 5.3. Thực thể tổ chức liên quan authz",
        "",
        "| Thực thể | Vai trò |",
        "|---|---|",
        "| `sys.org_unit` | Công ty / chi nhánh (pháp lý, kho thuộc CN…) |",
        "| `sys.department` | Phòng ban — trục scope `Department` |",
        "| `sys.job_level` | Cấp bậc — mang `default_scope_type` |",
        "| `hrm.job_title` | Chức danh nghiệp vụ (khác JobLevel); có thể gợi ý `job_level_id` |",
        "",
        "| Cơ chế | Thiết kế |",
        "|---|---|",
        "| Tenant isolation | Mọi query bắt buộc filter `tenant_id` (middleware + RLS tùy chọn) |",
        "| Field-level | `sys.field_permission` (Hidden/Masked/Read/Write) |",
        "| Menu | `sys.menu_item` lọc theo license module + `permission_code` |",
        "| Row Level Security | Khuyến nghị bật RLS PostgreSQL ở Phase 2 hoặc tenant nhạy cảm |",
        "| Cross-tenant | **Cấm** trừ Super Platform Admin (ngoài phạm vi app tenant) |",
        "",
        "---",
        "",
        "## 6. Trạng thái & máy trạng thái",
        "",
        "- Lưu `status` dạng mã ổn định (English snake/Pascal), không lưu nhãn UI.",
        "- Chuyển trạng thái ghi `status_history` (bảng riêng hoặc audit) với `from_status`, `to_status`, `changed_by`, `reason`.",
        "- Sau `Posted`/`Locked`: không update trực tiếp; dùng chứng từ điều chỉnh.",
        "",
        "---",
        "",
        "## 7. Tiền tệ, số lượng, làm tròn",
        "",
        "| Loại | Kiểu | Quy ước |",
        "|---|---|---|",
        "| Số lượng | `numeric(18,6)` | Làm tròn theo UoM |",
        "| Đơn giá / thành tiền | `numeric(18,4)` / `numeric(18,2)` | Tiền theo currency |",
        "| Tỷ giá | `numeric(18,8)` | Tại thời điểm chứng từ |",
        "| Thuế % | `numeric(9,4)` | |",
        "",
        "---",
        "",
        "## 8. File, audit, outbox",
        "",
        "| Kho | Schema.bảng | Ghi chú |",
        "|---|---|---|",
        "| File metadata | `sys.file_object` | Binary ở object storage |",
        "| Audit | `sys.audit_log` | before/after JSON |",
        "| Login | `sys.login_log` | |",
        "| Outbox | `sys.integration_outbox` | Đồng bộ event INT-03 |",
        "| Inbox | `sys.integration_inbox` | Idempotency consumer |",
        "",
        "---",
        "",
        "## 9. Quan hệ xuyên schema",
        "",
        "1. FK vật lý **khuyến nghị** khi cùng DB và module luôn đi kèm (ví dụ `inv` → `sys`).",
        "2. Với soft dependency: lưu **UUID tham chiếu** + không bắt buộc FK cứng (tránh gãy khi tắt module) — kiểm tra tồn tại ở tầng ứng dụng.",
        "3. Snapshot mã/tên trên dòng chứng từ để giữ lịch sử khi master đổi.",
        "",
        "---",
        "",
        "## 10. Ánh xạ tài liệu",
        "",
        "| Nội dung | Tài liệu |",
        "|---|---|",
        "| Schema SYS/WF | DDD-02 |",
        "| HRM…PRT thương mại | DDD-03 |",
        "| PUR…MFG | DDD-04 |",
        "| FIN…BI | DDD-05 |",
        "| Index, bảo mật, migration | DDD-06 |",
        "| Sự kiện | INT-03 |",
        "",
        "---",
        "",
        "*Hết DDD-01-v1.0.*",
        "",
    ]
    return a


# =============================================================================
# DDD-02 SYS + WF
# =============================================================================
def doc_02() -> list[str]:
    a: list[str] = []
    a += header(
        "DDD-02-v1.0",
        "Mô hình dữ liệu schema nền tảng (SYS, WF)",
        "Logical Data Model — Platform Schemas",
    )
    a += [
        "## 1. Giới thiệu",
        "",
        "Thiết kế các bảng cốt lõi schema **`sys`** và **`wf`** — nền cho mọi module nghiệp vụ.",
        "",
        "---",
        "",
        "## 2. Sơ đồ quan hệ logic (SYS)",
        "",
        "```text",
        " tenant ─┬─ org_unit (Company/Branch)",
        "         ├─ department (cây phòng; thuộc org_unit)",
        "         ├─ job_level (default_scope_type: Own|Team|Department|All)",
        "         ├─ app_user ─┬─ department_id / job_level_id / manager_user_id",
        "         │            ├─ user_department (kiêm nhiệm)",
        "         │            ├─ user_role ── role (bypass_data_scope)",
        "         │            │                 └── role_permission ── permission",
        "         │            ├─ user_data_scope (OrgUnit/Warehouse/Store…)",
        "         │            └─ session",
        "         ├─ field_permission (theo role + entity.field)",
        "         ├─ menu_item (license + permission_code)",
        "         ├─ license ── license_module",
        "         ├─ sequence_rule / setting / file_object",
        "         ├─ notification_* / audit_log / login_log",
        "         └─ api_key / webhook / integration_outbox|inbox",
        "```",
        "",
        "### 2.1. Mô hình phân quyền chuẩn (chốt)",
        "",
        "1. **Role + Permission** — quyền chức năng API/UI (`{module}.{resource}.{action}`).",
        "2. **Department + JobLevel** — tổ chức người và **data scope 4 tầng** (Own/Team/Department/All).",
        "3. **user_data_scope** — phạm vi theo chi nhánh / kho / cửa hàng / dự án (ERP đa điểm).",
        "4. **field_permission + menu_item** — bảo vệ trường nhạy cảm và menu.",
        "5. Khi bật HRM: đồng bộ `employee.department_id / job_level_id / manager` → `app_user` (authz vẫn chạy nếu chưa mua HRM).",
        "",
        "---",
        "",
        "## 3. Bảng schema `sys`",
        "",
        "### 3.1. Danh mục thực thể",
        "",
    ]
    a += ent_table(
        [
            ("tenant", "Không gian thuê bao", "id", "—"),
            ("org_unit", "Cây công ty/chi nhánh", "id", "FK tenant, parent_id"),
            ("department", "Cây phòng ban (scope Department)", "id", "FK tenant, org_unit, parent"),
            ("job_level", "Cấp bậc + default_scope_type", "id", "FK tenant"),
            ("app_user", "Tài khoản đăng nhập", "id", "FK tenant; dept/job_level/manager"),
            ("user_department", "Phòng kiêm nhiệm", "id", "FK user, department"),
            ("role", "Vai trò RBAC (+ bypass_data_scope)", "id", "FK tenant"),
            ("permission", "Catalog quyền chức năng", "id", "uq(code); module/resource/action"),
            ("role_permission", "Gán quyền cho role", "id", "FK role, permission"),
            ("user_role", "User ↔ Role (có hiệu lực/thu hồi)", "id", "FK user, role"),
            ("user_data_scope", "Phạm vi theo đối tượng (CN/kho…)", "id", "FK user + dimension/scope_id"),
            ("field_permission", "Quyền theo trường", "id", "FK role; entity.field"),
            ("menu_item", "Menu UI", "id", "permission_code + module"),
            ("license", "Hợp đồng license tenant", "id", "FK tenant"),
            ("license_module", "Module được bật", "id", "FK license; module_code"),
            ("sequence_rule", "Sinh số chứng từ", "id", "FK tenant; pattern"),
            ("setting", "Cấu hình key-value", "id", "FK tenant; key unique"),
            ("file_object", "Metadata file", "id", "FK tenant; storage_key"),
            ("notification_template", "Mẫu thông báo", "id", "FK tenant"),
            ("notification_log", "Lịch sử gửi", "id", "FK tenant"),
            ("audit_log", "Nhật ký thay đổi", "id", "FK tenant"),
            ("login_log", "Nhật ký đăng nhập", "id", "FK tenant, user"),
            ("api_key", "Khóa tích hợp", "id", "FK tenant"),
            ("webhook_subscription", "Webhook outbound", "id", "FK tenant"),
            ("integration_outbox", "Outbox event", "id", "FK tenant"),
            ("integration_inbox", "Inbox idempotent", "id", "FK tenant; uq event_id"),
            ("session", "Phiên đăng nhập", "id", "FK user"),
        ]
    )
    a += [
        "### 3.2. `sys.job_level` — cấp bậc & scope mặc định",
        "",
    ]
    a += col_table(
        [
            ("id", "UUID", "YES", "PK"),
            ("tenant_id", "UUID", "YES", "FK tenant"),
            ("code", "varchar(40)", "YES", "STAFF/MANAGER/DIRECTOR…"),
            ("name", "varchar(200)", "YES", "Tên hiển thị"),
            ("level_order", "int", "YES", "Thứ tự cấp"),
            ("default_scope_type", "varchar(20)", "YES", "Own|Team|Department|All"),
            ("is_active", "boolean", "YES", ""),
        ]
    )
    a += [
        "### 3.3. `sys.department`",
        "",
    ]
    a += col_table(
        [
            ("id", "UUID", "YES", "PK"),
            ("tenant_id", "UUID", "YES", "FK tenant"),
            ("code", "varchar(40)", "YES", "Unique theo tenant"),
            ("name", "varchar(200)", "YES", ""),
            ("parent_id", "UUID", "NO", "Cây phòng"),
            ("org_unit_id", "UUID", "YES", "Chi nhánh thuộc về"),
            ("manager_user_id", "UUID", "NO", "Trưởng phòng"),
            ("path", "varchar(500)", "YES", "Materialized path"),
            ("is_active", "boolean", "YES", ""),
        ]
    )
    a += [
        "### 3.4. `sys.app_user` — thuộc tính chính (authz)",
        "",
    ]
    a += col_table(
        [
            ("id", "UUID", "YES", "PK"),
            ("tenant_id", "UUID", "YES", "FK tenant"),
            ("username", "varchar(100)", "YES", "Unique theo tenant"),
            ("display_name", "varchar(200)", "NO", ""),
            ("email", "varchar(255)", "NO", "Unique theo tenant nếu có"),
            ("password_hash", "varchar(255)", "NO", "Null nếu SSO-only"),
            ("status", "varchar(20)", "YES", "Active/Locked/Disabled"),
            ("primary_org_unit_id", "UUID", "NO", "Chi nhánh chính"),
            ("department_id", "UUID", "NO", "Phòng ban chính → scope Department"),
            ("job_level_id", "UUID", "NO", "→ default_scope_type"),
            ("manager_user_id", "UUID", "NO", "→ scope Team"),
            ("employee_id", "UUID", "NO", "Ref mềm hrm.employee"),
            ("failed_login_count", "int", "YES", "Chống brute-force"),
        ]
    )
    a += [
        "### 3.5. `sys.role` / `sys.permission`",
        "",
    ]
    a += col_table(
        [
            ("role.code", "varchar(50)", "YES", "Unique theo tenant"),
            ("role.bypass_data_scope", "boolean", "YES", "true → All (super)"),
            ("role.is_system", "boolean", "YES", "Không xóa"),
            ("permission.code", "varchar(100)", "YES", "{module}.{resource}.{action}"),
            ("permission.module_code", "varchar(10)", "YES", "SYS/HRM/…"),
            ("permission.resource", "varchar(80)", "YES", "employee, leave…"),
            ("permission.action", "varchar(40)", "YES", "Create/Read/Update/Delete/Approve…"),
        ]
    )
    a += [
        "### 3.6. `sys.integration_outbox`",
        "",
    ]
    a += col_table(
        [
            ("id", "UUID", "YES", "PK"),
            ("tenant_id", "UUID", "YES", ""),
            ("event_id", "UUID", "YES", "Unique publish"),
            ("event_type", "varchar(120)", "YES", "INT-03"),
            ("aggregate_type", "varchar(80)", "NO", ""),
            ("aggregate_id", "UUID", "NO", ""),
            ("payload_json", "jsonb", "YES", "Envelope + payload"),
            ("status", "varchar(20)", "YES", "New/Published/Failed"),
            ("occurred_at", "timestamptz", "YES", ""),
            ("published_at", "timestamptz", "NO", ""),
            ("retry_count", "int", "YES", ""),
        ]
    )
    a += [
        "---",
        "",
        "## 4. Schema `wf` — phê duyệt",
        "",
        "### 4.1. Thực thể",
        "",
    ]
    a += ent_table(
        [
            ("wf_definition", "Định nghĩa quy trình", "id", "FK tenant; module/doc_type"),
            ("wf_definition_version", "Phiên bản quy trình", "id", "FK definition"),
            ("wf_node", "Bước duyệt", "id", "FK version"),
            ("wf_transition", "Chuyển bước", "id", "FK version"),
            ("wf_instance", "Phiên bản chạy", "id", "FK definition; doc ref"),
            ("wf_task", "Việc chờ duyệt", "id", "FK instance; assignee"),
            ("wf_task_action", "Hành động duyệt", "id", "FK task"),
            ("wf_delegation", "Ủy quyền", "id", "FK tenant; user"),
        ]
    )
    a += [
        "### 4.2. Liên kết chứng từ nguồn",
        "`wf_instance` lưu `source_module`, `source_doc_type`, `source_doc_id` (UUID) — **không** FK cứng sang mọi bảng chứng từ.",
        "",
        "---",
        "",
        "## 5. Ràng buộc & index gợi ý (SYS)",
        "",
        "| Bảng | Index / Unique |",
        "|---|---|",
        "| app_user | `uq(tenant_id, username)`; `ix(tenant_id, email)`; `ix(department_id)`; `ix(manager_user_id)` |",
        "| department | `uq(tenant_id, code)`; `ix(tenant_id, path)` |",
        "| job_level | `uq(tenant_id, code)` |",
        "| role | `uq(tenant_id, code)` |",
        "| permission | `uq(code)` |",
        "| role_permission | `uq(role_id, permission_id)` |",
        "| user_role | `uq(user_id, role_id)` where active; `ix(user_id, is_active)` |",
        "| user_department | `uq(user_id, department_id)` |",
        "| user_data_scope | `ix(user_id, dimension, scope_id)` |",
        "| field_permission | `uq(role_id, entity_type, field_name)` |",
        "| menu_item | `uq(tenant_id, code)` hoặc global catalog |",
        "| license_module | `uq(license_id, module_code)` |",
        "| sequence_rule | `uq(tenant_id, code)` |",
        "| integration_outbox | `uq(event_id)`; `ix(status, created_at)` |",
        "| integration_inbox | `uq(event_id, consumer)` |",
        "| audit_log | `ix(tenant_id, entity_type, entity_id, created_at)` |",
        "",
        "---",
        "",
        "## 6. Runtime gợi ý (không gắn stack)",
        "",
        "1. API check permission code (không nhét full permission vào JWT).",
        "2. Resolve effective scope: bypass role → else JobLevel → áp filter Own/Team/Department/All.",
        "3. AND thêm filter `user_data_scope` theo chứng từ (`org_unit_id`, `warehouse_id`…).",
        "4. `/me` trả roles, permission codes, effective_scope_type, accessible department ids.",
        "",
        "---",
        "",
        "## 7. Truy vết SRS",
        "Ánh xạ SYS-03 (UC_SYS_023…033) và nhóm Auth trong `SRS_SYS_v1.1`. Tham chiếu mẫu Digi: Role/Permission/Department/JobLevel + `ScopeType`.",
        "",
        "---",
        "",
        "*Hết DDD-02-v1.0.*",
        "",
    ]
    return a


# =============================================================================
# DDD-03 HRM LMS CRM POS PRT
# =============================================================================
def doc_03() -> list[str]:
    a: list[str] = []
    a += header(
        "DDD-03-v1.0",
        "Mô hình dữ liệu nhân sự & thương mại (HRM, LMS, CRM, POS, PRT)",
        "Logical Data Model — People & Commerce",
    )
    a += [
        "## 1. Giới thiệu",
        "",
        "Thiết kế schema **`hrm`**, **`lms`**, **`crm`**, **`pos`**, **`prt`** — vòng đời người và vòng đời bán hàng/khách hàng.",
        "",
        "---",
        "",
        "## 2. Schema `hrm`",
        "",
        "### 2.1. Sơ đồ logic",
        "",
        "```text",
        " job_title (→ sys.job_level) / employee_type",
        " department/job_level/manager sync ↔ sys.app_user",
        "          \\",
        "           employee ── contract ── contract_appendix",
        "              │",
        "              ├─ employment_status_history",
        "              ├─ shift_assignment ← shift_template",
        "              ├─ attendance_punch → timesheet / timesheet_line",
        "              ├─ leave_balance / leave_request",
        "              ├─ payroll_period → payslip / payslip_line",
        "              └─ offboarding_checklist",
        " recruitment_request → candidate → job_posting",
        "```",
        "",
        "### 2.2. Thực thể chính",
        "",
    ]
    a += ent_table(
        [
            ("employee", "Hồ sơ nhân sự", "id", "FK tenant; user_id; org_unit; department; job_level"),
            ("contract", "Hợp đồng LĐ", "id", "FK employee"),
            ("contract_appendix", "Phụ lục HĐ", "id", "FK contract"),
            ("employment_status_history", "Lịch sử trạng thái", "id", "FK employee"),
            ("recruitment_request", "Đề xuất tuyển", "id", "FK org/position"),
            ("candidate", "Ứng viên", "id", "FK request optional"),
            ("job_posting", "Tin tuyển", "id", "FK request"),
            ("onboarding_task", "Checklist nhận việc", "id", "FK employee"),
            ("shift_template", "Mẫu ca", "id", "FK tenant"),
            ("shift_assignment", "Xếp ca", "id", "FK employee, template"),
            ("attendance_punch", "Chấm công thô", "id", "FK employee"),
            ("timesheet", "Bảng công kỳ", "id", "FK tenant, period"),
            ("timesheet_line", "Dòng công NV", "id", "FK timesheet, employee"),
            ("leave_type", "Loại nghỉ", "id", "FK tenant"),
            ("leave_balance", "Quỹ phép", "id", "FK employee, leave_type"),
            ("leave_request", "Đơn nghỉ", "id", "FK employee"),
            ("payroll_period", "Kỳ lương", "id", "FK tenant"),
            ("payslip", "Phiếu lương", "id", "FK period, employee"),
            ("payslip_line", "Dòng lương", "id", "FK payslip"),
            ("transfer_order", "Điều động", "id", "FK employee"),
            ("offboarding_case", "Hồ sơ nghỉ việc", "id", "FK employee"),
        ]
    )
    a += [
        "### 2.3. `hrm.employee` — cột trọng yếu",
        "",
    ]
    a += col_table(
        [
            ("id", "UUID", "YES", "PK"),
            ("tenant_id", "UUID", "YES", ""),
            ("employee_code", "varchar(40)", "YES", "Unique theo tenant"),
            ("user_id", "UUID", "NO", "FK sys.app_user"),
            ("full_name", "varchar(200)", "YES", ""),
            ("dob", "date", "NO", ""),
            ("gender", "varchar(20)", "NO", ""),
            ("national_id_enc", "bytea/varchar", "NO", "Mã hóa/field security"),
            ("org_unit_id", "UUID", "YES", "Đơn vị chính"),
            ("job_title_id", "UUID", "NO", ""),
            ("employee_type_id", "UUID", "NO", ""),
            ("status", "varchar(30)", "YES", "Probation/Active/Terminated…"),
            ("hire_date", "date", "NO", ""),
            ("terminate_date", "date", "NO", ""),
        ]
    )
    a += [
        "### 2.4. Ràng buộc HRM",
        "- `employee_code` không tái sử dụng sau terminate (BR-HRM-01).",
        "- `payslip` chỉ tạo từ `timesheet` đã Locked (trừ điều chỉnh).",
        "- Dữ liệu lương: cột nhạy cảm — ACL field-level ở API; DB có thể mã hóa cột.",
        "",
        "---",
        "",
        "## 3. Schema `lms`",
        "",
    ]
    a += ent_table(
        [
            ("course", "Khóa học", "id", "FK tenant"),
            ("course_version", "Phiên bản nội dung", "id", "FK course"),
            ("lesson", "Bài học", "id", "FK course_version"),
            ("assessment", "Bài kiểm tra", "id", "FK course_version"),
            ("learning_path", "Lộ trình", "id", "FK tenant"),
            ("enrollment", "Ghi danh", "id", "FK course, learner"),
            ("learning_progress", "Tiến độ", "id", "FK enrollment"),
            ("assessment_attempt", "Lần thi", "id", "FK assessment, learner"),
            ("certificate", "Chứng chỉ", "id", "FK enrollment; → HRM event"),
            ("training_class", "Lớp offline", "id", "FK course"),
        ]
    )
    a += [
        "`learner` tham chiếu `hrm.employee_id` và/hoặc `sys.app_user_id`.",
        "",
        "---",
        "",
        "## 4. Schema `crm`",
        "",
        "### 4.1. Sơ đồ logic",
        "",
        "```text",
        " customer ── contact",
        "     │",
        "     ├─ lead → opportunity → quote → sales_order → sales_order_line",
        "     ├─ campaign / voucher",
        "     ├─ activity / visit",
        "     └─ conversation / case",
        "```",
        "",
        "### 4.2. Thực thể",
        "",
    ]
    a += ent_table(
        [
            ("customer", "Khách hàng", "id", "FK tenant"),
            ("contact", "Người liên hệ", "id", "FK customer"),
            ("lead", "Đầu mối", "id", "FK tenant"),
            ("opportunity", "Cơ hội", "id", "FK customer/lead"),
            ("quote", "Báo giá", "id", "FK customer"),
            ("quote_line", "Dòng báo giá", "id", "FK quote; item_id ref INV"),
            ("sales_order", "Đơn bán", "id", "FK customer"),
            ("sales_order_line", "Dòng đơn", "id", "FK sales_order; item_id"),
            ("campaign", "Chiến dịch", "id", "FK tenant"),
            ("voucher", "Mã KM", "id", "FK campaign optional"),
            ("activity", "Hoạt động CSKH/BH", "id", "FK customer"),
            ("sales_case", "Case/khiếu nại", "id", "FK customer"),
        ]
    )
    a += [
        "### 4.3. `crm.sales_order` — cột trọng yếu",
        "",
    ]
    a += col_table(
        [
            ("id", "UUID", "YES", "PK"),
            ("tenant_id", "UUID", "YES", ""),
            ("doc_no", "varchar(50)", "YES", "Sequence"),
            ("customer_id", "UUID", "YES", "FK customer"),
            ("status", "varchar(30)", "YES", "Draft…Confirmed…Closed"),
            ("order_date", "date", "YES", ""),
            ("currency_code", "char(3)", "YES", ""),
            ("total_amount", "numeric(18,2)", "YES", ""),
            ("warehouse_id", "UUID", "NO", "Ref inv.warehouse"),
            ("promised_date", "date", "NO", ""),
            ("correlation_id", "UUID", "NO", "E2E trace"),
        ]
    )
    a += [
        "---",
        "",
        "## 5. Schema `pos`",
        "",
    ]
    a += ent_table(
        [
            ("store", "Cửa hàng", "id", "FK org_unit/tenant"),
            ("terminal", "Máy POS", "id", "FK store"),
            ("sellable_item", "Hàng bán / map item", "id", "FK store; item_id"),
            ("price_list", "Bảng giá", "id", "FK tenant"),
            ("price_list_item", "Giá theo SP", "id", "FK price_list, item"),
            ("recipe", "Định mức trừ kho", "id", "FK sellable_item"),
            ("recipe_line", "NVL của món", "id", "FK recipe; item_id"),
            ("cash_shift", "Ca quỹ", "id", "FK store, terminal, user"),
            ("pos_order", "Hóa đơn bán", "id", "FK shift"),
            ("pos_order_line", "Dòng bán", "id", "FK pos_order"),
            ("pos_payment", "Thanh toán", "id", "FK pos_order"),
        ]
    )
    a += [
        "Trừ tồn: giao dịch đồng bộ với `inv` (API) + event `PosSaleCompleted`.",
        "",
        "---",
        "",
        "## 6. Schema `prt`",
        "",
    ]
    a += ent_table(
        [
            ("portal_account", "Tài khoản cổng", "id", "FK tenant; link customer/vendor/user"),
            ("portal_role", "Vai trò cổng", "id", "FK tenant"),
            ("portal_notification", "Thông báo portal", "id", "FK account"),
            ("self_service_ticket", "Ticket KH tạo", "id", "→ FSM/CRM ref"),
            ("portal_document_share", "Chia sẻ chứng từ", "id", "doc ref + ACL"),
        ]
    )
    a += [
        "Portal **không** nhân bản đơn hàng/công nợ — đọc projection từ CRM/FIN/LOG theo quyền.",
        "",
        "---",
        "",
        "## 7. Index gợi ý",
        "",
        "| Bảng | Index |",
        "|---|---|",
        "| hrm.employee | `uq(tenant_id, employee_code)`; `ix(tenant_id, org_unit_id, status)` |",
        "| hrm.leave_request | `ix(tenant_id, status, from_date)` |",
        "| crm.customer | `uq(tenant_id, code)`; `ix(tenant_id, tax_code)` |",
        "| crm.sales_order | `uq(tenant_id, doc_no)`; `ix(tenant_id, status, order_date)` |",
        "| pos.pos_order | `ix(shift_id)`; `ix(tenant_id, created_at)` |",
        "",
        "---",
        "",
        "*Hết DDD-03-v1.0.*",
        "",
    ]
    return a


# =============================================================================
# DDD-04 PUR INV LOG MFG
# =============================================================================
def doc_04() -> list[str]:
    a: list[str] = []
    a += header(
        "DDD-04-v1.0",
        "Mô hình dữ liệu chuỗi cung ứng (PUR, INV, LOG, MFG)",
        "Logical Data Model — Supply Chain",
    )
    a += [
        "## 1. Giới thiệu",
        "",
        "Thiết kế schema **`pur`**, **`inv`**, **`log`**, **`mfg`** — mua hàng, tồn kho, giao vận, sản xuất.",
        "",
        "---",
        "",
        "## 2. Schema `inv` (master hàng & tồn)",
        "",
        "### 2.1. Sơ đồ",
        "",
        "```text",
        " item ── uom_conversion",
        " warehouse ── bin_location",
        " stock_balance (item, warehouse, bin, lot)",
        " stock_document ── stock_document_line",
        " reservation ── reservation_line",
        " lot / serial",
        "```",
        "",
        "### 2.2. Thực thể",
        "",
    ]
    a += ent_table(
        [
            ("item", "Hàng hóa / NVL / TP / DV", "id", "FK tenant"),
            ("item_category", "Nhóm hàng", "id", "FK tenant"),
            ("uom", "Đơn vị tính", "id", "FK tenant"),
            ("uom_conversion", "Quy đổi ĐVT", "id", "FK item"),
            ("warehouse", "Kho", "id", "FK tenant, org"),
            ("bin_location", "Vị trí trong kho", "id", "FK warehouse"),
            ("stock_balance", "Tồn hiện tại", "id", "UQ item+wh+bin+lot"),
            ("lot", "Lô", "id", "FK item"),
            ("serial_no", "Serial", "id", "FK item; unique"),
            ("stock_document", "Chứng từ kho", "id", "type In/Out/Transfer/Adjust"),
            ("stock_document_line", "Dòng CT kho", "id", "FK document, item"),
            ("reservation", "Giữ hàng", "id", "source SO/WO…"),
            ("reservation_line", "Dòng giữ", "id", "FK reservation"),
            ("stock_count", "Kiểm kê", "id", "FK warehouse"),
            ("stock_count_line", "Dòng kiểm kê", "id", "FK count"),
        ]
    )
    a += [
        "### 2.3. `inv.stock_balance`",
        "",
    ]
    a += col_table(
        [
            ("id", "UUID", "YES", "PK"),
            ("tenant_id", "UUID", "YES", ""),
            ("item_id", "UUID", "YES", "FK item"),
            ("warehouse_id", "UUID", "YES", "FK warehouse"),
            ("bin_id", "UUID", "NO", "FK bin"),
            ("lot_id", "UUID", "NO", "FK lot"),
            ("qty_on_hand", "numeric(18,6)", "YES", "Tồn thực"),
            ("qty_reserved", "numeric(18,6)", "YES", "Đã giữ"),
            ("qty_available", "numeric(18,6)", "YES", "Generated/computed"),
        ]
    )
    a += [
        "> Cập nhật tồn trong transaction cùng `stock_document` post; dùng khóa hàng (row lock) theo `(item, warehouse)`.",
        "",
        "---",
        "",
        "## 3. Schema `pur`",
        "",
    ]
    a += ent_table(
        [
            ("vendor", "Nhà cung cấp", "id", "FK tenant"),
            ("vendor_contact", "Liên hệ NCC", "id", "FK vendor"),
            ("vendor_item_price", "Giá mua", "id", "FK vendor, item"),
            ("purchase_requisition", "PR", "id", "FK tenant"),
            ("purchase_requisition_line", "Dòng PR", "id", "FK PR, item"),
            ("rfq", "Yêu cầu báo giá", "id", "FK tenant"),
            ("rfq_vendor", "NCC được mời", "id", "FK rfq, vendor"),
            ("purchase_order", "PO", "id", "FK vendor"),
            ("purchase_order_line", "Dòng PO", "id", "FK PO, item"),
            ("goods_receipt", "GRN", "id", "FK PO optional"),
            ("goods_receipt_line", "Dòng GRN", "id", "FK GRN → INV receive"),
            ("purchase_invoice_match", "Khớp hóa đơn", "id", "FK PO/GRN/FIN ref"),
        ]
    )
    a += [
        "---",
        "",
        "## 4. Schema `log`",
        "",
    ]
    a += ent_table(
        [
            ("carrier", "Đơn vị vận chuyển", "id", "FK tenant"),
            ("vehicle", "Phương tiện", "id", "FK tenant"),
            ("driver", "Tài xế", "id", "FK tenant / employee ref"),
            ("shipment", "Chuyến/lô giao", "id", "FK SO/source"),
            ("shipment_line", "Dòng giao", "id", "FK shipment, item"),
            ("shipment_stop", "Điểm dừng", "id", "FK shipment"),
            ("shipment_tracking", "Mốc tracking", "id", "FK shipment"),
            ("cod_collection", "Thu COD", "id", "FK shipment → FIN"),
            ("delivery_proof", "Biên bản giao", "id", "FK shipment; file_id"),
        ]
    )
    a += [
        "Xuất kho giao: tạo `inv.stock_document` type Out gắn `shipment_id`.",
        "",
        "---",
        "",
        "## 5. Schema `mfg`",
        "",
    ]
    a += ent_table(
        [
            ("bom_header", "Định mức NVL (BOM)", "id", "FK item TP"),
            ("bom_line", "Dòng BOM", "id", "FK bom, item NVL"),
            ("routing", "Quy trình SX", "id", "FK item"),
            ("routing_operation", "Công đoạn", "id", "FK routing"),
            ("production_plan", "Kế hoạch", "id", "FK tenant"),
            ("work_order", "Lệnh SX", "id", "FK item, bom"),
            ("work_order_material", "NVL cấp cho LSX", "id", "FK WO, item"),
            ("work_order_operation", "Tiến độ công đoạn", "id", "FK WO"),
            ("work_order_output", "TP nhập", "id", "FK WO → INV receive"),
            ("qc_inspection", "Kiểm chất lượng", "id", "FK WO/lot"),
            ("qc_defect", "Lỗi QC", "id", "FK inspection"),
        ]
    )
    a += [
        "### 5.1. `mfg.work_order` — cột trọng yếu",
        "",
    ]
    a += col_table(
        [
            ("id", "UUID", "YES", "PK"),
            ("tenant_id", "UUID", "YES", ""),
            ("doc_no", "varchar(50)", "YES", ""),
            ("item_id", "UUID", "YES", "Thành phẩm"),
            ("bom_id", "UUID", "NO", ""),
            ("qty_planned", "numeric(18,6)", "YES", ""),
            ("qty_completed", "numeric(18,6)", "YES", "Mặc định 0"),
            ("status", "varchar(30)", "YES", "Planned/Released/Done…"),
            ("warehouse_id", "UUID", "NO", "Kho NVL/TP"),
            ("due_date", "date", "NO", ""),
        ]
    )
    a += [
        "---",
        "",
        "## 6. Liên kết xuyên module (Cung ứng)",
        "",
        "| Từ | Đến | Cơ chế |",
        "|---|---|---|",
        "| PUR GRN posted | INV stock in | API đồng bộ + event |",
        "| CRM SO confirmed | INV reservation | API đồng bộ |",
        "| LOG dispatch | INV stock out | API + event |",
        "| MFG release | INV issue NVL | API + event |",
        "| MFG complete | INV receive TP | API + event |",
        "| INV adjust | FIN (optional) | Event giá trị kho |",
        "",
        "---",
        "",
        "## 7. Index & toàn vẹn tồn",
        "",
        "| Đối tượng | Gợi ý |",
        "|---|---|",
        "| stock_balance | `uq(tenant, item, warehouse, bin, lot)` |",
        "| stock_document | `uq(tenant, doc_no)`; `ix(status, doc_date)` |",
        "| reservation | `ix(source_type, source_id)` |",
        "| serial_no | `uq(tenant, serial)` |",
        "| Concurrent update | `UPDATE … WHERE qty_available >= :q` + row lock |",
        "",
        "---",
        "",
        "*Hết DDD-04-v1.0.*",
        "",
    ]
    return a


# =============================================================================
# DDD-05 FIN AST FSM PJM BI
# =============================================================================
def doc_05() -> list[str]:
    a: list[str] = []
    a += header(
        "DDD-05-v1.0",
        "Mô hình dữ liệu tài chính & vận hành (FIN, AST, FSM, PJM, BI)",
        "Logical Data Model — Finance & Operations",
    )
    a += [
        "## 1. Giới thiệu",
        "",
        "Thiết kế schema **`fin`**, **`ast`**, **`fsm`**, **`pjm`**, **`bi`**.",
        "",
        "---",
        "",
        "## 2. Schema `fin`",
        "",
        "### 2.1. Sơ đồ",
        "",
        "```text",
        " account (COA) ── fiscal_period",
        " journal_entry ── journal_line",
        " ar_invoice / ar_receipt",
        " ap_invoice / ap_payment",
        " cash_book / bank_account / bank_txn",
        " tax_code / tax_txn",
        "```",
        "",
        "### 2.2. Thực thể",
        "",
    ]
    a += ent_table(
        [
            ("account", "Tài khoản KT", "id", "FK tenant; code unique"),
            ("fiscal_year", "Năm tài chính", "id", "FK tenant"),
            ("fiscal_period", "Kỳ KT", "id", "FK fiscal_year; status Open/Closed"),
            ("journal_entry", "Chứng từ ghi sổ", "id", "FK period"),
            ("journal_line", "Dòng Nợ/Có", "id", "FK journal, account"),
            ("ar_invoice", "Hóa đơn phải thu", "id", "FK customer ref"),
            ("ar_invoice_line", "Dòng HĐ", "id", "FK ar_invoice"),
            ("ar_receipt", "Phiếu thu", "id", "FK customer"),
            ("ar_allocation", "Khớp thu–hóa đơn", "id", "FK receipt, invoice"),
            ("ap_invoice", "Hóa đơn phải trả", "id", "FK vendor ref"),
            ("ap_payment", "Phiếu chi NCC", "id", "FK vendor"),
            ("cash_book", "Sổ quỹ", "id", "FK org"),
            ("bank_account", "TK ngân hàng", "id", "FK tenant"),
            ("bank_transaction", "Giao dịch NH", "id", "FK bank_account"),
            ("tax_code", "Mã thuế", "id", "FK tenant"),
            ("cost_center", "Trung tâm CP", "id", "FK tenant"),
        ]
    )
    a += [
        "### 2.3. Ràng buộc FIN",
        "- Mỗi `journal_entry` posted: tổng Nợ = tổng Có.",
        "- Không post vào `fiscal_period` Closed (trừ chứng từ điều chỉnh có quyền).",
        "- Nguồn module (HRM payroll, POS sale…) lưu `source_module`, `source_doc_id` trên journal/AR/AP.",
        "",
        "### 2.4. `fin.journal_line`",
        "",
    ]
    a += col_table(
        [
            ("id", "UUID", "YES", "PK"),
            ("journal_id", "UUID", "YES", "FK journal_entry"),
            ("line_no", "int", "YES", ""),
            ("account_id", "UUID", "YES", "FK account"),
            ("debit", "numeric(18,2)", "YES", "≥ 0"),
            ("credit", "numeric(18,2)", "YES", "≥ 0"),
            ("cost_center_id", "UUID", "NO", ""),
            ("partner_type", "varchar(20)", "NO", "Customer/Vendor/Employee"),
            ("partner_id", "UUID", "NO", "Ref mềm"),
            ("memo", "varchar(500)", "NO", ""),
        ]
    )
    a += [
        "---",
        "",
        "## 3. Schema `ast`",
        "",
    ]
    a += ent_table(
        [
            ("asset_category", "Nhóm tài sản", "id", "FK tenant"),
            ("asset", "Tài sản cố định", "id", "FK category; employee custodian"),
            ("asset_component", "Bộ phận TS", "id", "FK asset"),
            ("asset_acquisition", "Ghi tăng", "id", "FK asset; PUR/FIN ref"),
            ("depreciation_run", "Đợt KH", "id", "FK period"),
            ("depreciation_line", "Dòng KH", "id", "FK run, asset → FIN"),
            ("asset_transfer", "Điều chuyển TS", "id", "FK asset"),
            ("asset_disposal", "Thanh lý", "id", "FK asset"),
            ("asset_maintenance", "Bảo trì", "id", "FK asset"),
        ]
    )
    a += [
        "---",
        "",
        "## 4. Schema `fsm`",
        "",
    ]
    a += ent_table(
        [
            ("service_contract", "HĐ dịch vụ/BH", "id", "FK customer"),
            ("ticket", "Ticket", "id", "FK customer; PRT/CRM"),
            ("work_order", "Phiếu kỹ thuật", "id", "FK ticket"),
            ("work_order_part", "Linh kiện dùng", "id", "FK WO; item → INV"),
            ("work_order_time", "Giờ công KT", "id", "FK WO; technician"),
            ("sla_policy", "Chính sách SLA", "id", "FK tenant"),
            ("technician_skill", "Kỹ năng KT", "id", "FK user/employee"),
            ("appointment", "Lịch hẹn", "id", "FK WO"),
        ]
    )
    a += [
        "---",
        "",
        "## 5. Schema `pjm`",
        "",
    ]
    a += ent_table(
        [
            ("project", "Dự án", "id", "FK customer/contract ref"),
            ("project_member", "Thành viên", "id", "FK project, employee"),
            ("wbs_node", "Cấu trúc WBS", "id", "FK project"),
            ("task", "Công việc", "id", "FK wbs/project"),
            ("task_dependency", "Phụ thuộc task", "id", "FK task"),
            ("project_budget", "Ngân sách", "id", "FK project"),
            ("project_cost_actual", "Chi phí thực", "id", "source INV/FIN/HR"),
            ("project_milestone", "Mốc", "id", "FK project"),
            ("change_request", "CR dự án", "id", "FK project → WF"),
            ("project_document", "Tài liệu DA", "id", "FK file_object"),
        ]
    )
    a += [
        "---",
        "",
        "## 6. Schema `bi` (metadata)",
        "",
        "> BI Phase 1 lưu **metadata & phân quyền dataset**; dữ liệu phân tích có thể là view/materialized view hoặc kho riêng.",
        "",
    ]
    a += ent_table(
        [
            ("dataset", "Đăng ký dataset", "id", "module nguồn + license"),
            ("dataset_field", "Trường dataset", "id", "FK dataset"),
            ("dashboard", "Dashboard", "id", "FK tenant"),
            ("dashboard_widget", "Widget", "id", "FK dashboard, dataset"),
            ("report_definition", "Định nghĩa báo cáo", "id", "FK tenant"),
            ("report_schedule", "Lịch gửi báo cáo", "id", "FK report"),
            ("report_run_log", "Lịch sử chạy", "id", "FK report"),
            ("bi_acl", "Quyền xem dataset/dashboard", "id", "FK role/user"),
        ]
    )
    a += [
        "---",
        "",
        "## 7. Liên kết tài chính xuyên module",
        "",
        "| Nguồn | Đích FIN | Khóa truy vết |",
        "|---|---|---|",
        "| PosSaleCompleted / ShiftClosed | AR/Doanh thu/Quỹ | source_doc_id |",
        "| CrmSalesOrder + Delivery | AR Invoice | correlation_id |",
        "| PurGoodsReceived + Invoice | AP | PO/GRN ids |",
        "| HrmPayrollPosted | Journal chi phí lương | payroll_period_id |",
        "| AstDepreciationPosted | Journal KH | depreciation_run_id |",
        "| FsmWorkOrderClosed | AR phí DV | work_order_id |",
        "",
        "---",
        "",
        "*Hết DDD-05-v1.0.*",
        "",
    ]
    return a


# =============================================================================
# DDD-06 Physical
# =============================================================================
def doc_06() -> list[str]:
    a: list[str] = []
    a += header(
        "DDD-06-v1.0",
        "Thiết kế vật lý, bảo mật dữ liệu & vận hành CSDL",
        "Physical Design, Security & Database Operations",
    )
    a += [
        "## 1. Giới thiệu",
        "",
        "Hướng dẫn **hiện thực hóa** mô hình logic: kiểu dữ liệu, index, phân vùng, bảo mật, migration, backup/DR.",
        "",
        "---",
        "",
        "## 2. Ánh xạ kiểu dữ liệu",
        "",
        "| Logic | PostgreSQL | SQL Server |",
        "|---|---|---|",
        "| UUID PK | `uuid` | `uniqueidentifier` |",
        "| Chuỗi ngắn | `varchar(n)` | `nvarchar(n)` |",
        "| Văn bản | `text` | `nvarchar(max)` |",
        "| Số tiền | `numeric(18,2)` | `decimal(18,2)` |",
        "| Số lượng | `numeric(18,6)` | `decimal(18,6)` |",
        "| Boolean | `boolean` | `bit` |",
        "| JSON | `jsonb` | `nvarchar(max)` + JSON |",
        "| Thời điểm | `timestamptz` | `datetimeoffset` |",
        "| Ngày | `date` | `date` |",
        "",
        "---",
        "",
        "## 3. Chiến lược index",
        "",
        "### 3.1. Bắt buộc",
        "- PK clustered/heap theo engine.",
        "- `uq(tenant_id, code/doc_no)` trên master & chứng từ.",
        "- `ix(tenant_id, status, doc_date DESC)` trên chứng từ nóng.",
        "",
        "### 3.2. Theo tải",
        "| Vùng | Index bổ sung |",
        "|---|---|",
        "| Tồn kho | `(tenant_id, warehouse_id, item_id)` trên balance & lines |",
        "| POS | `(shift_id)`, `(tenant_id, created_at)` |",
        "| Audit | BRIN/partition theo tháng + `(entity_type, entity_id)` |",
        "| Outbox | `(status, created_at)` partial `WHERE status='New'` |",
        "",
        "### 3.3. Cấm",
        "- Index hóa mọi cột \"cho chắc\".",
        "- Unique toàn cục quên `tenant_id`.",
        "",
        "---",
        "",
        "## 4. Phân vùng & lưu trữ",
        "",
        "| Bảng | Chiến lược Phase 1–2 |",
        "|---|---|",
        "| audit_log, login_log, notification_log | Partition theo tháng |",
        "| integration_outbox (published cũ) | Archive sau N ngày |",
        "| pos_order | Partition theo tháng nếu > triệu dòng/tháng |",
        "| stock_document_line | Theo năm nếu cần |",
        "",
        "File binary: **object storage** (S3/MinIO/Azure Blob); DB chỉ metadata.",
        "",
        "---",
        "",
        "## 5. Bảo mật dữ liệu",
        "",
        "| Hạng mục | Yêu cầu |",
        "|---|---|",
        "| Transport | TLS tới DB |",
        "| Mật khẩu | Hash (Argon2/bcrypt); không lưu plaintext |",
        "| PII / lương / CCCD | Mã hóa cột hoặc vault; mask ở API |",
        "| Quyền DB | App dùng role least privilege; migration dùng role riêng |",
        "| RLS | Khuyến nghị `tenant_id` policy (PostgreSQL) với tenant lớn |",
        "| Backup | Mã hóa backup; kiểm soát truy cập |",
        "",
        "---",
        "",
        "## 6. Migration & phiên bản schema",
        "",
        "1. Công cụ: Flyway / Liquibase / EF Migrations / Alembic — **chốt 1** khi vào Source.",
        "2. Mỗi module có thư mục migration riêng theo schema.",
        "3. Migration **expand/contract**: thêm cột nullable → backfill → ràng buộc.",
        "4. Không sửa migration đã chạy trên môi trường chung; tạo migration mới.",
        "5. Seed permission/sequence theo module khi enable license.",
        "",
        "---",
        "",
        "## 7. Tính toàn vẹn giao dịch",
        "",
        "| Tình huống | Mức cô lập gợi ý |",
        "|---|---|",
        "| Post tồn / reserve | Read Committed + row lock rõ |",
        "| Post journal | Transaction; check Nợ=Có trước commit |",
        "| Outbox write | Cùng TX với aggregate nguồn |",
        "| Báo cáo nặng | Read replica / snapshot |",
        "",
        "---",
        "",
        "## 8. Backup, HA, DR",
        "",
        "| Hạng mục | Gợi ý tối thiểu production |",
        "|---|---|",
        "| RPO | ≤ 15 phút (WAL/PITR) |",
        "| RTO | Theo SLA khách (ví dụ ≤ 4 giờ) |",
        "| HA | Primary + standby đồng bộ/gần đồng bộ |",
        "| Kiểm thử restore | Định kỳ hàng quý |",
        "",
        "---",
        "",
        "## 9. Quan sát DB",
        "",
        "- Chậm query: `pg_stat_statements` / Query Store.",
        "- Bloat/vacuum; cảnh báo dung lượng.",
        "- Monitor lag replication & outbox depth (khớp INT-05).",
        "",
        "---",
        "",
        "## 10. Checklist nghiệm thu thiết kế CSDL",
        "",
        "1. Đủ schema 16 module (có thể empty schema nếu chưa mua).",
        "2. Mọi bảng nghiệp vụ có cột chuẩn DDD-01.",
        "3. Unique `(tenant_id, code/doc_no)` đúng.",
        "4. Soft-delete & audit tối thiểu.",
        "5. Outbox/Inbox có unique idempotency.",
        "6. Không FK cứng bắt buộc tới module soft-dependency.",
        "7. Script migration chạy clean trên DB trống.",
        "8. Tài liệu DDD khớp SRS entity & INT event (mẫu kiểm tra).",
        "",
        "---",
        "",
        "## 11. Truy vết",
        "",
        "| Liên quan | Tài liệu |",
        "|---|---|",
        "| Chuẩn | `00_CHUAN_TAI_LIEU_DDD.md` |",
        "| Logic module | DDD-02 … DDD-05 |",
        "| Tích hợp | `../02. Tích hợp liên module` |",
        "| SRS | `../01. Modules` |",
        "",
        "---",
        "",
        "*Hết DDD-06-v1.0.*",
        "",
    ]
    return a


def export_docx(md_path: Path) -> None:
    out = md_path.with_suffix(".docx")
    meta = extract_meta_from_md(md_path)
    meta["org"] = "ERP MODULAR PRODUCT"
    meta["status"] = "Chờ duyệt — Database Design"
    meta["classification"] = "Nội bộ dự án — Solution / DBA"
    meta["history"] = [
        ["1.0", TODAY, "Solution / DBA", "Khởi tạo bộ Database Design Document", "Chờ duyệt"],
    ]
    build(md_path, out, meta=meta)


def main() -> None:
    docs = [
        ("DDD-01_Tong_quan_chuan_thiet_ke_CSDL.md", doc_01),
        ("DDD-02_Schema_nen_tang_SYS_WF.md", doc_02),
        ("DDD-03_Schema_nhan_su_thuong_mai.md", doc_03),
        ("DDD-04_Schema_chuoi_cung_ung.md", doc_04),
        ("DDD-05_Schema_tai_chinh_van_hanh.md", doc_05),
        ("DDD-06_Thiet_ke_vat_ly_bao_mat_van_hanh.md", doc_06),
    ]
    for name, fn in docs:
        path = ROOT / name
        write(path, fn())
        export_docx(path)
    print("All DDD docs done.")


if __name__ == "__main__":
    main()
