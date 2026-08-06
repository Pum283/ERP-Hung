# 04. Thiết kế cơ sở dữ liệu (Database Design Document)

Bộ **Database Design Document (DDD)** mô tả thiết kế dữ liệu logic và hướng vật lý cho ERP bán theo module — nối tiếp SRS (01) và Tích hợp liên module (02).

**Định dạng bàn giao:** Word (`.docx`) — bìa, mục lục, số trang, trang ký duyệt.

Ngày cập nhật: 03/08/2026 · Phiên bản: **v1.0**

## Mục lục tài liệu

| STT | Mã | Tài liệu Word | Nội dung |
|---:|---|---|---|
| 0 | `DDD-MASTER` | [DDD-MASTER_Thiet_ke_tong_hop_CSDL.docx](./DDD-MASTER_Thiet_ke_tong_hop_CSDL.docx) | **Tổng hợp:** danh mục mọi bảng + chi tiết từng trường |
| 1 | `DDD-01` | [DDD-01_Tong_quan_chuan_thiet_ke_CSDL.docx](./DDD-01_Tong_quan_chuan_thiet_ke_CSDL.docx) | Kiến trúc DB, schema, multi-tenant, quy ước, cột chuẩn |
| 2 | `DDD-02` | [DDD-02_Schema_nen_tang_SYS_WF.docx](./DDD-02_Schema_nen_tang_SYS_WF.docx) | Bảng SYS + WF, outbox/inbox, RBAC, license |
| 3 | `DDD-03` | [DDD-03_Schema_nhan_su_thuong_mai.docx](./DDD-03_Schema_nhan_su_thuong_mai.docx) | HRM, LMS, CRM, POS, PRT |
| 4 | `DDD-04` | [DDD-04_Schema_chuoi_cung_ung.docx](./DDD-04_Schema_chuoi_cung_ung.docx) | PUR, INV, LOG, MFG |
| 5 | `DDD-05` | [DDD-05_Schema_tai_chinh_van_hanh.docx](./DDD-05_Schema_tai_chinh_van_hanh.docx) | FIN, AST, FSM, PJM, BI |
| 6 | `DDD-06` | [DDD-06_Thiet_ke_vat_ly_bao_mat_van_hanh.docx](./DDD-06_Thiet_ke_vat_ly_bao_mat_van_hanh.docx) | Index, partition, bảo mật, migration, HA/DR |

## Chuẩn & công cụ

- Chuẩn viết: [00_CHUAN_TAI_LIEU_DDD.md](./00_CHUAN_TAI_LIEU_DDD.md)
- Sinh lại bộ DDD-01…06: `_tools/write_ddd_docs_v1.py`
- Sinh lại **DDD-MASTER** (danh mục bảng + chi tiết trường): `_tools/write_ddd_master_v1.py`
- Catalog nguồn MASTER: `_tools/ddd_master_catalog.py`
- Đầu vào: [`../01. Modules`](../01.%20Modules/README.md) · [`../02. Tích hợp liên module`](../02.%20Tích%20hợp%20liên%20module/README.md)

## Thứ tự đọc gợi ý

1. **DDD-MASTER** — tra cứu nhanh mọi bảng / trường  
2. **DDD-01** — chuẩn chung  
3. **DDD-02** — nền tảng (bắt buộc trước mọi module)  
4. **DDD-03 → DDD-05** — theo nhóm module khách mua  
5. **DDD-06** — trước khi viết migration production  

## Ghi chú

- Khi mở Word lần đầu: chuột phải Mục lục → **Update Field**.
- Đây là thiết kế **logic + hướng vật lý**, chưa phải toàn bộ script SQL cuối.
- Bản `.md` chỉ chỉnh nội bộ; **bản giao chính thức là `.docx`**.
