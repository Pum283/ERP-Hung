# Chuẩn viết Tài liệu thiết kế cơ sở dữ liệu (Database Design Document)

## Mục tiêu
Mô tả **thiết kế dữ liệu logic & hướng vật lý** của ERP bán theo module — làm cầu nối từ SRS / INT sang triển khai schema, migration và ORM.

## Định dạng bàn giao
- **Chính thức:** Microsoft Word (`.docx`) — bìa, mục lục, số trang, trang ký duyệt.
- Markdown (`.md`) là bản nguồn nội bộ.

## Bộ tài liệu

| Mã | Tên | Nội dung |
|---|---|---|
| `DDD-01` | Tổng quan & chuẩn thiết kế CSDL | Kiến trúc DB, multi-tenant, quy ước đặt tên, cột chuẩn |
| `DDD-02` | Schema nền tảng (SYS, WF) | Identity, RBAC, license, outbox, workflow |
| `DDD-03` | Schema nhân sự & thương mại | HRM, LMS, CRM, POS, PRT |
| `DDD-04` | Schema chuỗi cung ứng | PUR, INV, LOG, MFG |
| `DDD-05` | Schema tài chính & vận hành | FIN, AST, FSM, PJM, BI |
| `DDD-06` | Thiết kế vật lý, bảo mật & vận hành DB | Index, phân vùng, bảo mật, migration, backup |

## Nguyên tắc
1. **Một DB logic / một tenant DB** (Phase 1): chia **schema theo module** (`sys`, `hrm`, `crm`…).
2. Mọi bảng nghiệp vụ có `tenant_id` (trừ danh mục hệ thống toàn cục nếu có).
3. **Soft-delete** mặc định (`is_deleted` / `deleted_at`); hạn chế xóa cứng.
4. Audit tối thiểu: `created_at/by`, `updated_at/by`.
5. FK sang master của module sở hữu; không nhân bản lệch.
6. Khớp với INT-03 (event) và SRS entity — khi lệch phải ghi Change Request.
7. Generic — không hard-code ngành/khách.

## Mức chi tiết
- **Logic:** thực thể, thuộc tính chính, PK/FK, cardinality, trạng thái.
- **Vật lý (hướng dẫn):** kiểu dữ liệu gợi ý, index, phân vùng — chưa phải script SQL cuối cùng của mọi bảng.
