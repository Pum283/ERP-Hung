# Chuẩn viết tài liệu Gói bán hàng

## Mục tiêu
Bộ tài liệu mô tả **cách đóng gói, bán, cấp license và triển khai** ERP theo module — phục vụ Presales, BA, Solution và Delivery. Nối tiếp SRS module (01), phụ thuộc/tích hợp (02) và thiết kế CSDL (04).

## Định dạng bàn giao
- **Chính thức:** Microsoft Word (`.docx`) — bìa, mục lục, số trang, trang ký duyệt.
- Markdown (`.md`) là bản nguồn nội bộ.

## Bộ tài liệu bắt buộc

| Mã | Tên | Nội dung |
|---|---|---|
| `PKG-01` | Chiến lược & nguyên tắc đóng gói | Mô hình bán, vai trò SYS, quy tắc Must/Soft |
| `PKG-02` | Catalog SKU module | 16 module: bán riêng, phụ thuộc, giá trị bán |
| `PKG-03` | Gói giải pháp thương mại | Bundle E2E / ngành / quy mô |
| `PKG-04` | License · quota · thương mại | Hợp đồng, hạn mức, upsell/downsell |
| `PKG-05` | Playbook Presales → Delivery | Checklist bán, demo, kickoff, nghiệm thu gói |

## Nguyên tắc nội dung
1. **Generic** — không gắn 1 khách / 1 ngành cứng; ngành = template gói.
2. **SYS luôn kèm** — không bán độc lập; luôn có trong mọi SKU/gói.
3. **Hard dependency phải có trong báo giá** trước kickoff kỹ thuật.
4. **Soft dependency** nêu rõ Phase 1 / Phase 2 — không “hứa E2E” khi thiếu module.
5. **License-aware** — tắt module ⇒ chặn UI/API; **không xóa dữ liệu**.
6. Truy vết: SRS module v1.1 · INT-02 · DDD license tables.

## Phân biệt với INT-02
- **INT-02** = phụ thuộc kỹ thuật / tích hợp (Solution).
- **PKG-*** = ngôn ngữ **thương mại & triển khai gói** (Presales + Delivery). Có thể trùng ý; PKG là bản bán hàng chính thức.

## Phiên bản
- Bộ Gói bán hàng bắt đầu từ **v1.0**.
- Đổi SKU / bundle / phụ thuộc cứng → tăng phiên bản PKG và ghi lịch sử.
