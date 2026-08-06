# DDD-06-v1.0 — Thiết kế vật lý, bảo mật dữ liệu & vận hành CSDL

> **Physical Design, Security & Database Operations**
> *Database Design Document (DDD)* — ERP bán theo module.
> Phiên bản **1.0** · Ngày 03/08/2026 · Trạng thái: **Chờ duyệt Solution / DBA**.
> Mức thiết kế logic + hướng vật lý. Generic — không gắn khách/ngành cứng.

---

## 0. Thông tin tài liệu

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `DDD-06-v1.0` |
| Tên | Thiết kế vật lý, bảo mật dữ liệu & vận hành CSDL |
| Phiên bản | 1.0 |
| Ngày | 03/08/2026 |
| Phân loại | Thiết kế CSDL (Solution / DBA) |
| Định dạng bàn giao | Microsoft Word (`.docx`) |
| Đầu vào | SRS module v1.1 · INT v1.0 |

| Ver | Ngày | Mô tả | Trạng thái |
|---|---|---|---|
| 1.0 | 03/08/2026 | Khởi tạo bộ Database Design Document | Chờ duyệt |

---

## 1. Giới thiệu

Hướng dẫn **hiện thực hóa** mô hình logic: kiểu dữ liệu, index, phân vùng, bảo mật, migration, backup/DR.

---

## 2. Ánh xạ kiểu dữ liệu

| Logic | PostgreSQL | SQL Server |
|---|---|---|
| UUID PK | `uuid` | `uniqueidentifier` |
| Chuỗi ngắn | `varchar(n)` | `nvarchar(n)` |
| Văn bản | `text` | `nvarchar(max)` |
| Số tiền | `numeric(18,2)` | `decimal(18,2)` |
| Số lượng | `numeric(18,6)` | `decimal(18,6)` |
| Boolean | `boolean` | `bit` |
| JSON | `jsonb` | `nvarchar(max)` + JSON |
| Thời điểm | `timestamptz` | `datetimeoffset` |
| Ngày | `date` | `date` |

---

## 3. Chiến lược index

### 3.1. Bắt buộc
- PK clustered/heap theo engine.
- `uq(tenant_id, code/doc_no)` trên master & chứng từ.
- `ix(tenant_id, status, doc_date DESC)` trên chứng từ nóng.

### 3.2. Theo tải
| Vùng | Index bổ sung |
|---|---|
| Tồn kho | `(tenant_id, warehouse_id, item_id)` trên balance & lines |
| POS | `(shift_id)`, `(tenant_id, created_at)` |
| Audit | BRIN/partition theo tháng + `(entity_type, entity_id)` |
| Outbox | `(status, created_at)` partial `WHERE status='New'` |

### 3.3. Cấm
- Index hóa mọi cột "cho chắc".
- Unique toàn cục quên `tenant_id`.

---

## 4. Phân vùng & lưu trữ

| Bảng | Chiến lược Phase 1–2 |
|---|---|
| audit_log, login_log, notification_log | Partition theo tháng |
| integration_outbox (published cũ) | Archive sau N ngày |
| pos_order | Partition theo tháng nếu > triệu dòng/tháng |
| stock_document_line | Theo năm nếu cần |

File binary: **object storage** (S3/MinIO/Azure Blob); DB chỉ metadata.

---

## 5. Bảo mật dữ liệu

| Hạng mục | Yêu cầu |
|---|---|
| Transport | TLS tới DB |
| Mật khẩu | Hash (Argon2/bcrypt); không lưu plaintext |
| PII / lương / CCCD | Mã hóa cột hoặc vault; mask ở API |
| Quyền DB | App dùng role least privilege; migration dùng role riêng |
| RLS | Khuyến nghị `tenant_id` policy (PostgreSQL) với tenant lớn |
| Backup | Mã hóa backup; kiểm soát truy cập |

---

## 6. Migration & phiên bản schema

1. Công cụ: Flyway / Liquibase / EF Migrations / Alembic — **chốt 1** khi vào Source.
2. Mỗi module có thư mục migration riêng theo schema.
3. Migration **expand/contract**: thêm cột nullable → backfill → ràng buộc.
4. Không sửa migration đã chạy trên môi trường chung; tạo migration mới.
5. Seed permission/sequence theo module khi enable license.

---

## 7. Tính toàn vẹn giao dịch

| Tình huống | Mức cô lập gợi ý |
|---|---|
| Post tồn / reserve | Read Committed + row lock rõ |
| Post journal | Transaction; check Nợ=Có trước commit |
| Outbox write | Cùng TX với aggregate nguồn |
| Báo cáo nặng | Read replica / snapshot |

---

## 8. Backup, HA, DR

| Hạng mục | Gợi ý tối thiểu production |
|---|---|
| RPO | ≤ 15 phút (WAL/PITR) |
| RTO | Theo SLA khách (ví dụ ≤ 4 giờ) |
| HA | Primary + standby đồng bộ/gần đồng bộ |
| Kiểm thử restore | Định kỳ hàng quý |

---

## 9. Quan sát DB

- Chậm query: `pg_stat_statements` / Query Store.
- Bloat/vacuum; cảnh báo dung lượng.
- Monitor lag replication & outbox depth (khớp INT-05).

---

## 10. Checklist nghiệm thu thiết kế CSDL

1. Đủ schema 16 module (có thể empty schema nếu chưa mua).
2. Mọi bảng nghiệp vụ có cột chuẩn DDD-01.
3. Unique `(tenant_id, code/doc_no)` đúng.
4. Soft-delete & audit tối thiểu.
5. Outbox/Inbox có unique idempotency.
6. Không FK cứng bắt buộc tới module soft-dependency.
7. Script migration chạy clean trên DB trống.
8. Tài liệu DDD khớp SRS entity & INT event (mẫu kiểm tra).

---

## 11. Truy vết

| Liên quan | Tài liệu |
|---|---|
| Chuẩn | `00_CHUAN_TAI_LIEU_DDD.md` |
| Logic module | DDD-02 … DDD-05 |
| Tích hợp | `../02. Tích hợp liên module` |
| SRS | `../01. Modules` |

---

*Hết DDD-06-v1.0.*
