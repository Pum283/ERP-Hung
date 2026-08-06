# 02. Tích hợp liên module

Bộ tài liệu mô tả **cách 16 module ERP kết nối với nhau** (sau khi SRS từng module đã có bản v1.1).

**Định dạng bàn giao:** Word (`.docx`) — có bìa, mục lục, số trang, trang ký duyệt.

Ngày cập nhật: 03/08/2026 · Phiên bản bộ tài liệu: **v1.0**

## Mục lục tài liệu

| STT | Mã | Tài liệu Word | Nội dung |
|---:|---|---|---|
| 1 | `INT-01` | [INT-01_Tong_quan_kien_truc_tich_hop.docx](./INT-01_Tong_quan_kien_truc_tich_hop.docx) | Kiến trúc tích hợp, nguyên tắc, Event Bus, chủ sở hữu master |
| 2 | `INT-02` | [INT-02_Ma_tran_phu_thuoc_module.docx](./INT-02_Ma_tran_phu_thuoc_module.docx) | Hard/soft dependency, gói bán E2E, ảnh hưởng tắt license |
| 3 | `INT-03` | [INT-03_Catalog_su_kien_hop_dong_du_lieu.docx](./INT-03_Catalog_su_kien_hop_dong_du_lieu.docx) | Catalog sự kiện, envelope, hợp đồng dữ liệu logic |
| 4 | `INT-04` | [INT-04_Luong_E2E_lien_module.docx](./INT-04_Luong_E2E_lien_module.docx) | 8 journey xuyên module (Lead-to-Cash, P2P, POS, SX…) |
| 5 | `INT-05` | [INT-05_Dong_bo_loi_van_hanh.docx](./INT-05_Dong_bo_loi_van_hanh.docx) | Outbox/Inbox, retry, đối soát, runbook sự cố |

## Chuẩn & công cụ

- Chuẩn viết: [00_CHUAN_TAI_LIEU.md](./00_CHUAN_TAI_LIEU.md)
- Sinh lại MD + DOCX: `_tools/write_int_docs_v1.py`
- Nguồn SRS module: [`../01. Modules/README.md`](../01.%20Modules/README.md)

## Thứ tự đọc gợi ý

1. **INT-01** — nắm nguyên tắc  
2. **INT-02** — chốt gói bán / phụ thuộc  
3. **INT-04** — hiểu journey nghiệp vụ  
4. **INT-03** — chi tiết sự kiện  
5. **INT-05** — vận hành & xử lý lỗi  

## Ghi chú

- Khi mở Word lần đầu: chuột phải Mục lục → **Update Field**.
- Bản `.md` chỉ để chỉnh nội bộ; **không** dùng làm bản giao chính thức.
