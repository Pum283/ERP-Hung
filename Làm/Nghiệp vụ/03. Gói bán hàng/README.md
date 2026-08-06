# 03. Gói bán hàng

Bộ tài liệu mô tả **cách đóng gói và bán ERP theo module** (SKU module, gói giải pháp, license, playbook Presales/Delivery).

**Định dạng bàn giao:** Word (`.docx`) — bìa, mục lục, số trang, trang ký duyệt.

Ngày cập nhật: 03/08/2026 · Phiên bản bộ tài liệu: **v1.0**

## Mục lục tài liệu

| STT | Mã | Tài liệu Word | Nội dung |
|---:|---|---|---|
| 1 | `PKG-01` | [PKG-01_Chien_luoc_nguyen_tac_dong_goi.docx](./PKG-01_Chien_luoc_nguyen_tac_dong_goi.docx) | Chiến lược bán module, nguyên tắc Must/Soft, vai trò SYS |
| 2 | `PKG-02` | [PKG-02_Catalog_SKU_module.docx](./PKG-02_Catalog_SKU_module.docx) | Catalog 16 module: bán riêng, phụ thuộc, giá trị, đối tượng |
| 3 | `PKG-03` | [PKG-03_Goi_giai_phap_thuong_mai.docx](./PKG-03_Goi_giai_phap_thuong_mai.docx) | Bundle E2E / theo hành trình / theo quy mô |
| 4 | `PKG-04` | [PKG-04_License_quota_thuong_mai.docx](./PKG-04_License_quota_thuong_mai.docx) | License, quota, định giá khung, upsell/downsell |
| 5 | `PKG-05` | [PKG-05_Playbook_Presales_Delivery.docx](./PKG-05_Playbook_Presales_Delivery.docx) | Checklist bán → demo → kickoff → nghiệm thu gói |

## Chuẩn & công cụ

- Chuẩn viết: [00_CHUAN_TAI_LIEU.md](./00_CHUAN_TAI_LIEU.md)
- Sinh lại MD + DOCX: `_tools/write_pkg_docs_v1.py`
- Liên quan: [`../01. Modules`](../01.%20Modules/README.md) · [`../02. Tích hợp liên module`](../02.%20Tích%20hợp%20liên%20module/README.md) · [`../04. Thiết kế CSDL`](../04.%20Thiết%20kế%20CSDL/README.md)

## Thứ tự đọc gợi ý

1. **PKG-01** — nguyên tắc đóng gói  
2. **PKG-02** — từng module bán gì  
3. **PKG-03** — chọn bundle cho khách  
4. **PKG-04** — license & thương mại  
5. **PKG-05** — vận hành bán / triển khai  

## Ghi chú

- Khi mở Word lần đầu: chuột phải Mục lục → **Update Field**.
- Bản `.md` chỉ chỉnh nội bộ; **bản giao chính thức là `.docx`**.
- Giá tiền cụ thể (VND/USD) để trống hoặc “theo bảng giá nội bộ” — không hard-code vào tài liệu này.
